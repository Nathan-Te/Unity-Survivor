using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using SurvivorGame.Localization;

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
        [SerializeField] private string[] gameTipKeys = new string[]
        {
            "LOADING_TIP_1",
            "LOADING_TIP_2",
            "LOADING_TIP_3",
            "LOADING_TIP_4",
            "LOADING_TIP_5",
            "LOADING_TIP_6",
            "LOADING_TIP_7",
            "LOADING_TIP_8",
            "LOADING_TIP_9",
            "LOADING_TIP_10",
            "LOADING_TIP_11",
            "LOADING_TIP_12",
            "LOADING_TIP_13",
            "LOADING_TIP_14",
            "LOADING_TIP_15"
        };

        [Header("Tip Rotation")]
        [Tooltip("Interval in seconds to rotate tips during long loads")]
        [SerializeField] private float tipRotationInterval = 5f;

        [Header("Settings")]
        [SerializeField] private bool verboseLogging = false;

        private Canvas _loadingCanvas;
        private bool _isLoading = false;
        private Coroutine _tipRotationCoroutine;

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

            // CRITICAL: Initialize tip and reset progress BEFORE fade-in (prevents visible swap)
            ResetProgressBar();
            DisplayRandomTip();

            // PHASE A: FADE IN (Apparition)
            if (verboseLogging)
                Debug.Log("[SceneLoader] Phase A: Fading in loading screen");

            yield return StartCoroutine(FadeInLoadingScreen());

            // Start tip rotation for long loads
            _tipRotationCoroutine = StartCoroutine(RotateTipsCoroutine());

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

            // Stop tip rotation
            if (_tipRotationCoroutine != null)
            {
                StopCoroutine(_tipRotationCoroutine);
                _tipRotationCoroutine = null;
            }

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
        /// Displays a random localized tip from the gameTipKeys array
        /// </summary>
        private void DisplayRandomTip()
        {
            if (tipText == null || gameTipKeys == null || gameTipKeys.Length == 0)
                return;

            int randomIndex = Random.Range(0, gameTipKeys.Length);
            string tipKey = gameTipKeys[randomIndex];
            string localizedTip = SimpleLocalizationHelper.Get(tipKey, "Loading...");
            tipText.text = localizedTip;

            if (verboseLogging)
                Debug.Log($"[SceneLoader] Displaying tip: {tipKey} = {localizedTip}");
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

        /// <summary>
        /// Rotates tips at regular intervals during long loads
        /// </summary>
        private IEnumerator RotateTipsCoroutine()
        {
            while (_isLoading)
            {
                yield return new WaitForSecondsRealtime(tipRotationInterval);

                if (_isLoading)
                {
                    DisplayRandomTip();

                    if (verboseLogging)
                        Debug.Log($"[SceneLoader] Tip rotated after {tipRotationInterval}s");
                }
            }
        }

        /// <summary>
        /// Resets the progress bar to 0
        /// </summary>
        private void ResetProgressBar()
        {
            if (progressBar != null)
            {
                progressBar.value = 0f;

                if (verboseLogging)
                    Debug.Log("[SceneLoader] Progress bar reset to 0");
            }
        }
    }
}
