using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private EnemyData data;

    [Header("État (Read Only)")]
    public float currentHp;
    public float currentDamage;
    public float currentSpeed;

    private Rigidbody _rb;
    private Collider _myCollider; // Référence locale mise en cache
    private int _xpValue;
    public EnemyData Data => data;
    private float _attackTimer;

    // --- STATUTS ---
    private float _burnTimer;
    private float _burnDamagePerSec;
    private float _burnTickTimer; // Pour appliquer les dégâts chaque seconde

    private float _slowTimer;
    private float _originalSpeed;
    private bool _isSlowed;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _myCollider = GetComponent<Collider>(); // On le récupère UNE fois ici
        if (_myCollider == null)
        {
            _myCollider = GetComponentInChildren<Collider>();
        }
        InitializeStats();
    }

    private void Update()
    {
        // Seuls les ennemis avec un projectile attaquent à distance
        if (data != null && data.projectilePrefab != null)
        {
            HandleRangedAttack();
        }

        HandleStatusEffects();
    }

    public void InitializeStats()
    {
        if (data == null)
        {
            currentHp = 10f;
            currentDamage = 5f;
            currentSpeed = 3f;
            _xpValue = 10;
        }
        else
        {
            currentHp = data.baseHp;
            currentDamage = data.baseDamage;
            currentSpeed = data.baseSpeed;
            if (_rb) _rb.mass = data.mass;
            _xpValue = data.xpDropAmount;
        }
        _originalSpeed = currentSpeed;
    }

    private void HandleStatusEffects()
    {
        float dt = Time.deltaTime;

        // GESTION BRÛLURE
        if (_burnTimer > 0)
        {
            _burnTimer -= dt;
            _burnTickTimer += dt;

            if (_burnTickTimer >= 1.0f) // Dégâts toutes les secondes
            {
                TakeDamage(_burnDamagePerSec);
                _burnTickTimer = 0f;
                // TODO: Faire pop un texte de dégâts couleur feu
            }
        }

        // GESTION RALENTISSEMENT
        if (_slowTimer > 0)
        {
            _slowTimer -= dt;
            if (_slowTimer <= 0)
            {
                // Fin du slow
                currentSpeed = _originalSpeed;
                _isSlowed = false;
            }
        }
    }

    public void ApplyBurn(float dps, float duration)
    {
        // On refresh ou on applique si c'est plus fort
        if (_burnTimer <= 0 || dps > _burnDamagePerSec)
        {
            _burnDamagePerSec = dps;
        }
        _burnTimer = duration;
    }

    public void ApplySlow(float factor, float duration)
    {
        if (!_isSlowed)
        {
            _originalSpeed = currentSpeed; // Sauvegarde
            currentSpeed *= factor;        // Application (ex: speed * 0.5)
            _isSlowed = true;
        }
        _slowTimer = duration;
    }

    public void ResetEnemy()
    {
        InitializeStats(); // Remet les PV et dégâts à la valeur du ScriptableObject
                           // Réinitialise la vélocité si nécessaire
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = Vector3.zero; // Attention: linearVelocity pour Unity 6 (anciennement velocity)
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void TakeDamage(float amount)
    {
        currentHp -= amount;
        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Notification au Manager (Pour les POI)
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.NotifyEnemyDeath(this.transform.position);
        }

        // 1. Drop d'XP
        if (GemPool.Instance != null)
        {
            GemPool.Instance.Spawn(transform.position, _xpValue);
        }

        // 2. Retour au Pool (Modifié)
        if (EnemyPool.Instance != null && data != null && data.prefab != null)
        {
            // On passe le prefab d'origine pour savoir dans quelle file le ranger
            EnemyPool.Instance.ReturnToPool(this.gameObject, data.prefab);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void HandleRangedAttack()
    {
        // Si le joueur est mort ou absent, on arrête
        if (PlayerController.Instance == null) return;

        float distSqr = (PlayerController.Instance.transform.position - transform.position).sqrMagnitude;

        // Calculs des seuils (au carré pour la perf)
        float fleeDistSqr = data.fleeDistance * data.fleeDistance;
        float attackRange = data.stopDistance + 2f; // On tire un peu plus loin que la zone d'arrêt
        float attackRangeSqr = attackRange * attackRange;

        // --- NOUVELLE LOGIQUE : PANIQUE ---
        // Si l'ennemi a une distance de fuite configurée (> 0)
        // ET qu'il est actuellement trop proche du joueur (dans la zone de fuite)
        if (data.fleeDistance > 0 && distSqr < fleeDistSqr)
        {
            // Il est en train de fuir (géré par le Job de mouvement), donc il ne peut pas viser/tirer.
            return;
        }

        // --- LOGIQUE DE TIR STANDARD ---
        if (distSqr <= attackRangeSqr)
        {
            _attackTimer += Time.deltaTime;
            if (_attackTimer >= data.attackCooldown)
            {
                Attack();
                _attackTimer = 0f;
            }
        }
        else
        {
            // Optionnel : Si le joueur sort de la portée, on peut reset un peu le timer 
            // pour ne pas tirer instantanément dès qu'il rentre à nouveau (0.5f de réaction)
            _attackTimer = Mathf.Min(_attackTimer, data.attackCooldown * 0.5f);
        }
    }

    private void Attack()
    {
        // Direction vers le joueur
        Vector3 dir = (PlayerController.Instance.transform.position - transform.position).normalized;

        // Utilisation du ProjectilePool (on peut réutiliser le pool du joueur ou en créer un EnemyProjectilePool dédié)
        // Pour faire simple, utilisons le ProjectilePool existant.
        // ATTENTION : Il faut créer un ProjectileController pour l'ennemi qui blesse le PLAYER !

        // Note: Pour ce prototype, assure-toi que le projectilePrefab de l'ennemi a un script qui fait des dégâts au joueur
        // (voir Étape 4 ci-dessous)

        Vector3 spawnPos = transform.position + Vector3.up + dir;
        GameObject proj = ProjectilePool.Instance.Get(data.projectilePrefab, spawnPos, Quaternion.LookRotation(dir));

        // Tu devras adapter ProjectileController pour qu'il puisse être "Hostile"
        if (proj.TryGetComponent<ProjectileController>(out var ctrl))
        {
            // On triche un peu ici en créant un SpellData temporaire pour l'ennemi, 
            // ou alors on surcharge Initialize pour prendre juste des dégâts.
            // Le mieux est de modifier ProjectileController (voir plus bas).
            ctrl.InitializeEnemyProjectile(data.baseDamage, data.projectilePrefab);
        }
    }

    private void OnEnable()
    {
        if (EnemyManager.Instance != null)
        {
            // On passe notre collider pour l'inscription dans le dictionnaire
            EnemyManager.Instance.RegisterEnemy(this, _myCollider);
        }
    }

    private void OnDisable()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterEnemy(this, _myCollider);
        }
    }
}