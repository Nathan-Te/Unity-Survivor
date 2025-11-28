using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private float _damage;
    private float _speed;
    private float _range;
    private int _pierceCount;

    private float _explosionRadius;
    private GameObject _sourcePrefab;

    private Vector3 _startPosition;
    private int _hitCount;

    private bool _isHostile = false;

    // Initialisation appelée par le Spawner/Manager au moment du tir
    public void Initialize(SpellData data, Vector3 direction)
    {
        _damage = data.damage;
        _speed = data.speed;
        _range = data.range;
        _pierceCount = data.pierceCount;
        _explosionRadius = data.explosionRadius;

        transform.forward = direction; // Orientation
        _startPosition = transform.position;
        _hitCount = 0;

        _sourcePrefab = data.projectilePrefab;

        // Appliquer la taille
        transform.localScale = Vector3.one * data.size;
    }

    private void Update()
    {
        // Mouvement simple (Translate est très rapide pour des projectiles droits)
        float moveDistance = _speed * Time.deltaTime;
        transform.Translate(Vector3.forward * moveDistance);

        // Vérification de la portée max
        if (Vector3.Distance(_startPosition, transform.position) >= _range)
        {
            Despawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isHostile)
        {
            if (other.TryGetComponent<PlayerController>(out var player))
            {
                // Il faudrait ajouter une méthode TakeDamage publique sur PlayerController
                // player.TakeDamage(_damage); <-- À AJOUTER DANS PLAYER

                // Hack pour l'instant si la méthode est privée :
                player.SendMessage("TakeDamage", _damage, SendMessageOptions.DontRequireReceiver);

                Despawn();
            }
            return;
        }

        // On vérifie si c'est un ennemi ou un obstacle
        bool isEnemy = EnemyManager.Instance.TryGetEnemyByCollider(other, out EnemyController directHitEnemy);
        // Note: Assure-toi que le layer "Obstacle" est bien défini dans ton projet
        bool isObstacle = !isEnemy && other.gameObject.layer == LayerMask.NameToLayer("Obstacle");

        if (isEnemy || isObstacle)
        {
            // CAS 1 : C'EST UNE EXPLOSION (AOE)
            if (_explosionRadius > 0)
            {
                Explode();
                Despawn(); // Une explosion détruit le projectile immédiatement
            }
            // CAS 2 : TIR DIRECT (Monocible / Perçant)
            else if (isEnemy)
            {
                directHitEnemy.TakeDamage(_damage);
                _hitCount++;
                if (_hitCount > _pierceCount) Despawn();
            }
            else if (isObstacle)
            {
                Despawn();
            }
        }
    }

    public void InitializeEnemyProjectile(float damage, GameObject sourcePrefab)
    {
        _damage = damage;
        _speed = 10f; // Vitesse par défaut ennemi
        _range = 30f;
        _pierceCount = 0;
        _explosionRadius = 0;
        _sourcePrefab = sourcePrefab;
        _isHostile = true; // Marque comme hostile

        _startPosition = transform.position;
        _hitCount = 0;
    }

    private void Explode()
    {
        // On demande à l'EnemyManager de trouver tous les ennemis dans la zone
        // C'est plus performant que Physics.OverlapSphere car on utilise la liste déjà en mémoire
        // et on évite les allocations de GC (Garbage Collector)
        var enemiesHit = EnemyManager.Instance.GetEnemiesInRange(transform.position, _explosionRadius);

        foreach (var enemy in enemiesHit)
        {
            enemy.TakeDamage(_damage);
        }

        // TODO OPTIONNEL : Instancier un VFX d'explosion ici
        // ParticleManager.Instance.PlayExplosion(transform.position, _explosionRadius);
    }

    private void Despawn()
    {
        ProjectilePool.Instance.ReturnToPool(gameObject, _sourcePrefab);
    }
}