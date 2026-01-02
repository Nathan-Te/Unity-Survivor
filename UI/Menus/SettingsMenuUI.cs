using UnityEngine;
using UnityEngine.UI;
using SurvivorGame.Localization;
using SurvivorGame.Settings;
using TMPro;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Settings menu panel with manual save mode.
    /// Detects unsaved changes and prompts user before discarding.
    /// </summary>
    public class SettingsMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI placeholderText;

        [Header("Language Selection")]
        [SerializeField] private Button englishButton;
        [SerializeField] private Button frenchButton;

        [Header("Confirmation Popup")]
        [SerializeField] private GameObject confirmationPopup;
        [SerializeField] private TextMeshProUGUI confirmationText;
        [SerializeField] private Button saveChangesButton;
        [SerializeField] private Button discardChangesButton;
        [SerializeField] private Button cancelButton;

        private void Start()
        {
            // Button listeners
            if (backButton) backButton.onClick.AddListener(OnBackPressed);
            if (saveButton) saveButton.onClick.AddListener(OnSavePressed);
            if (englishButton) englishButton.onClick.AddListener(() => SetLanguage(Language.English));
            if (frenchButton) frenchButton.onClick.AddListener(() => SetLanguage(Language.French));

            // Popup listeners
            if (saveChangesButton) saveChangesButton.onClick.AddListener(OnConfirmationSave);
            if (discardChangesButton) discardChangesButton.onClick.AddListener(OnConfirmationDiscard);
            if (cancelButton) cancelButton.onClick.AddListener(OnConfirmationCancel);

            // Subscribe to language changes
            SimpleLocalizationManager.OnLanguageChanged += RefreshText;

            // Hide popup initially
            if (confirmationPopup != null)
            {
                confirmationPopup.SetActive(false);
            }

            RefreshText();
        }

        private void OnDestroy()
        {
            // Remove button listeners
            if (backButton) backButton.onClick.RemoveListener(OnBackPressed);
            if (saveButton) saveButton.onClick.RemoveListener(OnSavePressed);
            if (englishButton) englishButton.onClick.RemoveListener(() => SetLanguage(Language.English));
            if (frenchButton) frenchButton.onClick.RemoveListener(() => SetLanguage(Language.French));

            // Remove popup listeners
            if (saveChangesButton) saveChangesButton.onClick.RemoveListener(OnConfirmationSave);
            if (discardChangesButton) discardChangesButton.onClick.RemoveListener(OnConfirmationDiscard);
            if (cancelButton) cancelButton.onClick.RemoveListener(OnConfirmationCancel);

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

            if (confirmationText != null)
            {
                confirmationText.text = SimpleLocalizationHelper.Get("SETTINGS_UNSAVED_CHANGES", "You have unsaved changes.\nDo you want to save them?");
            }
        }

        private void SetLanguage(Language language)
        {
            if (GameSettingsManager.Instance != null)
            {
                GameSettingsManager.Instance.SetLanguage(language);
                Debug.Log($"[SettingsMenuUI] Language changed to: {language}");
            }
        }

        private void OnSavePressed()
        {
            if (GameSettingsManager.Instance != null)
            {
                GameSettingsManager.Instance.SaveSettings();
                Debug.Log("[SettingsMenuUI] Settings saved manually.");
            }
        }

        private void OnBackPressed()
        {
            // Check for unsaved changes
            if (GameSettingsManager.Instance != null)
            {
                bool hasChanges = GameSettingsManager.Instance.HasUnsavedChanges;
                Debug.Log($"[SettingsMenuUI] OnBackPressed - HasUnsavedChanges: {hasChanges}");

                if (hasChanges)
                {
                    // Only show popup if it's not already visible
                    if (confirmationPopup != null && !confirmationPopup.activeSelf)
                    {
                        ShowConfirmationPopup();
                    }
                }
                else
                {
                    ReturnToMainMenu();
                }
            }
            else
            {
                ReturnToMainMenu();
            }
        }

        private void ShowConfirmationPopup()
        {
            if (confirmationPopup != null)
            {
                confirmationPopup.SetActive(true);
                Debug.Log("[SettingsMenuUI] Showing unsaved changes confirmation popup.");
            }
        }

        private void HideConfirmationPopup()
        {
            if (confirmationPopup != null)
            {
                confirmationPopup.SetActive(false);
            }
        }

        private void OnConfirmationSave()
        {
            // Save changes and return to main menu
            if (GameSettingsManager.Instance != null)
            {
                GameSettingsManager.Instance.SaveSettings();
                Debug.Log("[SettingsMenuUI] User chose to save changes.");
            }

            HideConfirmationPopup();
            ReturnToMainMenu();
        }

        private void OnConfirmationDiscard()
        {
            // Discard changes and return to main menu
            if (GameSettingsManager.Instance != null)
            {
                GameSettingsManager.Instance.DiscardChanges();
                Debug.Log("[SettingsMenuUI] User chose to discard changes.");
            }

            HideConfirmationPopup();
            ReturnToMainMenu();
        }

        private void OnConfirmationCancel()
        {
            // Just close popup, stay in settings menu
            HideConfirmationPopup();
            Debug.Log("[SettingsMenuUI] User canceled exit, staying in settings.");
        }

        private void ReturnToMainMenu()
        {
            var mainMenu = FindFirstObjectByType<MainMenuUI>();
            if (mainMenu != null)
            {
                mainMenu.ReturnToMainMenu();
            }
        }
    }
}
