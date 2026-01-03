using UnityEngine;
using UnityEngine.SceneManagement;
using SurvivorGame.Core;

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
        // CRITICAL: Get SceneLoader reference BEFORE blocking singleton access
        var sceneLoader = SceneLoader.Instance;

        // Mark game as restarting via GameStateController
        if (GameStateController.Instance != null)
            GameStateController.Instance.MarkRestarting();

        // Also set global flag for singleton access blocking
        SingletonGlobalState.IsSceneLoading = true;
        Debug.Log($"[GameStateManager] Game marked as restarting at frame {Time.frameCount}");

        // Wait one full frame so all Update() methods see the new state
        yield return null;

        Debug.Log($"[GameStateManager] Continuing restart after yield at frame {Time.frameCount}");

        // IMPORTANT: Disable ALL GameObjects in current scene to stop all Update() loops
        DeactivateCurrentScene();

        // Clean up persistent state BEFORE reloading (but DON'T clear pools yet - SceneLoader might need them)
        ResetProgression();
        ResetPlayerPersistentStats();
        ClearSceneSingletonReferences();

        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnRestartSceneLoaded;

        // Reload the scene using SceneLoader (shows loading screen)
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneLoader != null)
        {
            // CRITICAL: Let SceneLoader handle the transition (use cached reference)
            // Don't clear pools here - SceneLoader's cleanup will handle it
            sceneLoader.LoadScene(sceneName);
        }
        else
        {
            // Fallback: Use SceneManager directly if SceneLoader doesn't exist
            Debug.LogWarning("[GameStateManager] SceneLoader not found, using fallback SceneManager.LoadScene");

            // If no SceneLoader, do cleanup manually before loading
            ClearAllPools();

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

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

        // CRITICAL: Clear all pools FIRST to stop Update() loops
        ClearAllPools();

        ResetProgression();
        ResetPlayerPersistentStats();
        ClearSceneSingletonReferences();
    }

    /// <summary>
    /// Clears all object pools to stop Update() loops before scene transition.
    /// This prevents NullReferenceExceptions when pooled objects try to access destroyed singletons.
    /// </summary>
    private void ClearAllPools()
    {
        if (verboseLogging)
            Debug.Log("[GameStateManager] Clearing all object pools...");

        // Clear all pools (deactivates active objects, stops Update() loops)
        if (DamageTextPool.Instance != null)
            DamageTextPool.Instance.ClearAll();

        if (ProjectilePool.Instance != null)
            ProjectilePool.Instance.ClearAll();

        if (EnemyPool.Instance != null)
            EnemyPool.Instance.ClearAll();

        if (GemPool.Instance != null)
            GemPool.Instance.ClearAll();

        if (MapObjectPool.Instance != null)
            MapObjectPool.Instance.ClearAll();

        if (VFXPool.Instance != null)
            VFXPool.Instance.ClearAll();

        if (MinionPool.Instance != null)
            MinionPool.Instance.ClearAll();

        if (verboseLogging)
            Debug.Log("[GameStateManager] All pools cleared (objects deactivated, Update() loops stopped)");
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
        MinionPool.ClearInstance();
        MinionManager.ClearInstance();
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
    /// Returns to main menu with full cleanup of all managers and pools.
    /// Call this instead of directly loading the MainMenu scene.
    /// </summary>
    public static void ReturnToMainMenu()
    {
        if (Instance != null)
        {
            Instance.PerformReturnToMainMenu();
        }
        else
        {
            // Fallback if GameStateManager doesn't exist
            Debug.LogWarning("[GameStateManager] Instance not found, doing basic main menu load");
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }

    private void PerformReturnToMainMenu()
    {
        if (verboseLogging)
            Debug.Log("[GameStateManager] Returning to main menu...");

        // Start the return to menu coroutine
        StartCoroutine(ReturnToMainMenuSequence());
    }

    private System.Collections.IEnumerator ReturnToMainMenuSequence()
    {
        // CRITICAL: Get SceneLoader reference BEFORE blocking singleton access
        // Otherwise SceneLoader.Instance will return null when IsSceneLoading = true
        var sceneLoader = SceneLoader.Instance;

        // Mark game as restarting via GameStateController
        if (GameStateController.Instance != null)
            GameStateController.Instance.MarkRestarting();

        // Also set global flag for singleton access blocking
        SingletonGlobalState.IsSceneLoading = true;
        Debug.Log($"[GameStateManager] Game marked as restarting (returning to menu) at frame {Time.frameCount}");

        // Wait one full frame so all Update() methods see the new state
        yield return null;

        Debug.Log($"[GameStateManager] Continuing return to menu after yield at frame {Time.frameCount}");

        // IMPORTANT: Disable ALL GameObjects in current scene to stop all Update() loops
        DeactivateCurrentScene();

        // CRITICAL: Clear all pools before destroying singletons
        ClearAllPools();

        // CRITICAL: Force save progression BEFORE destroying ProgressionManager
        // This ensures all progression data is written to disk
        if (SurvivorGame.Progression.ProgressionManager.Instance != null)
        {
            SurvivorGame.Progression.ProgressionManager.Instance.SaveProgression();
            if (verboseLogging)
                Debug.Log("[GameStateManager] Forced progression save before returning to MainMenu");
        }

        // Ensure time scale is reset
        Time.timeScale = 1f;

        // Subscribe to scene loaded event to re-enable singleton access
        SceneManager.sceneLoaded += OnMainMenuSceneLoadedFinal;

        Debug.Log("[GameStateManager] Loading MainMenu scene...");

        // CRITICAL: Use SceneLoader if available (using reference captured before blocking)
        if (sceneLoader != null)
        {
            // Start the loading process (use cached reference, not Instance getter)
            sceneLoader.LoadScene("MainMenu");

            // IMPORTANT: Wait for SceneLoader to complete the fade-in and start loading
            // This gives the loading screen time to appear before we destroy everything
            yield return new WaitForSecondsRealtime(0.7f); // Fade duration + cleanup + buffer

            Debug.Log("[GameStateManager] SceneLoader fade-in complete, now destroying DontDestroyOnLoad objects...");

            // Now safe to destroy all persistent objects (except SceneLoader which is protected)
            // SceneLoader will continue running and complete the scene transition
            DestroyAllPersistentSingletonsExceptSceneLoader();

            // SceneLoader will persist and be reused in the MainMenu scene
            // No need to destroy it - it's designed to be reusable
        }
        else
        {
            Debug.LogWarning("[GameStateManager] SceneLoader not found, using fallback SceneManager.LoadSceneAsync");

            // CRITICAL: Start loading MainMenu scene BEFORE destroying managers
            // This ensures the load operation is registered before this object is destroyed
            AsyncOperation loadOp = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            loadOp.allowSceneActivation = true;

            // Wait one frame to ensure load operation started
            yield return null;

            Debug.Log("[GameStateManager] Scene load started (fallback), now destroying all DontDestroyOnLoad objects...");

            // Destroy everything EXCEPT SceneLoader (even in fallback mode, protect it)
            DestroyAllPersistentSingletonsExceptSceneLoader();
        }

        // NOTE: Code after this point will NOT execute because GameStateManager is destroyed
        // The scene load will complete and OnMainMenuSceneLoadedFinal will be called automatically
    }

    private void OnMainMenuSceneLoadedFinal(Scene scene, LoadSceneMode mode)
    {
        // CRITICAL: Re-enable singleton access after MainMenu is loaded
        SingletonGlobalState.IsSceneLoading = false;

        // Unsubscribe to avoid multiple calls
        SceneManager.sceneLoaded -= OnMainMenuSceneLoadedFinal;

        Debug.Log($"[GameStateManager] MainMenu scene '{scene.name}' loaded successfully");
        Debug.Log("[GameStateManager] Singleton access re-enabled");
        Debug.Log("[GameStateManager] All DontDestroyOnLoad objects were destroyed - MainMenu starts fresh");
        Debug.Log("[GameStateManager] MainMenu Canvas will be activated by MainMenuUI.Awake()");
    }

    private void OnMainMenuSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // CRITICAL: Re-enable singleton access after MainMenu is loaded
        SingletonGlobalState.IsSceneLoading = false;

        // Unsubscribe to avoid multiple calls
        SceneManager.sceneLoaded -= OnMainMenuSceneLoaded;

        // CRITICAL: Ensure all Canvas in the MainMenu scene are enabled
        // This fixes the issue where Canvas might be disabled from previous session
        EnsureMainMenuCanvasActive(scene);

        if (verboseLogging)
            Debug.Log($"[GameStateManager] MainMenu scene '{scene.name}' loaded, singleton access re-enabled");
    }

    /// <summary>
    /// Ensures all Canvas components in the MainMenu scene are active and enabled.
    /// This fixes the issue where returning from game leaves Canvas disabled.
    /// </summary>
    private void EnsureMainMenuCanvasActive(Scene scene)
    {
        GameObject[] rootObjects = scene.GetRootGameObjects();
        int canvasCount = 0;

        foreach (GameObject obj in rootObjects)
        {
            // Find all Canvas components recursively
            Canvas[] canvases = obj.GetComponentsInChildren<Canvas>(true); // true = include inactive
            foreach (Canvas canvas in canvases)
            {
                if (!canvas.gameObject.activeSelf || !canvas.enabled)
                {
                    canvas.gameObject.SetActive(true);
                    canvas.enabled = true;
                    canvasCount++;

                    if (verboseLogging)
                        Debug.Log($"[GameStateManager] Activated Canvas: {canvas.name}");
                }
            }
        }

        if (verboseLogging)
            Debug.Log($"[GameStateManager] Ensured {canvasCount} Canvas components are active in MainMenu scene");
    }

    /// <summary>
    /// Destroys Canvas objects that are in DontDestroyOnLoad scene.
    /// This ensures old MainMenu Canvas doesn't interfere when loading fresh MainMenu scene.
    /// Called before returning to MainMenu to ensure clean slate.
    /// Does NOT destroy Canvas from the current game scene (they'll be destroyed with scene reload).
    /// </summary>
    private void DestroyAllCanvasObjects()
    {
        if (verboseLogging)
            Debug.Log("[GameStateManager] Destroying Canvas objects in DontDestroyOnLoad...");

        int destroyedCount = 0;

        // Find ALL Canvas objects (including inactive)
        Canvas[] allCanvases = GameObject.FindObjectsOfType<Canvas>(true);

        foreach (Canvas canvas in allCanvases)
        {
            // ONLY destroy Canvas that are in DontDestroyOnLoad
            if (canvas.gameObject.scene.name == "DontDestroyOnLoad")
            {
                if (verboseLogging)
                    Debug.Log($"[GameStateManager] Destroying DontDestroyOnLoad Canvas: {canvas.name}");

                Destroy(canvas.gameObject);
                destroyedCount++;
            }
            else
            {
                if (verboseLogging)
                    Debug.Log($"[GameStateManager] Skipping Canvas in scene '{canvas.gameObject.scene.name}': {canvas.name} (will be destroyed with scene reload)");
            }
        }

        if (verboseLogging)
            Debug.Log($"[GameStateManager] Destroyed {destroyedCount} Canvas objects from DontDestroyOnLoad");
    }

    /// <summary>
    /// Deactivates all GameObjects in the current scene (except DontDestroyOnLoad objects)
    /// </summary>
    private void DeactivateCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = currentScene.GetRootGameObjects();

        foreach (GameObject obj in rootObjects)
        {
            // Deactivate all root objects in the current scene
            // This will stop all MonoBehaviour Update() calls immediately
            obj.SetActive(false);

            if (verboseLogging)
                Debug.Log($"[GameStateManager] Deactivated scene object: {obj.name}");
        }

        if (verboseLogging)
            Debug.Log($"[GameStateManager] Deactivated {rootObjects.Length} root GameObjects in scene '{currentScene.name}'");
    }

    /// <summary>
    /// Destroys ALL DontDestroyOnLoad singletons for a complete fresh start.
    /// This includes GameStateController, LevelManager, PlayerStats, GameTimer, etc.
    /// Used when returning to main menu to ensure a clean slate.
    /// </summary>
    private void DestroyAllPersistentSingletons()
    {
        if (verboseLogging)
            Debug.Log("[GameStateManager] Destroying ALL persistent DontDestroyOnLoad singletons...");

        // Clear singleton references first
        ClearSceneSingletonReferences();

        // Destroy all DontDestroyOnLoad GameObjects
        // Get all root objects in the DontDestroyOnLoad scene
        GameObject[] dontDestroyObjects = GameObject.FindObjectsOfType<GameObject>();
        int destroyedCount = 0;

        foreach (GameObject obj in dontDestroyObjects)
        {
            // Check if object is in DontDestroyOnLoad scene
            if (obj.scene.name == "DontDestroyOnLoad")
            {
                if (verboseLogging)
                    Debug.Log($"[GameStateManager] Destroying DontDestroyOnLoad object: {obj.name}");

                Destroy(obj);
                destroyedCount++;
            }
        }

        if (verboseLogging)
            Debug.Log($"[GameStateManager] Destroyed {destroyedCount} DontDestroyOnLoad objects");

        // NOTE: Do NOT reset IsSceneLoading here - it will be reset in OnMainMenuSceneLoaded() or OnRestartSceneLoaded()
    }

    /// <summary>
    /// Destroys ALL DontDestroyOnLoad GameObjects EXCEPT SceneLoader.
    /// Used for ALL cleanup scenarios to ensure loading screen always works.
    /// SceneLoader is ALWAYS protected and persists for the entire game session.
    /// </summary>
    private void DestroyAllPersistentSingletonsExceptSceneLoader()
    {
        if (verboseLogging)
            Debug.Log("[GameStateManager] Destroying persistent singletons (always protecting SceneLoader)...");

        // Clear singleton references first
        ClearSceneSingletonReferences();

        // Destroy ALL DontDestroyOnLoad GameObjects EXCEPT SceneLoader
        GameObject[] dontDestroyObjects = GameObject.FindObjectsOfType<GameObject>();
        int destroyedCount = 0;
        int skippedCount = 0;

        foreach (GameObject obj in dontDestroyObjects)
        {
            // Check if object is in DontDestroyOnLoad scene
            if (obj.scene.name == "DontDestroyOnLoad")
            {
                // CRITICAL: ALWAYS skip SceneLoader and its children
                // This ensures loading screen works for ALL transitions
                if (obj.GetComponent<SceneLoader>() != null || obj.GetComponentInParent<SceneLoader>() != null)
                {
                    if (verboseLogging)
                        Debug.Log($"[GameStateManager] Protecting SceneLoader object: {obj.name}");
                    skippedCount++;
                    continue;
                }

                if (verboseLogging)
                    Debug.Log($"[GameStateManager] Destroying DontDestroyOnLoad object: {obj.name}");

                Destroy(obj);
                destroyedCount++;
            }
        }

        if (verboseLogging)
            Debug.Log($"[GameStateManager] Destroyed {destroyedCount} objects, protected {skippedCount} SceneLoader objects");
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
