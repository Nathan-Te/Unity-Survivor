using System;
using UnityEngine;

/// <summary>
/// Handles all enemy-related events and kill tracking.
/// Broadcasts events to UI, score systems, and POI systems.
/// </summary>
public class EnemyEventBroadcaster : MonoBehaviour
{
    // Events
    public event Action<int> OnEnemyCountChanged; // For UI (active enemies)
    public event Action<int> OnKillCountChanged; // For UI (total kills)
    public event Action<Vector3> OnEnemyDeathPosition; // For POI systems
    public event Action<int, Vector3> OnEnemyKilledWithScore; // (scoreValue, position) For score system

    private int _totalKills = 0;

    public int TotalKills => _totalKills;

    /// <summary>
    /// Notifies listeners of enemy count change
    /// </summary>
    public void BroadcastEnemyCountChanged(int count)
    {
        OnEnemyCountChanged?.Invoke(count);
    }

    /// <summary>
    /// Notifies listeners of enemy death and increments kill counter
    /// </summary>
    public void NotifyEnemyDeath(Vector3 position, int scoreValue = 10)
    {
        // Increment kill counter
        _totalKills++;
        OnKillCountChanged?.Invoke(_totalKills);

        // Broadcast death events
        OnEnemyDeathPosition?.Invoke(position);
        OnEnemyKilledWithScore?.Invoke(scoreValue, position);
    }

    /// <summary>
    /// Resets the kill counter (used when restarting game)
    /// </summary>
    public void ResetKillCounter()
    {
        _totalKills = 0;
        OnKillCountChanged?.Invoke(_totalKills);
    }
}
