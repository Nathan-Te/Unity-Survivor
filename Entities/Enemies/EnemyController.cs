using UnityEngine;

/// <summary>
/// Main controller for enemies - handles stats, health, and lifecycle.
/// Delegates visual effects, status effects, and combat to specialized components.
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private EnemyData data;

    [Header("Animation (Optionnel)")]
    [SerializeField] private EnemyAnimator enemyAnimator;

    [Header("État (Read Only)")]
    public float currentHp;
    public float currentDamage;
    public float currentSpeed;

    private Rigidbody _rb;
    private Collider _myCollider;
    private int _xpValue;

    public EnemyData Data => data;

    // Sub-components for specialized functionality
    private EnemyStatusEffects _statusEffects;
    private EnemyVisuals _visuals;
    private EnemyRangedCombat _rangedCombat;

    private int _frameIntervalOffset;
    private const int LOGIC_FRAME_INTERVAL = 10; // Execute logic 1 frame out of 10

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _myCollider = GetComponent<Collider>();
        if (_myCollider == null) _myCollider = GetComponentInChildren<Collider>();

        // Find or assign animator
        if (enemyAnimator == null)
            enemyAnimator = GetComponentInChildren<EnemyAnimator>();

        // Get or add sub-components
        _statusEffects = GetComponent<EnemyStatusEffects>();
        if (_statusEffects == null)
            _statusEffects = gameObject.AddComponent<EnemyStatusEffects>();

        _visuals = GetComponent<EnemyVisuals>();
        if (_visuals == null)
            _visuals = gameObject.AddComponent<EnemyVisuals>();

        _rangedCombat = GetComponent<EnemyRangedCombat>();
        if (_rangedCombat == null)
            _rangedCombat = gameObject.AddComponent<EnemyRangedCombat>();

        InitializeStats();

        // Random frame offset to smooth CPU spikes
        _frameIntervalOffset = Random.Range(0, LOGIC_FRAME_INTERVAL);
    }

    private void Update()
    {
        // Time-sliced logic (heavy operations run every 10 frames)
        if ((Time.frameCount + _frameIntervalOffset) % LOGIC_FRAME_INTERVAL == 0)
        {
            float logicDeltaTime = Time.deltaTime * LOGIC_FRAME_INTERVAL;

            // Handle ranged attacks if enemy has projectile
            if (data != null && data.projectilePrefab != null)
            {
                _rangedCombat.UpdateRangedAttack(logicDeltaTime, data);
            }

            // Handle status effects (burn, slow)
            _statusEffects.UpdateStatusEffects(logicDeltaTime);
        }
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
    }

    public void TakeDamage(float amount)
    {
        currentHp -= amount;

        // Spawn damage text
        if (DamageTextPool.Instance != null)
        {
            Vector3 popPos = transform.position + Random.insideUnitSphere * 0.5f;
            DamageTextPool.Instance.Spawn(amount, popPos);
        }

        // Trigger hit flash effect
        _visuals.TriggerHitFlash();

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Notify manager with score value
        int scoreValue = data != null ? data.scoreValue : 10;
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.NotifyEnemyDeath(transform.position, scoreValue);

        // Drop XP gem
        if (GemPool.Instance != null)
            GemPool.Instance.Spawn(transform.position, _xpValue);

        // Return to pool
        if (EnemyPool.Instance != null && data != null && data.prefab != null)
            EnemyPool.Instance.ReturnToPool(gameObject, data.prefab);
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Despawns enemy without dropping XP or notifying events (used for capacity management)
    /// </summary>
    public void SilentDespawn()
    {
        // Unregister from manager
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.UnregisterEnemy(this, _myCollider);

        // Return to pool
        if (EnemyPool.Instance != null && data != null && data.prefab != null)
        {
            EnemyPool.Instance.ReturnToPool(gameObject, data.prefab);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Resets enemy to initial state (called when retrieved from pool)
    /// </summary>
    public void ResetEnemy()
    {
        InitializeStats();

        // Reset physics
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep();
        }

        // Reset animator
        if (enemyAnimator != null)
        {
            Animator anim = enemyAnimator.GetComponent<Animator>();
            if (anim != null)
            {
                anim.Rebind();
            }
        }

        // Reset sub-components
        _visuals.RestoreOriginalMaterials();
        _statusEffects.ResetStatusEffects();
        _rangedCombat.ResetAttackTimer();
    }

    /// <summary>
    /// Applies burn damage over time
    /// </summary>
    public void ApplyBurn(float dps, float duration)
    {
        _statusEffects.ApplyBurn(dps, duration);
    }

    /// <summary>
    /// Applies slow effect
    /// </summary>
    public void ApplySlow(float factor, float duration)
    {
        _statusEffects.ApplySlow(factor, duration);
    }

    private void OnEnable()
    {
        if (EnemyManager.Instance != null)
        {
            // Try to register with manager
            bool isRegistered = EnemyManager.Instance.RegisterEnemy(this, _myCollider);

            // If manager is at capacity, despawn immediately
            if (!isRegistered)
            {
                if (EnemyPool.Instance != null && data != null)
                {
                    EnemyPool.Instance.ReturnToPool(gameObject, data.prefab);
                }
                else
                {
                    Destroy(gameObject);
                }
                return;
            }
        }

        // Show boss health bar if this is a boss
        if (data != null && data.isBoss && BossHealthBarUI.Instance != null)
            BossHealthBarUI.Instance.Show(this);
    }

    private void OnDisable()
    {
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.UnregisterEnemy(this, _myCollider);
    }
}
