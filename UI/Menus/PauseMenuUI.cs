using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the pause menu overlay.
/// Opens with ESC key, pauses game (Time.timeScale = 0).
/// Provides Resume, Restart, and Quit options.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    [Header("Settings")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    private bool _isPaused = false;
    private bool _canPause = true; // Prevents pausing during level-up

    private void Start()
    {
        // Initialize hidden
        SetVisible(false);

        // Wire up buttons
        if (resumeButton) resumeButton.onClick.AddListener(Resume);
        if (restartButton) restartButton.onClick.AddListener(Restart);
        if (quitButton) quitButton.onClick.AddListener(Quit);

        // Listen to level-up events to block pausing
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelUp.AddListener(OnLevelUpStarted);
        }

        // Listen to game state changes to re-enable pausing after level-up
        if (GameStateController.Instance != null)
        {
            GameStateController.Instance.OnStateChanged.AddListener(OnGameStateChanged);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (_isPaused)
            {
                Resume();
            }
            else if (_canPause)
            {
                Pause();
            }
        }
    }

    private void OnLevelUpStarted()
    {
        // Disable pausing during level-up UI
        _canPause = false;

        // If already paused, auto-resume to show level-up
        if (_isPaused)
        {
            Resume();
        }
    }

    private void OnGameStateChanged(GameStateController.GameState newState)
    {
        // Re-enable pausing when returning to Playing state after level-up
        if (newState == GameStateController.GameState.Playing)
        {
            _canPause = true;
        }
        // Disable pausing during level-up
        else if (newState == GameStateController.GameState.LevelingUp)
        {
            _canPause = false;
        }
    }

    /// <summary>
    /// Opens the pause menu and pauses the game
    /// </summary>
    public void Pause()
    {
        if (!_canPause) return;

        _isPaused = true;
        SetVisible(true);

        // Use GameStateController instead of Time.timeScale
        if (GameStateController.Instance != null)
            GameStateController.Instance.Pause();
    }

    /// <summary>
    /// Closes the pause menu and resumes the game
    /// </summary>
    public void Resume()
    {
        _isPaused = false;
        SetVisible(false);

        // Use GameStateController instead of Time.timeScale
        if (GameStateController.Instance != null)
            GameStateController.Instance.Resume();

        // Re-enable pausing after resuming
        _canPause = true;
    }

    /// <summary>
    /// Restarts the current scene with full cleanup of all managers
    /// </summary>
    private void Restart()
    {
        GameStateManager.RestartGame();
    }

    /// <summary>
    /// Returns to the main menu
    /// </summary>
    private void Quit()
    {
        GameStateManager.ReturnToMainMenu();
    }

    /// <summary>
    /// Shows/hides the pause menu using CanvasGroup.alpha (avoids Canvas rebuild)
    /// </summary>
    private void SetVisible(bool visible)
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    private void OnDestroy()
    {
        // Cleanup button listeners
        if (resumeButton) resumeButton.onClick.RemoveListener(Resume);
        if (restartButton) restartButton.onClick.RemoveListener(Restart);
        if (quitButton) quitButton.onClick.RemoveListener(Quit);

        // Cleanup level-up listener
        // Use FindFirstObjectByType to avoid Singleton getter error during scene unload
        var levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.OnLevelUp.RemoveListener(OnLevelUpStarted);
        }

        // Cleanup game state listener
        var gameStateController = FindFirstObjectByType<GameStateController>();
        if (gameStateController != null)
        {
            gameStateController.OnStateChanged.RemoveListener(OnGameStateChanged);
        }
    }
}
