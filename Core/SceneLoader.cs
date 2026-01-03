using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

namespace SurvivorGame.Core
{
    /// <summary>
    /// Manages scene transitions with a loading screen.
    /// Prevents screen freezing by using LoadSceneAsync and displaying progress.
    /// Singleton that persists across scenes (DontDestroyOnLoad).
    /// </summary>
    public class SceneLoader : Singleton<SceneLoader>
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup loadingCanvasGroup;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI tipText;

        [Header("Loading Settings")]
        [SerializeField] private float minimumLoadingTime = 1.5f;
        [Tooltip("Time to fade in/out the loading screen")]
        [SerializeField] private float fadeDuration = 0.5f;

        [Header("Game Tips")]
        [SerializeField] private string[] gameTips = new string[]
        {
            "Tip: Combine Form and Effect runes to create powerful spells!",
            "Tip: Different elements have different effects on enemies.",
            "Tip: Multicast repeats your entire spell cast multiple times.",
            "Tip: Multishot adds extra projectiles to each cast.",
            "Tip: Level up your runes to unlock powerful upgrades!",
            "Tip: Collect gems to gain experience and level up.",
            "Tip: Some enemies are resistant to certain elements.",
            "Tip: Use the ban feature to avoid unwanted runes.",
            "Tip: Area spells always spread in a full circle.",
            "Tip: Orbit spells never despawn and can hit multiple enemies.",
            "Tip: Critical hits can stack - over 100% crit chance means multiple crits!",
            "Tip: Necrotic damage can spawn ghost minions when enemies die.",
            "Tip: Ghost minions explode on impact, dealing area damage.",
            "Tip: Move to avoid enemy attacks - mobility is key!",
            "Tip: Try different spell combinations to find your playstyle."
        };

        [Header("Settings")]
        [SerializeField] private bool verboseLogging = false;

        private Canvas _loadingCanvas;
        private bool _isLoading = false;

        protected override void Awake()
        {
            // CRITICAL: Check if another instance already exists BEFORE calling base.Awake()
            // This prevents duplicate SceneLoaders when MainMenu reloads
            if (Instance != null && Instance != this)
            {
                // Another SceneLoader already exists in DontDestroyOnLoad, destroy this duplicate
                if (verboseLogging || true) // Always log duplicates for debugging
                    Debug.Log($"[SceneLoader] Duplicate SceneLoader detected in scene '{gameObject.scene.name}', destroying this instance. Existing instance will continue working.");

                Destroy(gameObject);
                return;
            }

            base.Awake();

            if (Instance == this)
            {
                // Ensure this GameObject is at root level for DontDestroyOnLoad
                if (transform.parent != null)
                {
                    Debug.LogWarning("[SceneLoader] SceneLoader must be on a root GameObject. Moving to root.");
                    transform.SetParent(null);
                }

                DontDestroyOnLoad(gameObject);

                // Get the Canvas component
                _loadingCanvas = GetComponentInChildren<Canvas>();
                if (_loadingCanvas == null)
                {
                    Debug.LogError("[SceneLoader] No Canvas found in children! Loading screen will not display.");
                }

                // Hide loading screen by default
                HideLoadingScreen(instant: true);

                if (verboseLogging)
                    Debug.Log($"[SceneLoader] Initialized and set to DontDestroyOnLoad (scene: {gameObject.scene.name})");
            }
        }

        /// <summary>
        /// Loads a scene asynchronously with a loading screen
        /// </summary>
        public void LoadScene(string sceneName)
        {
            LoadScene(sceneName, destroyAfterLoad: false);
        }

        /// <summary>
        /// Loads a scene asynchronously with a loading screen
        /// </summary>
        /// <param name="sceneName">Name of the scene to load</param>
        /// <param name="destroyAfterLoad">If true, destroys the SceneLoader after loading completes (for MainMenu transitions)</param>
        public void LoadScene(string sceneName, bool destroyAfterLoad)
        {
            if (_isLoading)
            {
                Debug.LogWarning($"[SceneLoader] Already loading a scene, ignoring request to load '{sceneName}'");
                return;
            }

            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[SceneLoader] Scene name is null or empty!");
                return;
            }

            if (verboseLogging)
                Debug.Log($"[SceneLoader] Starting load sequence for scene: {sceneName} (destroyAfterLoad: {destroyAfterLoad})");

