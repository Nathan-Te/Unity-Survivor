using UnityEngine;
using System;
using System.Collections.Generic;

namespace SurvivorGame.Localization
{
    /// <summary>
    /// Singleton manager for game localization.
    /// Handles language switching and string retrieval.
    /// DontDestroyOnLoad - Persists across scene reloads.
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private Language _defaultLanguage = Language.English;

        [Header("Localization Tables")]
        [SerializeField] private List<LocalizationTable> _tables = new List<LocalizationTable>();

        private Language _currentLanguage;
        private Dictionary<string, LocalizationTable> _tableLookup;

        /// <summary>
        /// Event fired when language changes. UI components should subscribe to update their text.
        /// </summary>
        public static event Action OnLanguageChanged;

        public Language CurrentLanguage => _currentLanguage;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _currentLanguage = _defaultLanguage;
            BuildTableLookup();
        }

        private void BuildTableLookup()
        {
            _tableLookup = new Dictionary<string, LocalizationTable>();

            foreach (var table in _tables)
            {
                if (table != null && !string.IsNullOrEmpty(table.TableName))
                {
                    _tableLookup[table.TableName] = table;
                }
            }
        }

        /// <summary>
        /// Changes the current language and notifies all listeners.
        /// </summary>
        public void SetLanguage(Language language)
        {
            if (_currentLanguage == language) return;

            _currentLanguage = language;
            OnLanguageChanged?.Invoke();
        }

        /// <summary>
        /// Gets a localized string by key from a specific table.
        /// </summary>
        /// <param name="tableName">Name of the localization table</param>
        /// <param name="key">String key</param>
        /// <param name="fallbackValue">Value to return if key not found</param>
        /// <returns>Localized string or fallback value</returns>
        public string GetString(string tableName, string key, string fallbackValue = "")
        {
            if (_tableLookup == null)
            {
                BuildTableLookup();
            }

            if (_tableLookup.TryGetValue(tableName, out LocalizationTable table))
            {
                return table.GetString(key, _currentLanguage, fallbackValue);
            }

            Debug.LogWarning($"LocalizationTable '{tableName}' not found. Returning fallback.");
            return fallbackValue;
        }

        /// <summary>
        /// Gets a formatted localized string with parameter substitution.
        /// Example: GetFormattedString("UI", "LEVEL_TEXT", "5") for "LVL {0}" -> "LVL 5"
        /// </summary>
        public string GetFormattedString(string tableName, string key, params object[] args)
        {
            string template = GetString(tableName, key);

            if (string.IsNullOrEmpty(template))
            {
                return "";
            }

            try
            {
                return string.Format(template, args);
            }
            catch (FormatException)
            {
                Debug.LogError($"Format error for key '{key}' with args: {string.Join(", ", args)}");
                return template;
            }
        }

        /// <summary>
        /// Adds a table at runtime (useful for modding or dynamic content).
        /// </summary>
        public void RegisterTable(LocalizationTable table)
        {
            if (table == null || string.IsNullOrEmpty(table.TableName)) return;

            if (!_tables.Contains(table))
            {
                _tables.Add(table);
            }

            _tableLookup[table.TableName] = table;
        }
    }
}
