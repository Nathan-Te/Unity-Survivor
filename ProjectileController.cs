using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    // On remplace les floats individuels par la définition complète
    private SpellDefinition _def;
    private GameObject _sourcePrefab;

    private Vector3 _startPosition;
    private int _hitCount;
    private bool _isHostile = false;

    // Nouvelle Initialisation
    public void Initialize(SpellDefinition def, Vector3 direction)
    {
        _def = def;
        _sourcePrefab = def.Form.prefab; // Le prefab vient de la Forme maintenant

        transform.forward = direction;
        _startPosition = transform.position;
        _hitCount = 0;

        // Application de la taille calculée
        transform.localScale = Vector3.one * def.Size;

        // Couleur (Tint) selon l'élément
        if (def.Effect.tintColor != Color.white)
        {
            var renderer = GetComponentInChildren<Renderer>();
            if (renderer) renderer.material.color = def.Effect.tintColor;
        }
    }

    // Version Ennemie (Simplifiée pour l'instant)
    public void InitializeEnemyProjectile(float damage, GameObject sourcePrefab)
    {
        // On crée une définition "fake" pour l'ennemi pour garder la compatibilité
        _def = new SpellDefinition();
        _def.Damage = damage;
        _def.Speed = 10f;
        _def.Range = 30f;
        _def.Effect = ScriptableObject.CreateInstance<SpellEffect>(); // Vide pour éviter null ref

        _sourcePrefab = sourcePrefab;
        _isHostile = true;
        _startPosition = transform.position;
        _hitCount = 0;
    }

    private void Update()
    {
        if (_def == null) return;

        // 1. Mouvement (Vitesse calculée)
        float moveDistance = _def.Speed * Time.deltaTime;
        transform.Translate(Vector3.forward * moveDistance);

        // 2. Homing (Guidage)
        if (_def.IsHoming && !_isHostile)
        {
            Transform target = EnemyManager.Instance.GetTarget(transform.position, 10f, TargetingMode.Nearest, 0, false);
            if (target != null)
            {
                Vector3 dir = (target.position - transform.position).normalized;
                // Rotation douce vers la cible
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 5f * Time.deltaTime);
            }
        }

        // 3. Portée
        if (Vector3.Distance(_startPosition, transform.position) >= _def.Range)
        {
            Despawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isHostile)
        {
            // ... (Logique dégâts joueur inchangée) ...
            if (other.TryGetComponent<PlayerController>(out var player))
            {
                player.SendMessage("TakeDamage", _def.Damage, SendMessageOptions.DontRequireReceiver);
                Despawn();
            }
            else if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
            {
                Despawn(); // Le projectile se détruit contre le mur
            }
            return;
        }

        // Logique Joueur
        bool isEnemy = EnemyManager.Instance.TryGetEnemyByCollider(other, out EnemyController enemy);
        bool isObstacle = !isEnemy && other.gameObject.layer == LayerMask.NameToLayer("Obstacle");

        if (isEnemy)
        {
            // Application des Effets (On Hit)
            ApplyHitEffect(enemy);

            _hitCount++;

            // Gestion du Pierce
            if (_hitCount > _def.Pierce)
            {
                // Si c'est une zone (Nova/Smite/Explosion), on ne despawn pas forcément au contact
                // Mais pour un projectile standard :
                if (_def.Effect.aoeRadius <= 0) Despawn();
            }
        }
        else if (isObstacle)
        {
            // Explosion murale ?
            if (_def.Effect.aoeRadius > 0) ApplyAreaDamage(transform.position);
            Despawn();
        }
    }

    private void ApplyHitEffect(EnemyController target)
    {
        // 1. Dégâts de zone (AOE)
        if (_def.Effect.aoeRadius > 0)
        {
            ApplyAreaDamage(transform.position);
        }
        // 2. Monocible
        else
        {
            ApplyDamage(target);
        }
    }

    private void ApplyAreaDamage(Vector3 center)
    {
        var enemies = EnemyManager.Instance.GetEnemiesInRange(center, _def.Effect.aoeRadius);
        foreach (var e in enemies) ApplyDamage(e);

        // TODO: VFX d'explosion ici
    }

    private void ApplyDamage(EnemyController enemy)
    {
        // Applique les dégâts
        enemy.TakeDamage(_def.Damage);

        // Applique le Knockback
        if (_def.Effect.knockbackForce > 0 && enemy.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 pushDir = (enemy.transform.position - transform.position).normalized;
            rb.AddForce(pushDir * _def.Effect.knockbackForce, ForceMode.Impulse);
        }

        // Applique les status (Burn/Slow) -- À implémenter dans EnemyController
        // if (_def.Effect.applyBurn) enemy.ApplyBurn(...);
    }

    private void Despawn()
    {
        ProjectilePool.Instance.ReturnToPool(gameObject, _sourcePrefab);
    }
}