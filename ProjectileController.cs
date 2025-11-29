using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private SpellDefinition _def;
    private GameObject _sourcePrefab;

    private Vector3 _startPosition;
    private int _hitCount;
    private bool _isHostile;

    // Variables pour Smite (Météore)
    private float _delayTimer;
    private bool _hasImpacted;

    // Variables pour Orbit
    private float _currentAngle;
    private int _index;
    private int _totalCount;
    private float _orbitTimer;

    // --- CORRECTION DURATION ---
    private float _currentDuration;

    public void Initialize(SpellDefinition def, Vector3 direction, int index = 0, int totalCount = 1)
    {
        _def = def;
        _sourcePrefab = def.Form.prefab;
        _startPosition = transform.position;
        _hitCount = 0;
        _isHostile = false;

        // On initialise le timer local
        _currentDuration = def.Duration;

        // Stockage pour la logique Orbit
        _index = index;
        _totalCount = totalCount;
        _orbitTimer = 0f;

        // Initialisation spécifique
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

        // Couleur
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

    // 1. PROJECTILE STANDARD (Bolt / Nova)
    private void HandleStandardProjectile()
    {
        float moveDistance = _def.Speed * Time.deltaTime;
        transform.Translate(Vector3.forward * moveDistance);

        // Homing (Guidage)
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

    // 2. SMITE (Météore / Foudre)
    private void HandleSmiteBehavior()
    {
        if (_hasImpacted) return;

        _delayTimer -= Time.deltaTime;
        if (_delayTimer <= 0f)
        {
            // BOOM !
            _hasImpacted = true;
            ApplyAreaDamage(transform.position); // Smite est toujours une zone
            Despawn();
        }
    }

    // 3. ORBIT (Bouclier)
    private void HandleOrbitBehavior()
    {
        if (PlayerController.Instance == null) return;

        _orbitTimer += Time.deltaTime;
        UpdateOrbitPosition();

        // CORRECTION : On utilise la variable locale
        _currentDuration -= Time.deltaTime;
        if (_currentDuration <= 0f) Despawn();
    }

    private void UpdateOrbitPosition()
    {
        if (PlayerController.Instance == null) return;

        // 1. Calcul de l'angle de base (répartition équitable)
        float angleSeparation = 360f / _totalCount;
        float baseAngle = angleSeparation * _index;

        // 2. Ajout de la rotation dans le temps
        float currentRotation = _orbitTimer * _def.Speed * 40f;

        float finalAngleRad = (baseAngle + currentRotation) * Mathf.Deg2Rad;

        // 3. Calcul position
        Vector3 offset = new Vector3(Mathf.Cos(finalAngleRad), 0, Mathf.Sin(finalAngleRad)) * 2.0f;

        transform.position = PlayerController.Instance.transform.position + Vector3.up + offset;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_def.Form != null && _def.Form.tags.HasFlag(SpellTag.Smite)) return;

        // GESTION ENNEMIE (Hostile)
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

        // GESTION JOUEUR (Allié)
        bool isEnemy = EnemyManager.Instance.TryGetEnemyByCollider(other, out EnemyController enemy);
        bool isObstacle = !isEnemy && other.gameObject.layer == LayerMask.NameToLayer("Obstacle");

        if (isEnemy)
        {
            ApplyHit(enemy);
            _hitCount++;

            // Pierce check
            if (_def.Effect.aoeRadius > 0)
            {
                Despawn();
            }
            // CORRECTION : Le Pierce doit être strict (> permet de toucher Pierce + 1 ennemis)
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

        if (_def.Effect.knockbackForce > 0 && enemy.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 pushDir = (enemy.transform.position - transform.position).normalized;
            rb.AddForce(pushDir * _def.Effect.knockbackForce, ForceMode.Impulse);
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