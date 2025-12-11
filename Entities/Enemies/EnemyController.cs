using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private EnemyData data;

    [Header("Animation (Optionnel)")]
    [SerializeField] private EnemyAnimator enemyAnimator;

    [Header("Visual Effects")]
    [SerializeField] private float hitFlashDuration = 0.1f;
    [SerializeField] private Material hitFlashMaterial; // Matériau blanc pour le blink

    [Header("État (Read Only)")]
    public float currentHp;
    public float currentDamage;
    public float currentSpeed;

    private Rigidbody _rb;
    private Collider _myCollider;
    private int _xpValue;
    public EnemyData Data => data;
    private float _attackTimer;

    // --- HIT FLASH EFFECT ---
    private Renderer _renderer;
    private Material[] _originalMaterials; // Support pour plusieurs matériaux
    private Material[] _flashMaterialsArray; // Array de matériaux de flash
    private float _flashTimer;

    // --- STATUTS ---
    private float _burnTimer;
    private float _burnDamagePerSec;
    private float _burnTickTimer;
    private float _slowTimer;
    private float _originalSpeed;
    private bool _isSlowed;

    private int _frameIntervalOffset;
    private const int LOGIC_FRAME_INTERVAL = 10; // Exécute la logique 1 frame sur 10

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _myCollider = GetComponent<Collider>();
        if (_myCollider == null) _myCollider = GetComponentInChildren<Collider>();

        // Si on n'a pas assigné l'animator manuellement, on essaie de le trouver
        if (enemyAnimator == null) enemyAnimator = GetComponentInChildren<EnemyAnimator>();

        // Initialiser le Renderer pour le Hit Flash
        _renderer = GetComponentInChildren<Renderer>();
        if (_renderer != null)
        {
            // Sauvegarder TOUS les matériaux d'origine (support multi-matériaux)
            _originalMaterials = _renderer.sharedMaterials;

            // Créer un array de matériaux de flash (un pour chaque slot)
            _flashMaterialsArray = new Material[_originalMaterials.Length];

            // Créer le matériau de flash
            Material flashMat;
            if (hitFlashMaterial != null)
            {
                flashMat = new Material(hitFlashMaterial);
            }
            else
            {
                // Créer un matériau blanc par défaut si aucun n'est assigné
                Shader flashShader = Shader.Find("Universal Render Pipeline/Unlit");
                if (flashShader == null) flashShader = Shader.Find("Unlit/Color"); // Fallback Built-in
                if (flashShader == null) flashShader = Shader.Find("Standard"); // Fallback Standard

                flashMat = new Material(flashShader);
                flashMat.color = Color.white;
            }

            // Remplir tous les slots avec le même matériau de flash
            for (int i = 0; i < _flashMaterialsArray.Length; i++)
            {
                _flashMaterialsArray[i] = flashMat;
            }
        }

        InitializeStats();

        // On donne un offset aléatoire pour éviter que tous les ennemis calculent
        // EXACTEMENT à la même frame (lissage du pic CPU)
        _frameIntervalOffset = Random.Range(0, LOGIC_FRAME_INTERVAL);
    }

    private void Update()
    {
        // 1. Gestion du Hit Flash (chaque frame pour une réactivité immédiate)
        if (_flashTimer > 0)
        {
            _flashTimer -= Time.deltaTime;

            if (_flashTimer <= 0)
            {
                // Retour aux matériaux normaux
                if (_renderer != null && _originalMaterials != null)
                {
                    _renderer.materials = _originalMaterials;
                }
            }
        }

        // 2. Logique lourde (Attaque + Statuts) -> Throttled (Time Sliced)
        if ((Time.frameCount + _frameIntervalOffset) % LOGIC_FRAME_INTERVAL == 0)
        {
            // On passe le temps écoulé depuis la dernière exécution logique
            float logicDeltaTime = Time.deltaTime * LOGIC_FRAME_INTERVAL;

            if (data != null && data.projectilePrefab != null)
            {
                HandleRangedAttack(logicDeltaTime);
            }

            HandleStatusEffects(logicDeltaTime);
        }
    }

    public void InitializeStats()
    {
        if (data == null)
        {
            currentHp = 10f; currentDamage = 5f; currentSpeed = 3f; _xpValue = 10;
        }
        else
        {
            currentHp = data.baseHp; currentDamage = data.baseDamage; currentSpeed = data.baseSpeed;
            if (_rb) _rb.mass = data.mass; _xpValue = data.xpDropAmount;
        }
        _originalSpeed = currentSpeed;
    }

    public void SilentDespawn()
    {
        // On désinscrit l'ennemi du Manager proprement
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.UnregisterEnemy(this, _myCollider);

        // On le renvoie au pool sans lâcher d'XP ni d'event
        if (EnemyPool.Instance != null && data != null && data.prefab != null)
        {
            EnemyPool.Instance.ReturnToPool(this.gameObject, data.prefab);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetEnemy()
    {
        InitializeStats();
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep();
        }

        if (enemyAnimator != null)
        {
            Animator anim = enemyAnimator.GetComponent<Animator>();
            if (anim != null)
            {
                anim.Rebind(); // ⭐ Réinitialise complètement l'animator
            }
        }

        // Réinitialiser le Hit Flash
        _flashTimer = 0f;
        if (_renderer != null && _originalMaterials != null)
        {
            _renderer.materials = _originalMaterials;
        }
    }

    public void TakeDamage(float amount)
    {
        currentHp -= amount;
        if (DamageTextPool.Instance != null)
        {
            Vector3 popPos = transform.position + Random.insideUnitSphere * 0.5f;
            DamageTextPool.Instance.Spawn(amount, popPos);
        }

        // --- HIT FLASH EFFECT ---
        TriggerHitFlash();

        if (currentHp <= 0) Die();
    }

    private void TriggerHitFlash()
    {
        if (_renderer != null && _flashMaterialsArray != null)
        {
            _flashTimer = hitFlashDuration;
            _renderer.materials = _flashMaterialsArray;
        }
    }

    private void Die()
    {
        if (EnemyManager.Instance != null) EnemyManager.Instance.NotifyEnemyDeath(transform.position);
        if (GemPool.Instance != null) GemPool.Instance.Spawn(transform.position, _xpValue);

        if (EnemyPool.Instance != null && data != null && data.prefab != null)
            EnemyPool.Instance.ReturnToPool(this.gameObject, data.prefab);
        else
            Destroy(gameObject);
    }

    private void HandleStatusEffects(float dt)
    {
        // On utilise 'dt' (qui vaut ~0.16s) au lieu de Time.deltaTime (~0.016s)

        if (_burnTimer > 0)
        {
            _burnTimer -= dt;
            _burnTickTimer += dt;
            // Si on a accumulé assez de temps pour un tick (1s)
            if (_burnTickTimer >= 1.0f)
            {
                TakeDamage(_burnDamagePerSec);
                _burnTickTimer -= 1.0f; // On retire 1s au lieu de reset à 0 pour garder la précision
            }
        }
        if (_slowTimer > 0)
        {
            _slowTimer -= dt;
            if (_slowTimer <= 0) { currentSpeed = _originalSpeed; _isSlowed = false; }
        }
    }

    public void ApplyBurn(float dps, float duration)
    {
        if (_burnTimer <= 0 || dps > _burnDamagePerSec) _burnDamagePerSec = dps;
        _burnTimer = duration;
    }

    public void ApplySlow(float factor, float duration)
    {
        if (!_isSlowed) { _originalSpeed = currentSpeed; currentSpeed *= factor; _isSlowed = true; }
        _slowTimer = duration;
    }

    // -----------------------------------------------------------
    // GESTION DE L'ATTAQUE SYNCHRONISÉE
    // -----------------------------------------------------------

    private void HandleRangedAttack(float dt)
    {
        if (PlayerController.Instance == null) return;

        float distSqr = (PlayerController.Instance.transform.position - transform.position).sqrMagnitude;
        float fleeDistSqr = data.fleeDistance * data.fleeDistance;
        float attackRange = data.stopDistance + 2f;
        float attackRangeSqr = attackRange * attackRange;

        if (data.fleeDistance > 0 && distSqr < fleeDistSqr) return;

        if (distSqr <= attackRangeSqr)
        {
            _attackTimer += dt; // On ajoute le temps accumulé
            if (_attackTimer >= data.attackCooldown)
            {
                if (enemyAnimator != null) enemyAnimator.TriggerAttackAnimation();
                else SpawnProjectile();

                _attackTimer = 0f;
            }
        }
        else
        {
            // Réduction du timer si hors de portée (plus lente)
            _attackTimer = Mathf.Max(0, _attackTimer - dt);
        }
    }

    // Cette méthode est appelée par l'Animation Event (ou directement si pas d'anim)
    public void SpawnProjectile()
    {
        if (PlayerController.Instance == null) return;

        // Calcul de la direction de tir
        // Note : L'ennemi regarde déjà le joueur grâce au Job System, 
        // mais on recalcule le vecteur pour être précis.
        Vector3 dir = (PlayerController.Instance.transform.position - transform.position).normalized;

        // Point de spawn : On part du centre + un peu devant/haut
        Vector3 spawnPos = transform.position + Vector3.up + dir;

        // Récupération du projectile
        GameObject proj = ProjectilePool.Instance.Get(data.projectilePrefab, spawnPos, Quaternion.LookRotation(dir));

        if (proj.TryGetComponent<ProjectileController>(out var ctrl))
        {
            ctrl.InitializeEnemyProjectile(data.baseDamage, data.projectilePrefab);
        }
    }

    private void OnEnable()
    {
        if (EnemyManager.Instance != null)
        {
            // TENTATIVE D'ENREGISTREMENT
            bool isRegistered = EnemyManager.Instance.RegisterEnemy(this, _myCollider);

            // SÉCURITÉ ANTI-CRASH : Si le manager est plein, on se détruit tout de suite !
            if (!isRegistered)
            {
                // Retour au pool si possible, sinon Destroy
                if (EnemyPool.Instance != null && data != null)
                {
                    EnemyPool.Instance.ReturnToPool(this.gameObject, data.prefab);
                }
                else
                {
                    Destroy(gameObject);
                }
                return; // On arrête tout ici
            }
        }

        if (data != null && data.isBoss && BossHealthBarUI.Instance != null)
            BossHealthBarUI.Instance.Show(this);
    }

    private void OnDisable()
    {
        if (EnemyManager.Instance != null) EnemyManager.Instance.UnregisterEnemy(this, _myCollider);
    }
}