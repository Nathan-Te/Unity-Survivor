using UnityEngine;
using UnityEngine.UI;
using SurvivorGame.Localization;
using SurvivorGame.Progression;
using TMPro;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Leaderboard panel (placeholder for future implementation).
    /// Will display player stats, best runs, achievements, etc.
    /// </summary>
    public class LeaderboardUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI statsText;

        private void Start()
        {
            if (backButton) backButton.onClick.AddListener(OnBackPressed);

            // Subscribe to progression changes
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.OnProgressionChanged += OnProgressionChanged;
            }

            // Subscribe to language changes
            SimpleLocalizationManager.OnLanguageChanged += RefreshText;

            RefreshText();
        }

        private void OnDestroy()
        {
            if (backButton) backButton.onClick.RemoveListener(OnBackPressed);

            // Use FindFirstObjectByType to avoid Singleton getter error during scene unload
            var progressionManager = FindFirstObjectByType<ProgressionManager>();
            if (progressionManager != null)
            {
                progressionManager.OnProgressionChanged -= OnProgressionChanged;
            }

            SimpleLocalizationManager.OnLanguageChanged -= RefreshText;
        }

        private void OnProgressionChanged(PlayerProgressionData data)
        {
            RefreshText();
        }

        private void RefreshText()
        {
            if (titleText != null)
            {
                titleText.text = SimpleLocalizationHelper.Get("MENU_LEADERBOARD", "Leaderboard");
            }

            if (statsText != null)
            {
                var data = ProgressionManager.Instance?.CurrentProgression;
                if (data != null)
                {
                    // Display basic stats (placeholder until full leaderboard is implemented)
                    statsText.text = $"Total Runs: {data.totalRunsCompleted}\n" +
                                   $"Total Enemies Killed: {data.totalEnemiesKilled}\n" +
                                   $"Best Run Time: {FormatTime(data.bestRunTime)}\n" +
                                   $"Highest Level: {data.highestLevel}\n\n" +
                                   SimpleLocalizationHelper.Get("MENU_LEADERBOARD_PLACEHOLDER", "Full leaderboard coming soon!");
                }
                else
                {
                    statsText.text = SimpleLocalizationHelper.Get("MENU_LEADERBOARD_NO_DATA", "No data available");
                }
            }
        }

        private string FormatTime(float seconds)
        {
            if (seconds <= 0) return "N/A";

            int minutes = Mathf.FloorToInt(seconds / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            return $"{minutes:00}:{secs:00}";
        }

        private void OnBackPressed()
        {
            var mainMenu = FindFirstObjectByType<MainMenuUI>();
            if (mainMenu != null)
            {
                mainMenu.ReturnToMainMenu();
            }
        }
    }
}
