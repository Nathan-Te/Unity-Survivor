using UnityEngine;
using System;
using System.Collections.Generic;

namespace SurvivorGame.Localization
{
    /// <summary>
    /// ScriptableObject that holds a table of localized strings organized by keys.
    /// Used for UI strings, error messages, and other static text.
    /// Create via: Assets > Create > Localization > Localization Table
    /// </summary>
    [CreateAssetMenu(fileName = "New Localization Table", menuName = "Localization/Localization Table")]
    public class LocalizationTable : ScriptableObject
    {
        [Header("Table Info")]
        [Tooltip("Unique identifier for this table (e.g., 'UI', 'Combat', 'Errors')")]
        public string TableName = "UI";

        [Header("Entries")]
        [SerializeField] private List<LocalizationEntry> _entries = new List<LocalizationEntry>();

        private Dictionary<Language, Dictionary<string, string>> _cache;

        [Serializable]
        public class LocalizationEntry
        {
            [Tooltip("Unique key for this string (e.g., 'LEVEL_UP_TITLE')")]
            public string Key;

            [Header("Translations")]
            [TextArea(1, 3)]
            public string English;

            [TextArea(1, 3)]
            public string French;

            // Add more languages here as needed
        }

        /// <summary>
        /// Gets a localized string by key for a specific language.
        /// </summary>
        public string GetString(string key, Language language, string fallbackValue = "")
        {
            if (_cache == null)
            {
                BuildCache();
            }

            if (_cache.TryGetValue(language, out var languageDict))
            {
                if (languageDict.TryGetValue(key, out string value))
                {
                    return value;
                }
            }

            // Fallback to English if not found
            if (language != Language.English && _cache.TryGetValue(Language.English, out var englishDict))
            {
                if (englishDict.TryGetValue(key, out string englishValue))
                {
                    Debug.LogWarning($"Key '{key}' not found in {language}, using English fallback.");
                    return englishValue;
                }
            }

            Debug.LogWarning($"Key '{key}' not found in table '{TableName}'. Returning fallback.");
            return fallbackValue;
        }

        /// <summary>
        /// Builds runtime cache for fast lookups.
        /// </summary>
        private void BuildCache()
        {
            _cache = new Dictionary<Language, Dictionary<string, string>>();

            // Initialize dictionaries for each language
            foreach (Language lang in Enum.GetValues(typeof(Language)))
            {
                _cache[lang] = new Dictionary<string, string>();
            }

            // Populate cache
            foreach (var entry in _entries)
            {
                if (string.IsNullOrEmpty(entry.Key))
                {
                    Debug.LogWarning($"Empty key found in table '{TableName}'. Skipping entry.");
                    continue;
                }

                _cache[Language.English][entry.Key] = entry.English ?? "";
                _cache[Language.French][entry.Key] = entry.French ?? "";

                // Add more languages here as they're added to LocalizationEntry
            }
        }

        /// <summary>
        /// Clears the cache. Call this if entries are modified at runtime.
        /// </summary>
        public void ClearCache()
        {
            _cache = null;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor helper to add or update an entry.
        /// </summary>
        public void SetEntry(string key, string english, string french = "")
        {
            foreach (var entry in _entries)
            {
                if (entry.Key == key)
                {
                    entry.English = english;
                    if (!string.IsNullOrEmpty(french))
                    {
                        entry.French = french;
                    }
                    UnityEditor.EditorUtility.SetDirty(this);
                    return;
                }
            }

            // Add new entry
            _entries.Add(new LocalizationEntry
            {
                Key = key,
                English = english,
                French = french
            });
            UnityEditor.EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Returns all keys in this table for validation.
        /// </summary>
        public List<string> GetAllKeys()
        {
            var keys = new List<string>();
            foreach (var entry in _entries)
            {
                if (!string.IsNullOrEmpty(entry.Key))
                {
                    keys.Add(entry.Key);
                }
            }
            return keys;
        }

        private void OnValidate()
        {
            // Clear cache when modified in editor
            ClearCache();
        }
#endif
    }
}
