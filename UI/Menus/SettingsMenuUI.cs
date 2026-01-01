using UnityEngine;
using UnityEngine.UI;
using SurvivorGame.Localization;
using TMPro;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Settings menu panel (placeholder for future implementation).
    /// Will contain audio, graphics, controls, and language settings.
    /// </summary>
    public class SettingsMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI placeholderText;

        [Header("Language Selection")]
        [SerializeField] private Button englishButton;
        [SerializeField] private Button frenchButton;

        private void Start()
        {
            if (backButton) backButton.onClick.AddListener(OnBackPressed);
            if (englishButton) englishButton.onClick.AddListener(() => SetLanguage(Language.English));
            if (frenchButton) frenchButton.onClick.AddListener(() => SetLanguage(Language.French));

            // Subscribe to language changes
            SimpleLocalizationManager.OnLanguageChanged += RefreshText;

            RefreshText();
        }

        private void OnDestroy()
        {
            if (backButton) backButton.onClick.RemoveListener(OnBackPressed);
            if (englishButton) englishButton.onClick.RemoveListener(() => SetLanguage(Language.English));
            if (frenchButton) frenchButton.onClick.RemoveListener(() => SetLanguage(Language.French));

            SimpleLocalizationManager.OnLanguageChanged -= RefreshText;
        }

        private void RefreshText()
        {
            if (titleText != null)
            {
                titleText.text = SimpleLocalizationHelper.Get("MENU_SETTINGS", "Settings");
            }

            if (placeholderText != null)
            {
                placeholderText.text = SimpleLocalizationHelper.Get("MENU_SETTINGS_PLACEHOLDER", "Settings menu coming soon!\n\nFeatures:\n- Audio Settings\n- Graphics Options\n- Controls Configuration\n- Language Selection");
            }
        }

        private void SetLanguage(Language language)
        {
            if (SimpleLocalizationManager.Instance != null)
            {
                SimpleLocalizationManager.Instance.SetLanguage(language);
                Debug.Log($"[SettingsMenuUI] Language changed to: {language}");
            }
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
