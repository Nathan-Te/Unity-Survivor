using UnityEngine;
using System;
using System.Collections.Generic;

namespace SurvivorGame.Localization
{
    /// <summary>
    /// ScriptableObject that holds localized versions of a single string.
    /// Used for data-driven content like rune names, descriptions, enemy names.
    /// Create via: Assets > Create > Localization > Localized String
    /// </summary>
    [CreateAssetMenu(fileName = "New Localized String", menuName = "Localization/Localized String")]
    public class LocalizedString : ScriptableObject
    {
        [Header("Translations")]
        [SerializeField] private List<LanguageEntry> _translations = new List<LanguageEntry>();

        [Serializable]
        public class LanguageEntry
        {
            public Language Language;
            [TextArea(2, 5)]
            public string Text;
        }

        /// <summary>
        /// Gets the text for the current language from LocalizationManager.
        /// Falls back to English if current language not found.
        /// </summary>
        public string GetText()
        {
            if (LocalizationManager.Instance == null)
            {
                Debug.LogWarning("LocalizationManager not found. Returning first available translation.");
                return _translations.Count > 0 ? _translations[0].Text : "";
            }

            return GetText(LocalizationManager.Instance.CurrentLanguage);
        }

        /// <summary>
        /// Gets the text for a specific language.
        /// </summary>
        /// <param name="language">Target language</param>
        /// <param name="fallbackToEnglish">If true, falls back to English if language not found</param>
        public string GetText(Language language, bool fallbackToEnglish = true)
        {
            foreach (var entry in _translations)
            {
                if (entry.Language == language)
                {
                    return entry.Text;
                }
            }

            // Fallback to English
            if (fallbackToEnglish && language != Language.English)
            {
                foreach (var entry in _translations)
                {
                    if (entry.Language == Language.English)
                    {
                        return entry.Text;
                    }
                }
            }

            // Last resort: return first available
            return _translations.Count > 0 ? _translations[0].Text : "";
        }

        /// <summary>
        /// Sets text for a specific language. Useful for editor tools.
        /// </summary>
        public void SetText(Language language, string text)
        {
            foreach (var entry in _translations)
            {
                if (entry.Language == language)
                {
                    entry.Text = text;
                    return;
                }
            }

            // Add new entry if language not found
            _translations.Add(new LanguageEntry { Language = language, Text = text });
        }

        /// <summary>
        /// Implicit conversion to string for easy usage in code.
        /// Example: string name = myLocalizedString; // Calls GetText()
        /// </summary>
        public static implicit operator string(LocalizedString localizedString)
        {
            return localizedString != null ? localizedString.GetText() : "";
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure we have at least one entry for English
            bool hasEnglish = false;
            foreach (var entry in _translations)
            {
                if (entry.Language == Language.English)
                {
                    hasEnglish = true;
                    break;
                }
            }

            if (!hasEnglish && _translations.Count == 0)
            {
                _translations.Add(new LanguageEntry { Language = Language.English, Text = "" });
            }
        }
#endif
    }
}
