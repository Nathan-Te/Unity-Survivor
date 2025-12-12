using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main coordinator for the enemy system.
/// Delegates movement to EnemyMovementSystem, registration to EnemyRegistry, and events to EnemyEventBroadcaster.
/// </summary>
[DefaultExecutionOrder(-50)]
public class EnemyManager : Singleton<EnemyManager>
{
    [Header("Réglages")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Performance")]
    [SerializeField] private int maxEnemiesCapacity = 2000;

    [Header("Steering Settings")]
    [SerializeField] private float separationWeight = 1.5f;
    [SerializeField] private float rayDistance = 5.0f;
    [SerializeField] private float rayAngle = 45f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float avoidanceBlendSpeed = 3f;

    // Sub-components
    private EnemyMovementSystem _movementSystem;
    private EnemyRegistry _registry;
    private EnemyEventBroadcaster _eventBroadcaster;

    // Property delegations for public API compatibility
    public bool IsAtCapacity => _registry != null && _registry.IsAtCapacity;
    public int TotalKills => _eventBroadcaster != null ? _eventBroadcaster.TotalKills : 0;

    // Event delegations
    public event Action<int> OnEnemyCountChanged
    {
        add { if (_eventBroadcaster != null) _eventBroadcaster.OnEnemyCountChanged += value; }
        remove { if (_eventBroadcaster != null) _eventBroadcaster.OnEnemyCountChanged -= value; }
    }

    public event Action<int> OnKillCountChanged
    {
        add { if (_eventBroadcaster != null) _eventBroadcaster.OnKillCountChanged += value; }
        remove { if (_eventBroadcaster != null) _eventBroadcaster.OnKillCountChanged -= value; }
    }

    public event Action<Vector3> OnEnemyDeathPosition
    {
        add { if (_eventBroadcaster != null) _eventBroadcaster.OnEnemyDeathPosition += value; }
        remove { if (_eventBroadcaster != null) _eventBroadcaster.OnEnemyDeathPosition -= value; }
    }

    public event Action<int, Vector3> OnEnemyKilledWithScore
    {
        add { if (_eventBroadcaster != null) _eventBroadcaster.OnEnemyKilledWithScore += value; }
        remove { if (_eventBroadcaster != null) _eventBroadcaster.OnEnemyKilledWithScore -= value; }
    }

    protected override void Awake()
    {
        base.Awake();

        if (Instance == this)
        {
            // Get or add sub-components
            _movementSystem = GetComponent<EnemyMovementSystem>();
            if (_movementSystem == null)
                _movementSystem = gameObject.AddComponent<EnemyMovementSystem>();

            _registry = GetComponent<EnemyRegistry>();
            if (_registry == null)
                _registry = gameObject.AddComponent<EnemyRegistry>();

            _eventBroadcaster = GetComponent<EnemyEventBroadcaster>();
            if (_eventBroadcaster == null)
                _eventBroadcaster = gameObject.AddComponent<EnemyEventBroadcaster>();

            // Initialize sub-components with parameters
            _movementSystem.Initialize(
                playerTransform,
                obstacleLayer,
                maxEnemiesCapacity,
                separationWeight,
                rayDistance,
                rayAngle,
                rotationSpeed,
                avoidanceBlendSpeed
            );
            _registry.Initialize(_movementSystem, maxEnemiesCapacity, playerTransform);
        }
    }

    private void Update()
    {
        if (_movementSystem != null && _registry != null)
        {
            _movementSystem.UpdateMovement(_registry.ActiveEnemies);
        }
    }

    /// <summary>
    /// Tries to free space by despawning the farthest enemy
    /// </summary>
    public bool TryFreeSpaceByRecycling(float minDistanceRecycle)
    {
        return _registry != null && _registry.TryFreeSpaceByRecycling(minDistanceRecycle);
    }

    /// <summary>
    /// Registers an enemy to the active pool
    /// </summary>
    public bool RegisterEnemy(EnemyController enemy, Collider col)
    {
        if (_registry == null) return false;

        bool success = _registry.RegisterEnemy(enemy, col);

        if (success && _eventBroadcaster != null)
            _eventBroadcaster.BroadcastEnemyCountChanged(_registry.ActiveCount);

        return success;
    }

    /// <summary>
    /// Unregisters an enemy from the active pool
    /// </summary>
    public void UnregisterEnemy(EnemyController enemy, Collider col)
    {
        if (_registry == null) return;

        _registry.UnregisterEnemy(enemy, col);

        if (_eventBroadcaster != null)
            _eventBroadcaster.BroadcastEnemyCountChanged(_registry.ActiveCount);
    }

    /// <summary>
    /// Notifies the system of an enemy death
    /// </summary>
    public void NotifyEnemyDeath(Vector3 position, int scoreValue = 10)
    {
        if (_eventBroadcaster != null)
            _eventBroadcaster.NotifyEnemyDeath(position, scoreValue);
    }

    /// <summary>
    /// Debug utility to kill all enemies
    /// </summary>
    public void DebugKillAllEnemies()
    {
        if (_registry != null)
            _registry.DebugKillAllEnemies();
    }

    /// <summary>
    /// Resets the kill counter
    /// </summary>
    public void ResetKillCounter()
    {
        if (_eventBroadcaster != null)
            _eventBroadcaster.ResetKillCounter();
    }

    /// <summary>
    /// Fast lookup of enemy by collider
    /// </summary>
    public bool TryGetEnemyByCollider(Collider col, out EnemyController enemy)
    {
        enemy = null;
        return _registry != null && _registry.TryGetEnemyByCollider(col, out enemy);
    }

    /// <summary>
    /// Gets all enemies within range
    /// </summary>
    public List<EnemyController> GetEnemiesInRange(Vector3 center, float radius)
    {
        return _registry != null ? _registry.GetEnemiesInRange(center, radius) : new List<EnemyController>();
    }

    /// <summary>
    /// Gets target based on targeting mode
    /// </summary>
    public Transform GetTarget(Vector3 sourcePos, float range, TargetingMode mode, float areaSize = 2f, bool checkVisibility = true)
    {
        if (_registry == null) return null;

        List<EnemyController> activeEnemies = _registry.ActiveEnemies;

        switch (mode)
        {
            case TargetingMode.Nearest:
                return TargetingUtils.GetNearestEnemy(activeEnemies, sourcePos, range, checkVisibility, obstacleLayer);
            case TargetingMode.HighestDensity:
                return TargetingUtils.GetDensestCluster(activeEnemies, sourcePos, range, areaSize, checkVisibility, obstacleLayer);
            case TargetingMode.Random:
                return TargetingUtils.GetRandomEnemy(activeEnemies, sourcePos, range, checkVisibility, obstacleLayer);
            default:
                return null;
        }
    }

    protected override void OnApplicationQuit()
    {
        if (_movementSystem != null)
            _movementSystem.Cleanup();

        if (_registry != null)
            _registry.Clear();

        base.OnApplicationQuit();
    }

    protected override void OnDestroy()
    {
        if (_movementSystem != null)
            _movementSystem.Cleanup();

        if (_registry != null)
            _registry.Clear();

        base.OnDestroy();
    }
}
