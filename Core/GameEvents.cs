using UnityEngine.Events;

/// <summary>
/// Global game events for decoupled communication between systems.
/// Prevents circular dependencies and makes events easy to subscribe to.
/// </summary>
public static class GameEvents
{
    /// <summary>
    /// Fired when the player dies.
    /// Parameters: timeSurvived, levelReached, enemiesKilled, score
    /// </summary>
    public static UnityEvent<float, int, int, int> OnPlayerDeath = new UnityEvent<float, int, int, int>();

    /// <summary>
    /// Fired when a boss is defeated
    /// </summary>
    public static UnityEvent OnBossDefeated = new UnityEvent();

    /// <summary>
    /// Fired when a wave is completed
    /// </summary>
    public static UnityEvent<int> OnWaveCompleted = new UnityEvent<int>();

    /// <summary>
    /// Clears all event listeners (call this on scene unload)
    /// </summary>
    public static void ClearAllListeners()
    {
        OnPlayerDeath.RemoveAllListeners();
        OnBossDefeated.RemoveAllListeners();
        OnWaveCompleted.RemoveAllListeners();
    }
}
