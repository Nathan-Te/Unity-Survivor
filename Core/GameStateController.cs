using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Centralized game state controller.
/// Manages game states (Playing, Paused, Restarting) and notifies all systems via events.
/// This replaces Time.timeScale manipulation for better control.
/// </summary>
public class GameStateController : Singleton<GameStateController>
{
    public enum GameState
    {
        Playing,
        Paused,
        Restarting,
        LevelingUp
    }

    private GameState _currentState = GameState.Playing;

    // Events
    public UnityEvent OnGamePaused = new UnityEvent();
    public UnityEvent OnGameResumed = new UnityEvent();
    public UnityEvent OnGameRestarting = new UnityEvent();
    public UnityEvent<GameState> OnStateChanged = new UnityEvent<GameState>();

    public GameState CurrentState => _currentState;
    public bool IsPlaying => _currentState == GameState.Playing;
    public bool IsPaused => _currentState == GameState.Paused;
    public bool IsRestarting => _currentState == GameState.Restarting;

    protected override void Awake()
    {
        base.Awake();

        if (Instance == this)
        {
            // Ensure this GameObject is at root level for DontDestroyOnLoad
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }

            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// Sets the game state and fires appropriate events
    /// </summary>
    public void SetState(GameState newState)
    {
        if (_currentState == newState) return;

        GameState previousState = _currentState;
        _currentState = newState;

        Debug.Log($"[GameStateController] State changed: {previousState} → {newState}");

        // Update Time.timeScale based on state
        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
            case GameState.LevelingUp:
                Time.timeScale = 0f;
                break;
            case GameState.Restarting:
                // Keep timeScale = 1 for restart to allow coroutines to run
                Time.timeScale = 1f;
                break;
        }

        // Fire specific events
        switch (newState)
        {
            case GameState.Paused:
                OnGamePaused?.Invoke();
                break;
            case GameState.Playing:
                if (previousState == GameState.Paused)
                    OnGameResumed?.Invoke();
                break;
            case GameState.Restarting:
                OnGameRestarting?.Invoke();
                break;
        }

        // Fire generic state change event
        OnStateChanged?.Invoke(newState);
    }

    /// <summary>
    /// Pauses the game
    /// </summary>
    public void Pause()
    {
        SetState(GameState.Paused);
    }

    /// <summary>
    /// Resumes the game
    /// </summary>
    public void Resume()
    {
        SetState(GameState.Playing);
    }

    /// <summary>
    /// Marks the game as restarting
    /// </summary>
    public void MarkRestarting()
    {
        SetState(GameState.Restarting);
    }
}
