using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Centralized manager for game state transitions (restart, quit, etc.)
/// Ensures all singletons and managers are properly reset before scene reload.
/// </summary>
public class GameStateManager : Singleton<GameStateManager>
{
    [Header("Settings")]
    [SerializeField] private bool verboseLogging = true;

    protected override void Awake()
    {
        base.Awake();

        if (Instance == this)
        {
            // Ensure this GameObject is at root level for DontDestroyOnLoad
            if (transform.parent != null)
            {
                Debug.LogWarning("[GameStateManager] GameStateManager must be on a root GameObject. Moving to root.");
                transform.SetParent(null);
            }

            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// Restarts the current game scene with full cleanup of all managers and pools.
    /// Call this instead of directly reloading the scene.
    /// </summary>
    public static void RestartGame()
    {
        if (Instance != null)
        {
            Instance.PerformRestart();
        }
        else
        {
            // Fallback if GameStateManager doesn't exist
            Debug.LogWarning("[GameStateManager] Instance not found, doing basic restart");
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void PerformRestart()
    {
        if (verboseLogging)
            Debug.Log("[GameStateManager] Starting game restart sequence...");

        // Start the restart coroutine
        StartCoroutine(RestartSequence());
    }

    private System.Collections.IEnumerator RestartSequence()
    {
        // Mark game as restarting via GameStateController
        if (GameStateController.Instance != null)
            GameStateController.Instance.MarkRestarting();

        // Also set global flag for singleton access blocking
        SingletonGlobalState.IsSceneLoading = true;
        Debug.Log($"[GameStateManager] Game marked as restarting at frame {Time.frameCount}");

        // Wait one full frame so all Update() methods see the new state
        yield return null;

        Debug.Log($"[GameStateManager] Continuing restart after yield at frame {Time.frameCount}");

        // Clean up persistent state BEFORE reloading
        CleanupBeforeReload();

        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnRestartSceneLoaded;

        // Reload the scene (this will destroy all objects and stop all Update() calls)
        string sceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(sceneName);

        if (verboseLogging)
            Debug.Log("[GameStateManager] Scene reload initiated");
    }

    private void OnRestartSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-enable ALL singleton access after scene is loaded
        SingletonGlobalState.IsSceneLoading = false;

        // Unsubscribe to avoid multiple calls
        SceneManager.sceneLoaded -= OnRestartSceneLoaded;

        // Reset game state to Playing after restart
        if (GameStateController.Instance != null)
        {
            GameStateController.Instance.SetState(GameStateController.GameState.Playing);
        }

        if (verboseLogging)
            Debug.Log($"[GameStateManager] Scene '{scene.name}' loaded, all singletons re-enabled, game state set to Playing");
    }

    /// <summary>
    /// Cleanup persistent state that survives scene reloads.
    /// Resets DontDestroyOnLoad singletons and clears references for scene-based singletons.
    /// </summary>
    private void CleanupBeforeReload()
    {
        if (verboseLogging)
            Debug.Log("[GameStateManager] Cleaning up persistent state...");

        ResetProgression();
        ResetPlayerPersistentStats();
        ClearSceneSingletonReferences();
    }

    /// <summary>
    /// Clears static references for all scene-based singletons (non-DontDestroyOnLoad).
    /// This prevents stale references after scene reload.
    /// </summary>
    private void ClearSceneSingletonReferences()
    {
        if (verboseLogging)
            Debug.Log("[GameStateManager] Clearing scene-based singleton references...");

        // Clear all scene-based singletons (those that will be destroyed with scene reload)
        PlayerController.ClearInstance();
        EnemyManager.ClearInstance();
        ProjectilePool.ClearInstance();
        EnemyPool.ClearInstance();
        GemPool.ClearInstance();
        DamageTextPool.ClearInstance();
        MapObjectPool.ClearInstance();
        VFXPool.ClearInstance();
        WorldStateManager.ClearInstance();
        MapManager.ClearInstance();
        BossHealthBarUI.ClearInstance();
        EnemyScalingManager.ClearInstance();
        ZonePOIScalingManager.ClearInstance();
        ArcadeScoreSystem.ClearInstance();

        // Note: GameStateController, GameStateManager, LevelManager, GameTimer, PlayerStats, and MemoryManager
        // are DontDestroyOnLoad and should NOT be cleared
    }

    private void ResetProgression()
    {
        if (verboseLogging)
            Debug.Log("[GameStateManager] Resetting progression...");

        // Reset LevelManager
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.currentLevel = 1;
            LevelManager.Instance.currentExperience = 0;
            LevelManager.Instance.experienceToNextLevel = 100;
            LevelManager.Instance.availableRerolls = 2;
            LevelManager.Instance.availableBans = 1;

            // Clear banned runes via reflection (since it's private)
            var field = typeof(LevelManager).GetField("_bannedRunes",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var list = field.GetValue(LevelManager.Instance) as System.Collections.Generic.List<string>;
                list?.Clear();
            }

            // Clear pending level-ups
            var pendingField = typeof(LevelManager).GetField("_pendingLevelUps",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (pendingField != null)
            {
                pendingField.SetValue(LevelManager.Instance, 0);
            }
        }

        // Reset GameTimer
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.ResetTimer();
        }
    }

    private void ResetPlayerPersistentStats()
    {
        if (verboseLogging)
            Debug.Log("[GameStateManager] Resetting player persistent stats...");

        // Reset PlayerStats (only if it's a DontDestroyOnLoad singleton)
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.Might = 1.0f;
            PlayerStats.Instance.CooldownSpeed = 1.0f;
            PlayerStats.Instance.AreaSize = 1.0f;
            PlayerStats.Instance.ProjectileSpeed = 1.0f;
            PlayerStats.Instance.AdditionalAmount = 0;
            PlayerStats.Instance.CritChance = 0.0f;
            PlayerStats.Instance.CritDamage = 1.5f;
            PlayerStats.Instance.ExperienceMultiplier = 1.0f;

            // Clear acquired stat types and stat runes via reflection
            var acquiredField = typeof(PlayerStats).GetField("_acquiredStatTypes",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (acquiredField != null)
            {
                var hashSet = acquiredField.GetValue(PlayerStats.Instance) as System.Collections.Generic.HashSet<StatType>;
                hashSet?.Clear();
            }

            var runesField = typeof(PlayerStats).GetField("_statRunes",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (runesField != null)
            {
                var dict = runesField.GetValue(PlayerStats.Instance) as System.Collections.Generic.Dictionary<StatType, Rune>;
                dict?.Clear();
            }
        }

        // PlayerController will be reset when scene reloads (Awake sets _currentHp = maxHp)
        // All other managers (WorldStateManager, EnemyManager, SpellManager) will be destroyed and recreated
    }

    /// <summary>
    /// Quits the application (or stops Play mode in editor)
    /// </summary>
    public static void QuitGame()
    {
        if (Instance != null && Instance.verboseLogging)
            Debug.Log("[GameStateManager] Quitting game...");

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
