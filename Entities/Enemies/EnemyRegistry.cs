using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages enemy registration, capacity limits, and collider cache.
/// Handles adding/removing enemies from the active pool.
/// </summary>
public class EnemyRegistry : MonoBehaviour
{
    private int maxEnemiesCapacity = 2000;
    private Transform playerTransform;

    private List<EnemyController> _activeEnemies = new List<EnemyController>();
    private Dictionary<int, EnemyController> _colliderCache = new Dictionary<int, EnemyController>();

    private EnemyMovementSystem _movementSystem;

    public bool IsAtCapacity => _activeEnemies.Count >= maxEnemiesCapacity;
    public int ActiveCount => _activeEnemies.Count;
    public List<EnemyController> ActiveEnemies => _activeEnemies;

    public void Initialize(EnemyMovementSystem movementSystem, int capacity, Transform player)
    {
        _movementSystem = movementSystem;
        maxEnemiesCapacity = capacity;
        playerTransform = player;
    }

    /// <summary>
    /// Registers an enemy to the active pool
    /// </summary>
    /// <returns>True if successful, false if at capacity</returns>
    public bool RegisterEnemy(EnemyController enemy, Collider col)
    {
        // At capacity - reject
        if (_activeEnemies.Count >= maxEnemiesCapacity)
            return false;

        if (!_activeEnemies.Contains(enemy))
        {
            _activeEnemies.Add(enemy);

            // Add to movement system
            if (_movementSystem != null)
            {
                _movementSystem.AddEnemyToMovement(
                    enemy.transform,
                    enemy.currentSpeed,
                    enemy.Data.stopDistance,
                    enemy.Data.fleeDistance
                );
            }

            // Cache collider for fast lookup
            if (col != null)
                _colliderCache.TryAdd(col.GetInstanceID(), enemy);
        }

        return true;
    }

    /// <summary>
    /// Unregisters an enemy from the active pool
    /// </summary>
    public void UnregisterEnemy(EnemyController enemy, Collider col)
    {
        if (_activeEnemies.Contains(enemy))
        {
            int index = _activeEnemies.IndexOf(enemy);
            int lastIndex = _activeEnemies.Count - 1;

            // Remove from movement system (handles swap-back internally)
            if (_movementSystem != null)
            {
                _movementSystem.RemoveEnemyFromMovement(index, lastIndex);
            }

            // Swap-back removal for performance
            if (index != lastIndex)
            {
                _activeEnemies[index] = _activeEnemies[lastIndex];
            }

            _activeEnemies.RemoveAt(lastIndex);

            // Remove from collider cache
            if (col != null)
                _colliderCache.Remove(col.GetInstanceID());
        }
    }

    /// <summary>
    /// Tries to free space by despawning the farthest enemy
    /// </summary>
    /// <param name="minDistanceRecycle">Minimum distance required to recycle (e.g., 40m)</param>
    /// <returns>True if space was freed</returns>
    public bool TryFreeSpaceByRecycling(float minDistanceRecycle)
    {
        if (_activeEnemies.Count == 0 || playerTransform == null)
            return false;

        float maxDistSq = -1f;
        int bestIndex = -1;
        float minDistSq = minDistanceRecycle * minDistanceRecycle;
        Vector3 playerPos = playerTransform.position;

        // Find farthest enemy
        for (int i = 0; i < _activeEnemies.Count; i++)
        {
            if (_activeEnemies[i] == null) continue;

            float dSq = (_activeEnemies[i].transform.position - playerPos).sqrMagnitude;

            if (dSq > maxDistSq)
            {
                maxDistSq = dSq;
                bestIndex = i;
            }
        }

        // Despawn if far enough
        if (bestIndex != -1 && maxDistSq > minDistSq)
        {
            _activeEnemies[bestIndex].SilentDespawn();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Fast lookup of enemy by collider instance ID
    /// </summary>
    public bool TryGetEnemyByCollider(Collider col, out EnemyController enemy)
    {
        return _colliderCache.TryGetValue(col.GetInstanceID(), out enemy);
    }

    /// <summary>
    /// Gets all enemies within a radius
    /// </summary>
    public List<EnemyController> GetEnemiesInRange(Vector3 center, float radius)
    {
        List<EnemyController> results = new List<EnemyController>();
        float radiusSqr = radius * radius;

        for (int i = 0; i < _activeEnemies.Count; i++)
        {
            if (_activeEnemies[i] == null) continue;

            if ((_activeEnemies[i].transform.position - center).sqrMagnitude <= radiusSqr)
                results.Add(_activeEnemies[i]);
        }

        return results;
    }

    /// <summary>
    /// Debug utility to kill all active enemies
    /// </summary>
    public void DebugKillAllEnemies()
    {
        for (int i = _activeEnemies.Count - 1; i >= 0; i--)
        {
            if (_activeEnemies[i] != null)
                _activeEnemies[i].TakeDamage(99999f);
        }
    }

    /// <summary>
    /// Clears all cached data
    /// </summary>
    public void Clear()
    {
        _activeEnemies.Clear();
        _colliderCache.Clear();
    }
}
