using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SurvivorGame.Localization
{
    /// <summary>
    /// Simple UI component for language selection.
    /// Attach to a dropdown or buttons in your settings menu.
    /// </summary>
    public class LanguageSelectorUI : MonoBehaviour
    {
        [Header("Option 1: Dropdown")]
        [SerializeField] private TMP_Dropdown languageDropdown;

        [Header("Option 2: Individual Buttons")]
        [SerializeField] private Button englishButton;
        [SerializeField] private Button frenchButton;

        [Header("Current Language Display (Optional)")]
        [SerializeField] private TextMeshProUGUI currentLanguageText;

        private void Start()
        {
            InitializeDropdown();
            InitializeButtons();
            UpdateCurrentLanguageDisplay();

            // Listen for language changes to update display
            LocalizationManager.OnLanguageChanged += UpdateCurrentLanguageDisplay;
        }

        private void InitializeDropdown()
        {
            if (languageDropdown != null)
            {
                // Clear existing options
                languageDropdown.ClearOptions();

                // Add language options
                languageDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "English",
                    "Français"
                });

                // Set current selection
                if (LocalizationManager.Instance != null)
                {
                    languageDropdown.value = (int)LocalizationManager.Instance.CurrentLanguage;
                }

                // Subscribe to value changes
                languageDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            }
        }

        private void InitializeButtons()
        {
            if (englishButton != null)
            {
                englishButton.onClick.AddListener(() => SetLanguage(Language.English));

                // Optional: Highlight current language button
                UpdateButtonHighlight();
            }

            if (frenchButton != null)
            {
                frenchButton.onClick.AddListener(() => SetLanguage(Language.French));

                // Optional: Highlight current language button
                UpdateButtonHighlight();
            }
        }

        private void OnDropdownValueChanged(int index)
        {
            Language selectedLanguage = (Language)index;
            SetLanguage(selectedLanguage);
        }

        private void SetLanguage(Language language)
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.SetLanguage(language);
                UpdateButtonHighlight();
            }
        }

        private void UpdateButtonHighlight()
        {
            if (LocalizationManager.Instance == null) return;

            Language currentLanguage = LocalizationManager.Instance.CurrentLanguage;

            // Highlight current language button (optional visual feedback)
            if (englishButton != null)
            {
                var colors = englishButton.colors;
                colors.normalColor = currentLanguage == Language.English
                    ? new Color(1f, 0.84f, 0f) // Gold
                    : Color.white;
                englishButton.colors = colors;
            }

            if (frenchButton != null)
            {
                var colors = frenchButton.colors;
                colors.normalColor = currentLanguage == Language.French
                    ? new Color(1f, 0.84f, 0f) // Gold
                    : Color.white;
                frenchButton.colors = colors;
            }
        }

        private void UpdateCurrentLanguageDisplay()
        {
            if (currentLanguageText != null && LocalizationManager.Instance != null)
            {
                Language currentLanguage = LocalizationManager.Instance.CurrentLanguage;
                currentLanguageText.text = currentLanguage switch
                {
                    Language.English => "English",
                    Language.French => "Français",
                    _ => currentLanguage.ToString()
                };
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (languageDropdown != null)
            {
                languageDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
            }

            if (englishButton != null)
            {
                englishButton.onClick.RemoveAllListeners();
            }

            if (frenchButton != null)
            {
                frenchButton.onClick.RemoveAllListeners();
            }

            LocalizationManager.OnLanguageChanged -= UpdateCurrentLanguageDisplay;
        }
    }
}
