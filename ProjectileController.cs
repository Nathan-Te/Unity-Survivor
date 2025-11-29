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
    private float _orbitRadius = 2.0f; // Distance du joueur
    private int _index;
    private int _totalCount;
    private float _orbitTimer;

    public void Initialize(SpellDefinition def, Vector3 direction, int index = 0, int totalCount = 1)
    {
        _def = def;
        _sourcePrefab = def.Form.prefab;
        _startPosition = transform.position;
        _hitCount = 0;
        _isHostile = false;

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
            // Pour Orbit, on ignore la direction, la position sera calculée dans Update
            // On peut mettre une position initiale propre pour éviter un flash visuel au centre
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
        // ... (Ton code existant pour ennemi) ...
        _def = new SpellDefinition();
        _def.Damage = damage;
        _def.Speed = 10f;
        _def.Range = 30f;
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
            Despawn(); // Ou jouer VFX d'explosion puis Despawn
        }
    }

    // 3. ORBIT (Bouclier)
    private void HandleOrbitBehavior()
    {
        if (PlayerController.Instance == null) return;

        // On utilise un timer global ou local accumulé pour que la rotation soit fluide
        _orbitTimer += Time.deltaTime;

        UpdateOrbitPosition();

        // Durée de vie
        _def.Duration -= Time.deltaTime;
        if (_def.Duration <= 0f) Despawn();
    }

    private void UpdateOrbitPosition()
    {
        if (PlayerController.Instance == null) return;

        // 1. Calcul de l'angle de base (répartition équitable : 0°, 120°, 240°...)
        float angleSeparation = 360f / _totalCount;
        float baseAngle = angleSeparation * _index;

        // 2. Ajout de la rotation dans le temps
        // Vitesse * 40 pour avoir un chiffre raisonnable dans l'inspecteur
        float currentRotation = _orbitTimer * _def.Speed * 40f;

        float finalAngleRad = (baseAngle + currentRotation) * Mathf.Deg2Rad;

        // 3. Calcul position
        Vector3 offset = new Vector3(Mathf.Cos(finalAngleRad), 0, Mathf.Sin(finalAngleRad)) * 2.0f; // 2.0f = Rayon (tu peux le mettre dans SpellForm si tu veux)

        transform.position = PlayerController.Instance.transform.position + Vector3.up + offset;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Pour le Smite, on ignore les collisions physiques avant l'explosion du timer
        if (_def.Form != null && _def.Form.tags.HasFlag(SpellTag.Smite)) return;

        // ... (Reste de ton code OnTriggerEnter existant : gestion hostile/ennemi/obstacle) ...
        // ... (N'oublie pas d'ajouter l'appel aux status ci-dessous dans ApplyDamage) ...
    }

    private void ApplyDamage(EnemyController enemy)
    {
        enemy.TakeDamage(_def.Damage);

        // --- NOUVEAU : Application des Status ---
        if (_def.Effect.applyBurn)
        {
            // Exemple : 20% des dégâts du coup par seconde pendant 3s
            enemy.ApplyBurn(_def.Damage * 0.2f, 3f);
        }
        if (_def.Effect.applySlow)
        {
            // Ralentissement de 50% pendant 2s
            enemy.ApplySlow(0.5f, 2f);
        }

        // Knockback
        if (_def.Effect.knockbackForce > 0 && enemy.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 pushDir = (enemy.transform.position - transform.position).normalized;
            rb.AddForce(pushDir * _def.Effect.knockbackForce, ForceMode.Impulse);
        }
    }

    // ... (Reste des méthodes : ApplyAreaDamage, Despawn, ApplyColor...)
    private void ApplyColor()
    {
        if (_def.Effect != null && _def.Effect.tintColor != Color.white)
        {
            var rend = GetComponentInChildren<Renderer>();
            if (rend) rend.material.color = _def.Effect.tintColor;
        }
    }

    private void ApplyAreaDamage(Vector3 center)
    {
        // Utilise le rayon de l'effet, ou une valeur par défaut pour Smite
        float radius = _def.Effect.aoeRadius > 0 ? _def.Effect.aoeRadius : 3f;

        var enemies = EnemyManager.Instance.GetEnemiesInRange(center, radius);
        foreach (var e in enemies) ApplyDamage(e);
    }

    private void Despawn()
    {
        ProjectilePool.Instance.ReturnToPool(gameObject, _sourcePrefab);
    }
}