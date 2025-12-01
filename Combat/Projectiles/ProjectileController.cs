using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private SpellDefinition _def;
    private GameObject _sourcePrefab;

    // Stratégie active (Pattern Strategy)
    private IMotionStrategy _motionStrategy;

    private int _hitCount;
    private bool _isHostile;

    public void Initialize(SpellDefinition def, Vector3 direction, int index = 0, int totalCount = 1)
    {
        _def = def;
        _sourcePrefab = def.Form.prefab;
        _hitCount = 0;
        _isHostile = false;

        // 1. Choix de la Stratégie
        if (_def.Form.tags.HasFlag(SpellTag.Smite))
        {
            _motionStrategy = new SmiteMotion(_def.Form.impactDelay);
            // On n'oriente pas le Smite, il est fixe au sol là où il a spawn
        }
        else if (_def.Form.tags.HasFlag(SpellTag.Orbit))
        {
            _motionStrategy = new OrbitMotion(_def.Duration, _def.Speed, index, totalCount);
            // Position initiale immédiate pour éviter le glitch visuel
            _motionStrategy.Update(this, 0f);
        }
        else
        {
            transform.forward = direction;
            _motionStrategy = new LinearMotion(transform.position, _def.Range, _def.Speed, _def.IsHoming, false);
        }

        // 2. Visuels
        transform.localScale = Vector3.one * def.Size;
        if (def.Effect.tintColor != Color.white)
        {
            var renderer = GetComponentInChildren<Renderer>();
            if (renderer) renderer.material.color = def.Effect.tintColor;
        }
    }

    public void InitializeEnemyProjectile(float damage, GameObject sourcePrefab)
    {
        _def = new SpellDefinition();
        _def.Damage = damage;
        _def.Speed = 10f;
        _def.Range = 30f;
        _def.Pierce = 0;
        _def.Effect = ScriptableObject.CreateInstance<SpellEffect>();

        _sourcePrefab = sourcePrefab;
        _isHostile = true;
        _hitCount = 0;

        // Mouvement linéaire simple pour l'ennemi
        transform.forward = transform.forward; // Déjà orienté par l'ennemi
        _motionStrategy = new LinearMotion(transform.position, 30f, 10f, false, true);
    }

    private void Update()
    {
        if (_motionStrategy != null)
        {
            _motionStrategy.Update(this, Time.deltaTime);
        }
    }

    // --- LOGIQUE PUBLIQUE POUR LES STRATÉGIES ---

    public void TriggerSmiteExplosion()
    {
        ApplyAreaDamage(transform.position);
        Despawn();
    }

    public void Despawn()
    {
        if (ProjectilePool.Instance != null)
        {
            ProjectilePool.Instance.ReturnToPool(gameObject, _sourcePrefab);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- COLLISIONS ---

    private void OnTriggerEnter(Collider other)
    {
        // Smite ignore les triggers physiques (c'est le Timer qui déclenche)
        if (_def.Form != null && _def.Form.tags.HasFlag(SpellTag.Smite)) return;

        // GESTION HOSTILE
        if (_isHostile)
        {
            if (other.TryGetComponent<PlayerController>(out var player))
            {
                player.SendMessage("TakeDamage", _def.Damage, SendMessageOptions.DontRequireReceiver);
                Despawn();
            }
            else if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
            {
                Despawn();
            }
            return;
        }

        // GESTION JOUEUR
        bool isEnemy = EnemyManager.Instance.TryGetEnemyByCollider(other, out EnemyController enemy);
        bool isObstacle = !isEnemy && other.gameObject.layer == LayerMask.NameToLayer("Obstacle");

        if (isEnemy)
        {
            ApplyHit(enemy);
            _hitCount++;

            // Logique Pierce / AOE
            if (_def.Effect.aoeRadius > 0)
            {
                Despawn(); // Les AOE explosent au premier contact
            }
            else if (_hitCount > _def.Pierce)
            {
                Despawn();
            }
        }
        else if (isObstacle)
        {
            if (_def.Effect.aoeRadius > 0) ApplyAreaDamage(transform.position);
            Despawn();
        }
    }

    private void ApplyHit(EnemyController target)
    {
        if (_def.Effect.aoeRadius > 0)
        {
            ApplyAreaDamage(transform.position);
        }
        else
        {
            ApplyDamage(target);
        }
    }

    private void ApplyAreaDamage(Vector3 center)
    {
        float radius = _def.Effect.aoeRadius > 0 ? _def.Effect.aoeRadius : 3f;
        var enemies = EnemyManager.Instance.GetEnemiesInRange(center, radius);
        foreach (var e in enemies) ApplyDamage(e);

        // TODO: Instantiate VFX Explosion
    }

    private void ApplyDamage(EnemyController enemy)
    {
        enemy.TakeDamage(_def.Damage);

        if (_def.Effect.applyBurn) enemy.ApplyBurn(_def.Damage * 0.2f, 3f);
        if (_def.Effect.applySlow) enemy.ApplySlow(0.5f, 2f);

        if (_def.Knockback > 0 && enemy.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 pushDir = (enemy.transform.position - transform.position).normalized;
            rb.AddForce(pushDir * _def.Knockback, ForceMode.Impulse);
        }

        // Logique Minion / Chain
        bool isFatal = (enemy.currentHp - _def.Damage) <= 0;
        if (isFatal && _def.MinionChance > 0 && _def.MinionPrefab != null)
        {
            if (Random.value <= _def.MinionChance)
                Instantiate(_def.MinionPrefab, enemy.transform.position, Quaternion.identity);
        }

        if (_def.ChainCount > 0) HandleChainReaction(enemy);
    }

    private void HandleChainReaction(EnemyController currentTarget)
    {
        // Logique Chain inchangée, je la remets pour que le script soit complet
        var candidates = EnemyManager.Instance.GetEnemiesInRange(transform.position, _def.ChainRange);
        EnemyController bestCandidate = null;
        float closestDistSqr = float.MaxValue;

        foreach (var candidate in candidates)
        {
            if (candidate == currentTarget || candidate.currentHp <= 0) continue;
            float dSqr = (candidate.transform.position - currentTarget.transform.position).sqrMagnitude;
            if (dSqr < closestDistSqr)
            {
                closestDistSqr = dSqr;
                bestCandidate = candidate;
            }
        }

        if (bestCandidate != null)
        {
            SpellDefinition chainDef = new SpellDefinition();
            // Copie manuelle simple
            chainDef.Form = _def.Form;
            chainDef.Effect = _def.Effect;
            chainDef.Speed = _def.Speed;
            chainDef.Size = _def.Size * 0.8f;
            chainDef.Range = _def.ChainRange * 1.2f;

            chainDef.Damage = _def.Damage * _def.ChainDamageReduction;
            chainDef.ChainCount = _def.ChainCount - 1;
            chainDef.ChainRange = _def.ChainRange;
            chainDef.ChainDamageReduction = _def.ChainDamageReduction;

            Vector3 spawnPos = currentTarget.transform.position + Vector3.up;
            Vector3 dir = (bestCandidate.transform.position - spawnPos).normalized;

            GameObject p = ProjectilePool.Instance.Get(_def.Form.prefab, spawnPos, Quaternion.LookRotation(dir));
            if (p.TryGetComponent<ProjectileController>(out var ctrl))
            {
                ctrl.Initialize(chainDef, dir);
            }
        }
    }
}