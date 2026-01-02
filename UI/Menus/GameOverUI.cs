using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using SurvivorGame.Localization;
using SurvivorGame.Progression;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Game Over screen with run statistics and navigation options.
    /// Displays: time survived, level reached, enemies killed, score, gold collected.
    /// Options: Retry (restart level), Main Menu (return to menu).
    /// Automatically saves progression in both cases.
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI timeSurvivedText;
        [SerializeField] private TextMeshProUGUI levelReachedText;
        [SerializeField] private TextMeshProUGUI enemiesKilledText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI goldCollectedText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Settings")]
        [SerializeField] private bool verboseLogging = false;

        private bool _isShowing = false;
        private int _goldAtStartOfRun = 0;

        private void Start()
        {
            // Wire up buttons
            if (retryButton) retryButton.onClick.AddListener(OnRetryClicked);
            if (mainMenuButton) mainMenuButton.onClick.AddListener(OnMainMenuClicked);

            // Subscribe to language changes
            SimpleLocalizationManager.OnLanguageChanged += RefreshText;

            // Hide by default
            SetVisible(false);

            if (verboseLogging)
                Debug.Log("[GameOverUI] Initialized");
        }

        private void OnDestroy()
        {
            if (retryButton) retryButton.onClick.RemoveListener(OnRetryClicked);
            if (mainMenuButton) mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);

            SimpleLocalizationManager.OnLanguageChanged -= RefreshText;
        }

        /// <summary>
        /// Called at the start of a run to record initial gold
        /// </summary>
        public void OnRunStart()
        {
            if (GoldManager.Instance != null)
            {
                _goldAtStartOfRun = GoldManager.Instance.CurrentSessionGold;
            }
        }

        /// <summary>
        /// Shows the Game Over screen with run statistics
        /// </summary>
        public void Show(float timeSurvived, int levelReached, int enemiesKilled, int score)
        {
            if (_isShowing) return;
            _isShowing = true;

            // Get gold collected during this run
            int goldCollected = 0;
            if (GoldManager.Instance != null)
            {
                goldCollected = GoldManager.Instance.CurrentSessionGold - _goldAtStartOfRun;

                // Convert session gold to total (persistent gold)
                GoldManager.Instance.ConvertSessionGoldToTotal();
            }

            // Award the gold to ProgressionManager (for meta-progression)
            if (ProgressionManager.Instance != null && GoldManager.Instance != null)
            {
                ProgressionManager.Instance.AwardGold(GoldManager.Instance.TotalGold - ProgressionManager.Instance.CurrentProgression.gold);
            }

            // Record run statistics (including score)
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.RecordRunStats(
                    enemiesKilled,
                    timeSurvived,
                    levelReached,
                    score
                );
            }

            // Save progression (important for both Retry and Main Menu)
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.SaveProgression();
            }

            // Display statistics
            DisplayStatistics(timeSurvived, levelReached, enemiesKilled, score, goldCollected);

            // Show UI
            SetVisible(true);

            if (verboseLogging)
                Debug.Log($"[GameOverUI] Showing Game Over: Time={timeSurvived:F1}s, Level={levelReached}, Kills={enemiesKilled}, Score={score}, Gold={goldCollected}");
        }

        /// <summary>
        /// Displays run statistics in the UI
        /// </summary>
        private void DisplayStatistics(float timeSurvived, int levelReached, int enemiesKilled, int score, int goldCollected)
        {
            // Title
            if (titleText != null)
            {
                titleText.text = SimpleLocalizationHelper.Get("GAMEOVER_TITLE", "GAME OVER");
            }

            // Time survived
            if (timeSurvivedText != null)
            {
                string formattedTime = FormatTime(timeSurvived);
                timeSurvivedText.text = SimpleLocalizationHelper.GetFormatted("GAMEOVER_TIME", formattedTime);
            }

            // Level reached
            if (levelReachedText != null)
            {
                levelReachedText.text = SimpleLocalizationHelper.GetFormatted("GAMEOVER_LEVEL", levelReached);
            }

            // Enemies killed
            if (enemiesKilledText != null)
            {
                enemiesKilledText.text = SimpleLocalizationHelper.GetFormatted("GAMEOVER_KILLS", enemiesKilled);
            }

            // Score
            if (scoreText != null)
            {
                scoreText.text = SimpleLocalizationHelper.GetFormatted("GAMEOVER_SCORE", score);
            }

            // Gold collected
            if (goldCollectedText != null)
            {
                goldCollectedText.text = SimpleLocalizationHelper.GetFormatted("GAMEOVER_GOLD", goldCollected);
            }
        }

        /// <summary>
        /// Formats time in MM:SS format
        /// </summary>
        private string FormatTime(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            return $"{minutes:00}:{secs:00}";
        }

        /// <summary>
        /// Refreshes localized text
        /// </summary>
        private void RefreshText()
        {
            // Refresh static text (buttons, title)
            // Note: Statistics will be refreshed when Show() is called again
            if (titleText != null)
            {
                titleText.text = SimpleLocalizationHelper.Get("GAMEOVER_TITLE", "GAME OVER");
            }
        }

        /// <summary>
        /// Restarts the current level
        /// </summary>
        private void OnRetryClicked()
        {
            if (verboseLogging)
                Debug.Log("[GameOverUI] Retry clicked - restarting game");

            // Use GameStateManager for proper restart
            GameStateManager.RestartGame();
        }

        /// <summary>
        /// Returns to the main menu
        /// </summary>
        private void OnMainMenuClicked()
        {
            if (verboseLogging)
                Debug.Log("[GameOverUI] Main Menu clicked - returning to main menu");

            // Use GameStateManager for proper cleanup and menu transition
            GameStateManager.ReturnToMainMenu();
        }

        /// <summary>
        /// Shows/hides the Game Over UI
        /// </summary>
        private void SetVisible(bool visible)
        {
            if (canvasGroup == null) return;

            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;

            if (!visible)
            {
                _isShowing = false;
            }
        }
    }
}
