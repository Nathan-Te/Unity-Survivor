using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SurvivorGame.Progression;
using SurvivorGame.Localization;
using SurvivorGame.Settings;
using TMPro;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Main menu controller with navigation to all game modes and meta-progression screens.
    /// Displays player gold and manages transitions between menu panels.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private CanvasGroup mainMenuPanel;
        [SerializeField] private CanvasGroup levelSelectionPanel;
        [SerializeField] private CanvasGroup upgradesPanel;
        [SerializeField] private CanvasGroup settingsPanel;
        [SerializeField] private CanvasGroup leaderboardPanel;

        [Header("Main Menu Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button upgradesButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button leaderboardButton;
        [SerializeField] private Button quitButton;

        [Header("Player Info Display")]
        [SerializeField] private TextMeshProUGUI goldText;

        [Header("Settings")]
        [SerializeField] private bool verboseLogging = false;

        private CanvasGroup _currentPanel;

        private void Awake()
        {
            // CRITICAL: Ensure the main Canvas is always active when MainMenu scene loads
            // This fixes the issue where returning from game scene leaves Canvas disabled
            Canvas mainCanvas = GetComponentInParent<Canvas>();

            Debug.Log($"[MainMenuUI] Awake() - Looking for Canvas parent... Scene: {gameObject.scene.name}");

            if (mainCanvas != null)
            {
                Debug.Log($"[MainMenuUI] Found Canvas: {mainCanvas.name} (active: {mainCanvas.gameObject.activeSelf}, enabled: {mainCanvas.enabled})");

                mainCanvas.enabled = true;
                mainCanvas.gameObject.SetActive(true);

                Debug.Log($"[MainMenuUI] Canvas re-activated: {mainCanvas.name}");
            }
            else
            {
                Debug.LogError("[MainMenuUI] CRITICAL: No Canvas found in parent hierarchy! MainMenu UI will not display.");
                Debug.LogError($"[MainMenuUI] This GameObject: {gameObject.name}, Scene: {gameObject.scene.name}");
            }

            // CRITICAL: Force reset to main menu panel (hide all sub-panels)
            // This ensures we always start at the main menu, not at a sub-panel
            ResetToMainMenuPanel();

            // CRITICAL: Initialize managers BEFORE any other UI scripts' Start() methods execute
            // This ensures ProgressionManager exists when LeaderboardUI, LevelSelectionUI, etc. try to access it
            InitializeGameSettings();

            // Subscribe to progression events if ProgressionManager already existed
            // (if it was newly created, InitializeGameSettings() already subscribed)
            SubscribeToProgressionEvents();
        }

        /// <summary>
        /// Forces all panels to their initial state (main menu visible, all others hidden)
        /// </summary>
        private void ResetToMainMenuPanel()
        {
            // Hide ALL panels first
            if (levelSelectionPanel != null) SetPanelVisible(levelSelectionPanel, false);
            if (upgradesPanel != null) SetPanelVisible(upgradesPanel, false);
            if (settingsPanel != null) SetPanelVisible(settingsPanel, false);
            if (leaderboardPanel != null) SetPanelVisible(leaderboardPanel, false);

            // Show main menu panel
            if (mainMenuPanel != null)
            {
                SetPanelVisible(mainMenuPanel, true);
                _currentPanel = mainMenuPanel;
            }

            if (verboseLogging)
                Debug.Log("[MainMenuUI] Reset to main menu panel");
        }

        private void Start()
        {
            // Ensure time is running (in case we came from a paused game scene)
            Time.timeScale = 1f;

            // Validate critical UI references
            if (goldText == null)
            {
                Debug.LogError("[MainMenuUI] goldText is not assigned in Inspector! Player gold will not display. Please assign the TextMeshProUGUI component in the MainMenuUI Inspector.");
            }

            // Wire up main menu buttons
            if (playButton) playButton.onClick.AddListener(OpenLevelSelection);
            if (upgradesButton) upgradesButton.onClick.AddListener(OpenUpgrades);
            if (settingsButton) settingsButton.onClick.AddListener(OpenSettings);
            if (leaderboardButton) leaderboardButton.onClick.AddListener(OpenLeaderboard);
            if (quitButton) quitButton.onClick.AddListener(QuitGame);

            // Subscribe to language changes
            SimpleLocalizationManager.OnLanguageChanged += RefreshText;

            // Note: ProgressionManager initialization moved to Awake() to ensure it exists
            // before other UI scripts' Start() methods execute

            // Note: Panel visibility is already set in Awake() via ResetToMainMenuPanel()
            // No need to call ShowPanel again

            // Update player info display IMMEDIATELY (synchronous)
            RefreshPlayerInfo();

            // Also update with a delay as fallback
            Invoke(nameof(RefreshPlayerInfo), 0.1f);

            if (verboseLogging)
                Debug.Log("[MainMenuUI] Initialized");
        }

        private bool _subscribedToProgression = false;

        /// <summary>
        /// Subscribes to ProgressionManager events (can be called before ProgressionManager exists)
        /// </summary>
        private void SubscribeToProgressionEvents()
        {
            if (_subscribedToProgression)
            {
                if (verboseLogging)
                    Debug.Log("[MainMenuUI] Already subscribed to ProgressionManager events, skipping");
                return;
            }

            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.OnProgressionLoaded += UpdatePlayerInfo;
                ProgressionManager.Instance.OnProgressionChanged += UpdatePlayerInfo;
                _subscribedToProgression = true;

                if (verboseLogging)
                    Debug.Log("[MainMenuUI] Subscribed to existing ProgressionManager events");
            }
            else
            {
                if (verboseLogging)
                    Debug.Log("[MainMenuUI] ProgressionManager doesn't exist yet, will subscribe after creation");
            }
        }

        /// <summary>
        /// Initializes core managers if they don't exist.
        /// This ensures everything is ready when MainMenu loads.
        /// IMPORTANT: SimpleLocalizationManager must exist BEFORE GameSettingsManager initializes.
        /// </summary>
        private void InitializeGameSettings()
        {
            Debug.Log("[MainMenuUI] InitializeGameSettings() called");

            bool createdNewProgressionManager = false;

            // 1. ProgressionManager - create if doesn't exist
            // CRITICAL: Use FindFirstObjectByType instead of Instance getter to avoid Singleton error
            var existingProgression = FindFirstObjectByType<ProgressionManager>();
            if (existingProgression == null)
            {
                GameObject progressionManagerObj = new GameObject("ProgressionManager");
                progressionManagerObj.AddComponent<ProgressionManager>();
                createdNewProgressionManager = true;
                Debug.Log("[MainMenuUI] Created ProgressionManager");
            }
            else
            {
                Debug.Log("[MainMenuUI] ProgressionManager already exists");
            }

            // CRITICAL: If we just created ProgressionManager, subscribe to its events NOW
            // This is necessary because ProgressionManager.Awake() has already run (synchronously)
            // and triggered OnProgressionLoaded before we could subscribe in Start()
            if (createdNewProgressionManager && ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.OnProgressionLoaded += UpdatePlayerInfo;
                ProgressionManager.Instance.OnProgressionChanged += UpdatePlayerInfo;
                _subscribedToProgression = true;
                Debug.Log("[MainMenuUI] Subscribed to newly created ProgressionManager events");

                // Force immediate update since we missed the OnProgressionLoaded event
                if (ProgressionManager.Instance.CurrentProgression != null)
                {
                    UpdatePlayerInfo(ProgressionManager.Instance.CurrentProgression);
                    Debug.Log($"[MainMenuUI] Force-updated UI with progression data: Gold={ProgressionManager.Instance.CurrentProgression.gold}");
                }
            }

            // 2. GameSettingsManager - create if doesn't exist
            var existingSettings = FindFirstObjectByType<GameSettingsManager>();
            if (existingSettings == null)
            {
                GameObject settingsManagerObj = new GameObject("GameSettingsManager");
                settingsManagerObj.AddComponent<GameSettingsManager>();
                Debug.Log("[MainMenuUI] Created GameSettingsManager");
            }
            else
            {
                Debug.Log("[MainMenuUI] GameSettingsManager already exists");
            }

            Debug.Log("[MainMenuUI] InitializeGameSettings() completed");
        }

        /// <summary>
        /// Refreshes player info display (called with delay to ensure ProgressionManager is ready)
        /// </summary>
        private void RefreshPlayerInfo()
        {
            if (ProgressionManager.Instance != null && ProgressionManager.Instance.CurrentProgression != null)
            {
                UpdatePlayerInfo(ProgressionManager.Instance.CurrentProgression);

                if (verboseLogging)
                    Debug.Log($"[MainMenuUI] Refreshed player info - Gold: {ProgressionManager.Instance.CurrentProgression.gold}");
            }
            else
            {
                // Fallback: Display 0 gold if ProgressionManager isn't ready yet
                if (goldText != null)
                {
                    //goldText.text = SimpleLocalizationHelper.FormatGold(0);
                    goldText.text = "0";
                }

                if (verboseLogging)
                    Debug.LogWarning("[MainMenuUI] ProgressionManager not ready, displaying 0 gold as fallback");
            }
        }

        private void OnDestroy()
        {
            // Cleanup button listeners
            if (playButton) playButton.onClick.RemoveListener(OpenLevelSelection);
            if (upgradesButton) upgradesButton.onClick.RemoveListener(OpenUpgrades);
            if (settingsButton) settingsButton.onClick.RemoveListener(OpenSettings);
            if (leaderboardButton) leaderboardButton.onClick.RemoveListener(OpenLeaderboard);
            if (quitButton) quitButton.onClick.RemoveListener(QuitGame);

            // Cleanup progression listeners
            // Use FindFirstObjectByType to avoid Singleton getter error during scene unload
            var progressionManager = FindFirstObjectByType<ProgressionManager>();
            if (progressionManager != null)
            {
                progressionManager.OnProgressionLoaded -= UpdatePlayerInfo;
                progressionManager.OnProgressionChanged -= UpdatePlayerInfo;
            }
            _subscribedToProgression = false;

            // Cleanup localization listener
            SimpleLocalizationManager.OnLanguageChanged -= RefreshText;
        }

        /// <summary>
        /// Updates the gold display
        /// </summary>
        private void UpdatePlayerInfo(PlayerProgressionData data)
        {
            if (data == null)
            {
                if (verboseLogging)
                    Debug.LogWarning("[MainMenuUI] UpdatePlayerInfo called with null data");
                return;
            }

            if (goldText != null)
            {
                // SAFETY: Wait for SimpleLocalizationManager if it doesn't exist yet
                if (SimpleLocalizationManager.Instance == null)
                {
                    // Use simple fallback during initialization
                    goldText.text = data.gold.ToString();
                    if (verboseLogging)
                        Debug.Log($"[MainMenuUI] SimpleLocalizationManager not ready yet, using simple display: {data.gold}");
                    return;
                }

                // Use localized gold display
                string formattedGold = data.gold.ToString();
                //string formattedGold = SimpleLocalizationHelper.FormatGold(data.gold);

                if (string.IsNullOrEmpty(formattedGold))
                {
                    goldText.text = data.gold.ToString(); // Fallback
                }
                else
                {
                    goldText.text = formattedGold;
                }

                if (verboseLogging)
                    Debug.Log($"[MainMenuUI] Updated gold display: {goldText.text}");
            }
            else
            {
                Debug.LogWarning("[MainMenuUI] goldText is null, cannot update display");
            }
        }

        /// <summary>
        /// Checks if goldText is still correct after one frame
        /// </summary>
        private System.Collections.IEnumerator CheckGoldTextAfterFrame()
        {
            yield return null; // Wait 1 frame
            if (goldText != null)
            {
                Debug.Log($"[MainMenuUI] Gold text after 1 frame: '{goldText.text}' (should NOT be empty)");
                if (string.IsNullOrEmpty(goldText.text))
                {
                    Debug.LogError("[MainMenuUI] CRITICAL: Gold text was CLEARED after assignment! Something is overwriting it.");
                }
            }
        }

        /// <summary>
        /// Refreshes all localized text
        /// </summary>
        private void RefreshText()
        {
            UpdatePlayerInfo(ProgressionManager.Instance?.CurrentProgression);
        }

        /// <summary>
        /// Opens the level selection panel
        /// </summary>
        public void OpenLevelSelection()
        {
            if (verboseLogging)
                Debug.Log("[MainMenuUI] Opening Level Selection");

            ShowPanel(levelSelectionPanel);
        }

        /// <summary>
        /// Opens the upgrades panel
        /// </summary>
        public void OpenUpgrades()
        {
            if (verboseLogging)
                Debug.Log("[MainMenuUI] Opening Upgrades");

            ShowPanel(upgradesPanel);
        }

        /// <summary>
        /// Opens the settings panel
        /// </summary>
        public void OpenSettings()
        {
            if (verboseLogging)
                Debug.Log("[MainMenuUI] Opening Settings");

            ShowPanel(settingsPanel);
        }

        /// <summary>
        /// Opens the leaderboard panel
        /// </summary>
        public void OpenLeaderboard()
        {
            if (verboseLogging)
                Debug.Log("[MainMenuUI] Opening Leaderboard");

            ShowPanel(leaderboardPanel);
        }

        /// <summary>
        /// Returns to the main menu panel
        /// </summary>
        public void ReturnToMainMenu()
        {
            if (verboseLogging)
                Debug.Log("[MainMenuUI] Returning to Main Menu");

            ShowPanel(mainMenuPanel);
        }

        /// <summary>
        /// Shows a specific panel and hides all others
        /// </summary>
        private void ShowPanel(CanvasGroup panel)
        {
            if (panel == null)
            {
                Debug.LogWarning("[MainMenuUI] Attempted to show null panel");
                return;
            }

            // Hide current panel
            if (_currentPanel != null)
            {
                SetPanelVisible(_currentPanel, false);
            }

            // Show new panel
            SetPanelVisible(panel, true);
            _currentPanel = panel;
        }

        /// <summary>
        /// Shows/hides a panel using CanvasGroup (avoids Canvas rebuild)
        /// </summary>
        private void SetPanelVisible(CanvasGroup panel, bool visible)
        {
            if (panel == null) return;

            panel.alpha = visible ? 1f : 0f;
            panel.interactable = visible;
            panel.blocksRaycasts = visible;
        }

        /// <summary>
        /// Quits the game
        /// </summary>
        private void QuitGame()
        {
            if (verboseLogging)
                Debug.Log("[MainMenuUI] Quitting game");

            GameStateManager.QuitGame();
        }
    }
}