            StartCoroutine(LoadSceneAsyncRoutine(sceneName, destroyAfterLoad));
        }

        /// <summary>
        /// Main loading coroutine with all phases
        /// </summary>
        private IEnumerator LoadSceneAsyncRoutine(string sceneName, bool destroyAfterLoad)
        {
            _isLoading = true;
            float loadStartTime = Time.realtimeSinceStartup;

            // PHASE A: FADE IN (Apparition)
            if (verboseLogging)
                Debug.Log("[SceneLoader] Phase A: Fading in loading screen");

            yield return StartCoroutine(FadeInLoadingScreen());

            // Display random tip
            DisplayRandomTip();

            // PHASE B: CLEANUP (Nettoyage)
            if (verboseLogging)
                Debug.Log("[SceneLoader] Phase B: Cleaning up memory");

            CleanupBeforeLoad();

            // Small delay to let cleanup complete
            yield return new WaitForSecondsRealtime(0.1f);

            // PHASE C: LOADING (Chargement)
            if (verboseLogging)
                Debug.Log($"[SceneLoader] Phase C: Loading scene '{sceneName}'");

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            if (asyncLoad == null)
            {
                Debug.LogError($"[SceneLoader] Failed to start loading scene '{sceneName}'");
                _isLoading = false;
                yield return StartCoroutine(FadeOutLoadingScreen());
                yield break;
            }

            // Prevent the scene from activating immediately
            asyncLoad.allowSceneActivation = false;

            // Update progress bar
            while (!asyncLoad.isDone)
            {
                // LoadSceneAsync progress goes from 0 to 0.9, then jumps to 1.0 when allowSceneActivation = true
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                UpdateProgressBar(progress);

                // Check if we've reached 90% AND minimum loading time has elapsed
                float elapsedTime = Time.realtimeSinceStartup - loadStartTime;
                if (asyncLoad.progress >= 0.9f && elapsedTime >= minimumLoadingTime)
                {
                    if (verboseLogging)
                        Debug.Log($"[SceneLoader] Loading complete (progress: {asyncLoad.progress:F2}, time: {elapsedTime:F2}s)");

                    // PHASE D: FINITION (Allow scene activation)
                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            // Final progress update (100%)
            UpdateProgressBar(1.0f);

            if (verboseLogging)
                Debug.Log($"[SceneLoader] Scene '{sceneName}' loaded successfully");

            // Small delay before fade out
            yield return new WaitForSecondsRealtime(0.2f);

            // PHASE E: FADE OUT (Disparition)
            if (verboseLogging)
                Debug.Log("[SceneLoader] Phase E: Fading out loading screen");

            yield return StartCoroutine(FadeOutLoadingScreen());

            _isLoading = false;

            if (verboseLogging)
                Debug.Log($"[SceneLoader] Load sequence complete for '{sceneName}'");

            // PHASE F: DESTROY (Optional - for MainMenu clean slate)
            if (destroyAfterLoad)
            {
                if (verboseLogging)
                    Debug.Log("[SceneLoader] Destroying SceneLoader after load (MainMenu fresh start)");

                // Destroy after a small delay to ensure everything is settled
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Fades in the loading screen (Alpha 0 -> 1)
        /// </summary>
        private IEnumerator FadeInLoadingScreen()
        {
            if (loadingCanvasGroup == null) yield break;

            // Activate Canvas
            if (_loadingCanvas != null)
            {
                _loadingCanvas.enabled = true;
                _loadingCanvas.gameObject.SetActive(true);
            }

            // Fade in
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime; // Use unscaled time in case timeScale is 0
                float alpha = Mathf.Clamp01(elapsed / fadeDuration);
                loadingCanvasGroup.alpha = alpha;
                yield return null;
            }

            // Ensure fully visible
            loadingCanvasGroup.alpha = 1f;
            loadingCanvasGroup.interactable = true;
            loadingCanvasGroup.blocksRaycasts = true;
        }

        /// <summary>
        /// Fades out the loading screen (Alpha 1 -> 0)
        /// </summary>
        private IEnumerator FadeOutLoadingScreen()
        {
            if (loadingCanvasGroup == null) yield break;

            // Fade out
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
                loadingCanvasGroup.alpha = alpha;
                yield return null;
            }

            // Hide completely
            HideLoadingScreen(instant: true);
        }

        /// <summary>
        /// Hides the loading screen instantly
        /// </summary>
        private void HideLoadingScreen(bool instant)
        {
            if (loadingCanvasGroup == null) return;

            loadingCanvasGroup.alpha = 0f;
            loadingCanvasGroup.interactable = false;
            loadingCanvasGroup.blocksRaycasts = false;

            // Optionally disable canvas to save performance
            if (_loadingCanvas != null && instant)
            {
                _loadingCanvas.enabled = false;
            }
        }

        /// <summary>
        /// Updates the progress bar value
        /// </summary>
        private void UpdateProgressBar(float progress)
        {
            if (progressBar != null)
            {
                progressBar.value = progress;
            }
        }

        /// <summary>
        /// Displays a random tip from the gameTips array
        /// </summary>
        private void DisplayRandomTip()
        {
            if (tipText == null || gameTips == null || gameTips.Length == 0)
                return;

            int randomIndex = Random.Range(0, gameTips.Length);
            tipText.text = gameTips[randomIndex];

            if (verboseLogging)
                Debug.Log($"[SceneLoader] Displaying tip: {gameTips[randomIndex]}");
        }

        /// <summary>
        /// Cleans up memory before loading the new scene
        /// </summary>
        private void CleanupBeforeLoad()
        {
            // Call MemoryManager cleanup if it exists
            if (MemoryManager.Instance != null)
            {
                MemoryManager.ForceCleanup();

                if (verboseLogging)
                    Debug.Log("[SceneLoader] MemoryManager.ForceCleanup() called");
            }
            else
            {
                // Fallback: Force garbage collection manually
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.GC.Collect();

                if (verboseLogging)
                    Debug.Log("[SceneLoader] Manual GC.Collect() called (MemoryManager not found)");
            }

            // Unload unused assets
            Resources.UnloadUnusedAssets();
        }
    }
}
