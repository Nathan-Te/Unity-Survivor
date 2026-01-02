using UnityEngine;
using SurvivorGame.UI;

/// <summary>
/// Manages game over state and coordinates between player death and UI display.
/// Listens to GameEvents.OnPlayerDeath and triggers the Game Over UI.
/// </summary>
public class GameOverManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameOverUI gameOverUI;

    [Header("Settings")]
    [SerializeField] private bool verboseLogging = false;

    private void Start()
    {
        // Subscribe to player death event
        GameEvents.OnPlayerDeath.AddListener(OnPlayerDeath);

        if (verboseLogging)
            Debug.Log("[GameOverManager] Initialized and listening for player death");
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        GameEvents.OnPlayerDeath.RemoveListener(OnPlayerDeath);
    }

    /// <summary>
    /// Called when the player dies
    /// </summary>
    private void OnPlayerDeath(float timeSurvived, int levelReached, int enemiesKilled, int score)
    {
        if (verboseLogging)
            Debug.Log($"[GameOverManager] Player died: Time={timeSurvived:F1}s, Level={levelReached}, Kills={enemiesKilled}, Score={score}");

        // Set game state to paused (stops gameplay)
        if (GameStateController.Instance != null)
        {
            GameStateController.Instance.Pause();
        }

        // Show Game Over UI with statistics
        if (gameOverUI != null)
        {
            gameOverUI.Show(timeSurvived, levelReached, enemiesKilled, score);
        }
        else
        {
            Debug.LogError("[GameOverManager] GameOverUI reference is missing!");
        }
    }
}
