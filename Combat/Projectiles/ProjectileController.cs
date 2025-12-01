using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private SpellDefinition _def;
    private GameObject _sourcePrefab;

    private Vector3 _startPosition;
    private int _hitCount;
    private bool _isHostile;

    // Variables Smite
    private float _delayTimer;
    private bool _hasImpacted;

    // Variables Orbit
    private int _index;
    private int _totalCount;
    private float _orbitTimer;
    private float _currentDuration; // Timer local

    public void Initialize(SpellDefinition def, Vector3 direction, int index = 0, int totalCount = 1)
    {
        _def = def;
        _sourcePrefab = def.Form.prefab;
        _startPosition = transform.position;
        _hitCount = 0;
        _isHostile = false;

        _currentDuration = def.Duration;

        _index = index;
        _totalCount = totalCount;
        _orbitTimer = 0f;

        if (_def.Form.tags.HasFlag(SpellTag.Smite))
        {
            _delayTimer = _def.Form.impactDelay;
            _hasImpacted = false;
        }
        else if (_def.Form.tags.HasFlag(SpellTag.Orbit))
        {
            UpdateOrbitPosition();
        }
        else
        {
            transform.forward = direction;
        }

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
    }

    private void Update()
    {
        if (_def == null) return;

        if (_def.Form != null && _def.Form.tags.HasFlag(SpellTag.Smite))
        {
            HandleSmiteBehavior();
        }
        else if (_def.Form != null && _def.Form.tags.HasFlag(SpellTag.Orbit))
        {
            HandleOrbitBehavior();
        }
        else
        {
            HandleStandardProjectile();
        }
    }

    private void HandleStandardProjectile()
    {
        float moveDistance = _def.Speed * Time.deltaTime;
        transform.Translate(Vector3.forward * moveDistance);

        if (_def.IsHoming && !_isHostile)
        {
            Transform target = EnemyManager.Instance.GetTarget(transform.position, 10f, TargetingMode.Nearest, 0, false);
            if (target != null)
            {
                Vector3 dir = (target.position - transform.position).normalized;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 5f * Time.deltaTime);
            }
        }

        if (Vector3.Distance(_startPosition, transform.position) >= _def.Range) Despawn();
    }

    private void HandleSmiteBehavior()
    {
        if (_hasImpacted) return;

        _delayTimer -= Time.deltaTime;
        if (_delayTimer <= 0f)
        {
            _hasImpacted = true;
            ApplyAreaDamage(transform.position);
            Despawn();
        }
    }

    private void HandleOrbitBehavior()
    {
        if (PlayerController.Instance == null) return;

        _orbitTimer += Time.deltaTime;
        UpdateOrbitPosition();

        _currentDuration -= Time.deltaTime;
        if (_currentDuration <= 0f) Despawn();
    }

    private void UpdateOrbitPosition()
    {
        if (PlayerController.Instance == null) return;

        float angleSeparation = 360f / _totalCount;
        float baseAngle = angleSeparation * _index;
        float currentRotation = _orbitTimer * _def.Speed * 40f;

        float finalAngleRad = (baseAngle + currentRotation) * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(finalAngleRad), 0, Mathf.Sin(finalAngleRad)) * 2.0f;

        transform.position = PlayerController.Instance.transform.position + Vector3.up + offset;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_def.Form != null && _def.Form.tags.HasFlag(SpellTag.Smite)) return;

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

        bool isEnemy = EnemyManager.Instance.TryGetEnemyByCollider(other, out EnemyController enemy);
        bool isObstacle = !isEnemy && other.gameObject.layer == LayerMask.NameToLayer("Obstacle");

        if (isEnemy)
        {
            ApplyHit(enemy);
            _hitCount++;

            if (_def.Effect.aoeRadius > 0)
            {
                Despawn();
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
    }

    private void ApplyDamage(EnemyController enemy)
    {
        enemy.TakeDamage(_def.Damage);

        if (_def.Effect.applyBurn) enemy.ApplyBurn(_def.Damage * 0.2f, 3f);
        if (_def.Effect.applySlow) enemy.ApplySlow(0.5f, 2f);

        // CORRECTION ICI : On utilise _def.Knockback (calculé) au lieu de Effect.knockbackForce (base)
        if (_def.Knockback > 0 && enemy.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 pushDir = (enemy.transform.position - transform.position).normalized;
            rb.AddForce(pushDir * _def.Knockback, ForceMode.Impulse);
        }

        // Logique Minion / Chain
        bool isFatal = (enemy.currentHp - _def.Damage) <= 0;
        if (isFatal && _def.MinionChance > 0 && _def.MinionPrefab != null)
        {
            if (Random.value <= _def.MinionChance) Instantiate(_def.MinionPrefab, enemy.transform.position, Quaternion.identity);
        }

        if (_def.ChainCount > 0) HandleChainReaction(enemy);
    }

    private void HandleChainReaction(EnemyController currentTarget)
    {
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
            // Clone manuel rapide
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

    private void Despawn()
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
}