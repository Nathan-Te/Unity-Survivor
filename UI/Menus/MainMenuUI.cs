using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SurvivorGame.Progression;
using SurvivorGame.Localization;
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

        private void Start()
        {
            // Wire up main menu buttons
            if (playButton) playButton.onClick.AddListener(OpenLevelSelection);
            if (upgradesButton) upgradesButton.onClick.AddListener(OpenUpgrades);
            if (settingsButton) settingsButton.onClick.AddListener(OpenSettings);
            if (leaderboardButton) leaderboardButton.onClick.AddListener(OpenLeaderboard);
            if (quitButton) quitButton.onClick.AddListener(QuitGame);

            // Subscribe to progression changes
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.OnProgressionLoaded += UpdatePlayerInfo;
                ProgressionManager.Instance.OnProgressionChanged += UpdatePlayerInfo;
            }

            // Subscribe to language changes
            SimpleLocalizationManager.OnLanguageChanged += RefreshText;

            // Show main menu by default
            ShowPanel(mainMenuPanel);

            // Update player info display
            UpdatePlayerInfo(ProgressionManager.Instance?.CurrentProgression);

            if (verboseLogging)
                Debug.Log("[MainMenuUI] Initialized");
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
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.OnProgressionLoaded -= UpdatePlayerInfo;
                ProgressionManager.Instance.OnProgressionChanged -= UpdatePlayerInfo;
            }

            // Cleanup localization listener
            SimpleLocalizationManager.OnLanguageChanged -= RefreshText;
        }

        /// <summary>
        /// Updates the gold display
        /// </summary>
        private void UpdatePlayerInfo(PlayerProgressionData data)
        {
            if (data == null) return;

            if (goldText != null)
            {
                goldText.text = SimpleLocalizationHelper.FormatGold(data.gold);
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
