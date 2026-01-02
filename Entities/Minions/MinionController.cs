using UnityEngine;

/// <summary>
/// Controller for ghost minions (Necrotic effect).
/// Ghosts fly in straight lines toward enemies and explode on contact, dealing AOE damage and applying Necrotic mark.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class MinionController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private MinionData data;

    [Header("Runtime Stats (Read Only)")]
    public float currentSpeed;
    public float currentExplosionRadius;
    public float currentExplosionDamage;
    public float currentCritChance;
    public float currentCritDamageMultiplier;

    private Rigidbody _rb;
    private Collider _myCollider;
    private Transform _currentTarget;
    private float _lifetimeTimer;
    private bool _hasExploded = false;
    private GameObject _spawnerEnemy; // Enemy that spawned this ghost (to ignore in collision)
    private float _spawnTime; // Time when ghost spawned (for collision grace period)

    public MinionData Data => data;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _myCollider = GetComponent<Collider>();
        if (_myCollider == null) _myCollider = GetComponentInChildren<Collider>();

        // DEBUG: Log component setup
        Debug.Log($"[MinionController] Awake - GameObject: {gameObject.name}, Layer: {LayerMask.LayerToName(gameObject.layer)}");
        Debug.Log($"[MinionController] Rigidbody: {(_rb != null ? "Found" : "MISSING")}, Collider: {(_myCollider != null ? "Found" : "MISSING")}");

        if (_myCollider != null)
        {
            Debug.Log($"[MinionController] Collider isTrigger: {_myCollider.isTrigger}");
        }

        // Configure Rigidbody for kinematic ghost movement
        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
            Debug.Log($"[MinionController] Rigidbody configured - isKinematic: {_rb.isKinematic}, useGravity: {_rb.useGravity}");
        }

        // CRITICAL FIX: Ensure collider is a trigger for OnTriggerEnter to work
        if (_myCollider != null)
        {
            if (!_myCollider.isTrigger)
            {
                _myCollider.isTrigger = true;
                Debug.LogWarning($"[MinionController] FIXED: Collider isTrigger was false, set to true");
            }
        }
        else
        {
            Debug.LogError($"[MinionController] NO COLLIDER FOUND! OnTriggerEnter will never fire!");
        }

        // CRITICAL FIX: Ensure GameObject is on Minion layer
        int minionLayer = LayerMask.NameToLayer("Minion");
        if (gameObject.layer != minionLayer)
        {
            Debug.LogWarning($"[MinionController] FIXED: Layer was '{LayerMask.LayerToName(gameObject.layer)}', changing to 'Minion'");
            gameObject.layer = minionLayer;
        }
    }

    private void OnEnable()
    {
        InitializeStats();
        _lifetimeTimer = 0f;
        _hasExploded = false;
        _spawnerEnemy = null; // Reset spawner reference on pool reuse
        _spawnTime = Time.time; // Record spawn time for grace period

        // Reset scale to default (prevents pool reuse scale issues)
        transform.localScale = Vector3.one;

        // Find target immediately on spawn (don't wait for Update)
        _currentTarget = FindNearestEnemy();

        Debug.Log($"[MinionController] OnEnable - Reset state, Target: {(_currentTarget != null ? _currentTarget.name : "NONE")}");
    }

    /// <summary>
    /// Initializes ghost stats from MinionData (base stats only, upgrades applied via SetUpgradedStats)
    /// </summary>
    private void InitializeStats()
    {
        if (data == null)
        {
            Debug.LogError("[MinionController] No MinionData assigned!");
            return;
        }

        currentSpeed = data.baseMoveSpeed;
        currentExplosionRadius = data.baseExplosionRadius;
        currentExplosionDamage = data.baseExplosionDamage;
    }

    /// <summary>
    /// Sets upgraded stats from spell bonuses (called by MinionManager on spawn)
    /// </summary>
    public void SetUpgradedStats(float speed, float explosionRadius, float explosionDamage, float critChance, float critDamageMultiplier)
    {
        currentSpeed = speed;
        currentExplosionRadius = explosionRadius;
        currentExplosionDamage = explosionDamage;
        currentCritChance = critChance;
        currentCritDamageMultiplier = critDamageMultiplier;
    }

    /// <summary>
    /// Sets the enemy that spawned this ghost (to ignore in collision)
    /// </summary>
    public void SetSpawnerEnemy(GameObject spawner)
    {
        _spawnerEnemy = spawner;
        Debug.Log($"[MinionController] SetSpawnerEnemy called - Spawner: {(spawner != null ? spawner.name : "NULL")}");
    }

    private void Update()
    {
        // SAFETY: Stop executing if scene is restarting/loading
        if (SingletonGlobalState.IsSceneLoading || SingletonGlobalState.IsApplicationQuitting)
            return;

        // Don't process if game is not playing
        if (GameStateController.Instance != null && !GameStateController.Instance.IsPlaying)
            return;

        // Handle lifetime (despawn after duration)
        _lifetimeTimer += Time.deltaTime;
        if (_lifetimeTimer >= data.duration)
        {
            Despawn();
            return;
        }

        // Find target if we don't have one OR if current target is invalid/dead
        if (_currentTarget == null || !_currentTarget.gameObject.activeInHierarchy)
        {
            _currentTarget = FindNearestEnemy();
        }

        // Move toward target in straight line
        if (_currentTarget != null)
        {
            Vector3 direction = (_currentTarget.position - transform.position).normalized;
            transform.position += direction * currentSpeed * Time.deltaTime;

            // Smoothly rotate to face target on Y-axis only (prevents tilting)
            if (direction != Vector3.zero)
            {
                // Flatten direction to XZ plane (ignore Y difference)
                Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z).normalized;
                if (flatDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(flatDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
                }
            }
        }
    }

    /// <summary>
    /// Finds the nearest enemy using EnemyManager
    /// </summary>
    private Transform FindNearestEnemy()
    {
        if (EnemyManager.Instance == null) return null;

        // Use large range to find any enemy on map
        return EnemyManager.Instance.GetTarget(transform.position, 100f, TargetingMode.Nearest, 0f, false);
    }

    /// <summary>
    /// Triggers explosion when colliding with enemy
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // SAFETY: Stop executing if scene is restarting/loading
        if (SingletonGlobalState.IsSceneLoading || SingletonGlobalState.IsApplicationQuitting)
            return;

        // DEBUG: Log all collisions to diagnose the issue
        Debug.Log($"[MinionController] OnTriggerEnter called! Other: {other.gameObject.name}, Layer: {LayerMask.LayerToName(other.gameObject.layer)}, HasExploded: {_hasExploded}");

        // Ignore if already exploded
        if (_hasExploded) return;

        // CRITICAL: Grace period after spawn (0.1 seconds) to avoid instant collision
        float timeSinceSpawn = Time.time - _spawnTime;
        if (timeSinceSpawn < 0.1f)
        {
            Debug.Log($"[MinionController] ⚠️ Grace period active ({timeSinceSpawn:F3}s < 0.1s) - ignoring collision");
            return;
        }

        // CRITICAL: Ignore the spawner enemy (prevents instant explosion)
        if (_spawnerEnemy != null && other.gameObject == _spawnerEnemy)
        {
            Debug.Log($"[MinionController] ⚠️ Ignoring spawner enemy: {other.gameObject.name}");
            return;
        }

        // Check if hit an enemy (Layer check, not Tag)
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (other.gameObject.layer == enemyLayer)
        {
            // CRITICAL: Ignore dead/dying enemies (they're being returned to pool)
            var enemyController = other.GetComponent<EnemyController>();
            if (enemyController != null && enemyController.currentHp <= 0)
            {
                Debug.Log($"[MinionController] ⚠️ Ignoring dead enemy: {other.gameObject.name} (HP: {enemyController.currentHp})");
                return;
            }

            Debug.Log($"[MinionController] ✅ Enemy collision detected! Exploding on {other.gameObject.name}");
            Explode();
        }
        else
        {
            Debug.Log($"[MinionController] ❌ Not an enemy - layer is '{LayerMask.LayerToName(other.gameObject.layer)}' (expected 'Enemy' = layer {enemyLayer})");
        }
    }

    /// <summary>
    /// Explodes, dealing AOE damage and applying Necrotic mark to all enemies in radius
    /// Supports critical hits with overcrit mechanics (same as projectiles)
    /// </summary>
    private void Explode()
    {
        if (_hasExploded) return;
        _hasExploded = true;

        Vector3 explosionCenter = transform.position;

        // Get all enemies in explosion radius
        if (EnemyManager.Instance != null)
        {
            var enemies = EnemyManager.Instance.GetEnemiesInRange(explosionCenter, currentExplosionRadius);

            foreach (var enemy in enemies)
            {
                // Calculate overcrit: CritChance can exceed 100%
                // Example: 150% CritChance = 1 guaranteed crit + 50% chance for a second crit
                float critChance = currentCritChance;
                int guaranteedCrits = Mathf.FloorToInt(critChance);
                float remainingCritChance = critChance - guaranteedCrits;

                // Roll for additional crit if there's a remainder
                int totalCrits = guaranteedCrits;
                if (remainingCritChance > 0 && Random.value < remainingCritChance)
                {
                    totalCrits++;
                }

                // Calculate final damage
                float finalDamage = currentExplosionDamage;
                bool isCritical = totalCrits > 0;

                if (isCritical)
                {
                    // Each crit multiplies damage successively
                    // Example: 10 dmg, 2x crit, 2 crits = 10 * 2 * 2 = 40 dmg
                    for (int i = 0; i < totalCrits; i++)
                    {
                        finalDamage *= currentCritDamageMultiplier;
                    }
                }

                // Check if this hit will kill the enemy BEFORE applying damage
                bool isFatal = (enemy.currentHp - finalDamage) <= 0;

                // Apply damage with appropriate damage type
                DamageType damageType = isCritical ? DamageType.Critical : DamageType.Normal;
                enemy.TakeDamage(finalDamage, damageType);

                // Play hit sound (critical or normal)
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayEnemyHitSound(enemy.transform.position, isCritical);
                }

                // Apply Necrotic mark (for potential chain reaction)
                if (enemy.StatusEffects != null)
                {
                    enemy.StatusEffects.ApplyNecroticMark();

                    // CRITICAL: Spawn new ghost if this explosion kills a marked enemy
                    // This enables chain reactions: Ghost A kills Enemy B → Ghost B spawns → kills Enemy C → etc.
                    // Chain reactions always spawn (100% chance for simplicity, can be adjusted)
                    if (isFatal && enemy.StatusEffects.IsMarkedByNecrotic && data != null && MinionManager.Instance != null)
                    {
                        // For chain reactions, spawn 1 ghost per kill (no over-spawn on chains to avoid exponential explosion)
                        // This keeps the chain linear: 1 ghost → 1 kill → 1 new ghost
                        Vector3 spawnPos = enemy.transform.position;

                        MinionManager.Instance.SpawnMinion(
                            data, // Reuse same MinionData
                            spawnPos,
                            currentSpeed, // Inherit speed from this ghost
                            currentExplosionRadius, // Inherit explosion radius
                            currentExplosionDamage, // Inherit explosion damage
                            currentCritChance, // Inherit crit chance
                            currentCritDamageMultiplier, // Inherit crit multiplier
                            enemy.gameObject // Set spawner enemy to avoid instant re-explosion
                        );

                        // Mark enemy as having spawned a ghost (prevent duplicate spawns)
                        enemy.StatusEffects.MarkGhostSpawned();

                        Debug.Log($"[MinionController] ⚡ CHAIN REACTION! Ghost spawned new ghost at {spawnPos}");
                    }
                }
            }
        }

        // Spawn explosion VFX
        if (data.explosionVfxPrefab != null && VFXPool.Instance != null)
        {
            VFXPool.Instance.Spawn(data.explosionVfxPrefab, explosionCenter, Quaternion.identity, 2f);
        }

        // Play explosion sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAreaExplosionSound(explosionCenter);
        }

        // Return to pool
        Despawn();
    }

    /// <summary>
    /// Returns ghost to pool (called on lifetime expire or explosion)
    /// </summary>
    private void Despawn()
    {
        // Notify MinionManager to unregister
        if (MinionManager.Instance != null)
        {
            MinionManager.Instance.UnregisterMinion(this);
        }

        // Return to pool
        if (MinionPool.Instance != null && data != null)
        {
            MinionPool.Instance.ReturnToPool(gameObject, data.prefab);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetData(MinionData newData)
    {
        data = newData;
        InitializeStats();
    }
}
