using UnityEngine;
using UnityEngine.UI;

namespace SurvivorGame.Localization
{
    /// <summary>
    /// Simple language selector with buttons.
    /// Attach to your settings menu and assign buttons.
    /// </summary>
    public class SimpleLanguageSelector : MonoBehaviour
    {
        [Header("Language Buttons")]
        [SerializeField] private Button englishButton;
        [SerializeField] private Button frenchButton;

        [Header("Visual Feedback (Optional)")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = new Color(1f, 0.84f, 0f); // Gold

        private void Start()
        {
            if (englishButton != null)
            {
                englishButton.onClick.AddListener(() => SetLanguage(Language.English));
            }

            if (frenchButton != null)
            {
                frenchButton.onClick.AddListener(() => SetLanguage(Language.French));
            }

            UpdateButtonVisuals();
            SimpleLocalizationManager.OnLanguageChanged += UpdateButtonVisuals;
        }

        private void SetLanguage(Language language)
        {
            if (SimpleLocalizationManager.Instance != null)
            {
                SimpleLocalizationManager.Instance.SetLanguage(language);
            }
        }

        private void UpdateButtonVisuals()
        {
            if (SimpleLocalizationManager.Instance == null) return;

            Language current = SimpleLocalizationManager.Instance.CurrentLanguage;

            if (englishButton != null)
            {
                var colors = englishButton.colors;
                colors.normalColor = current == Language.English ? selectedColor : normalColor;
                englishButton.colors = colors;
            }

            if (frenchButton != null)
            {
                var colors = frenchButton.colors;
                colors.normalColor = current == Language.French ? selectedColor : normalColor;
                frenchButton.colors = colors;
            }
        }

        private void OnDestroy()
        {
            if (englishButton != null)
            {
                englishButton.onClick.RemoveAllListeners();
            }

            if (frenchButton != null)
            {
                frenchButton.onClick.RemoveAllListeners();
            }

            SimpleLocalizationManager.OnLanguageChanged -= UpdateButtonVisuals;
        }
    }
}
