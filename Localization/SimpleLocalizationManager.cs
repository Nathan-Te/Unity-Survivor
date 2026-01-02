using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

namespace SurvivorGame.Localization
{
    /// <summary>
    /// Simplified localization manager that loads from JSON files.
    /// Place your language files in Assets/Resources/Localization/
    /// Files should be named: en.json, fr.json, etc.
    /// </summary>
    public class SimpleLocalizationManager : MonoBehaviour
    {
        public static SimpleLocalizationManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private Language _defaultLanguage = Language.English;
        [SerializeField] private bool _loadFromResources = true;

        private Language _currentLanguage;
        private Dictionary<string, string> _currentStrings = new Dictionary<string, string>();

        /// <summary>
        /// Event fired when language changes.
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
            LoadLanguage(_currentLanguage);
        }

        /// <summary>
        /// Changes the current language and reloads strings.
        /// </summary>
        public void SetLanguage(Language language)
        {
            if (_currentLanguage == language) return;

            _currentLanguage = language;
            LoadLanguage(_currentLanguage);
            OnLanguageChanged?.Invoke();
        }

        /// <summary>
        /// Forces language reload even if it's already the current language.
        /// Useful for applying settings on startup.
        /// </summary>
        public void ForceSetLanguage(Language language)
        {
            _currentLanguage = language;
            LoadLanguage(_currentLanguage);
            OnLanguageChanged?.Invoke();
        }

        /// <summary>
        /// Gets a localized string by key.
        /// </summary>
        public string GetString(string key, string fallback = "")
        {
            if (_currentStrings.TryGetValue(key, out string value))
            {
                return value;
            }

            Debug.LogWarning($"Localization key '{key}' not found. Using fallback.");
            return !string.IsNullOrEmpty(fallback) ? fallback : key;
        }

        /// <summary>
        /// Gets a formatted localized string with parameter substitution.
        /// Example: GetFormattedString("LEVEL_TEXT", 5) for "LVL {0}" -> "LVL 5"
        /// </summary>
        public string GetFormattedString(string key, params object[] args)
        {
            string template = GetString(key);

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
        /// Loads language strings from JSON file.
        /// </summary>
        private void LoadLanguage(Language language)
        {
            _currentStrings.Clear();

            string fileName = GetLanguageFileName(language);

            if (_loadFromResources)
            {
                LoadFromResources(fileName);
            }
            else
            {
                LoadFromStreamingAssets(fileName);
            }

            Debug.Log($"Loaded {_currentStrings.Count} localization strings for {language}");
        }

        private void LoadFromResources(string fileName)
        {
            TextAsset jsonFile = Resources.Load<TextAsset>($"Localization/{fileName}");

            if (jsonFile == null)
            {
                Debug.LogError($"Localization file not found: Resources/Localization/{fileName}.json");
                return;
            }

            ParseJsonStrings(jsonFile.text);
        }

        private void LoadFromStreamingAssets(string fileName)
        {
            string path = Path.Combine(Application.streamingAssetsPath, "Localization", $"{fileName}.json");

            if (!File.Exists(path))
            {
                Debug.LogError($"Localization file not found: {path}");
                return;
            }

            string json = File.ReadAllText(path);
            ParseJsonStrings(json);
        }

        private void ParseJsonStrings(string json)
        {
            try
            {
                LocalizationData data = JsonUtility.FromJson<LocalizationData>(json);

                if (data != null && data.entries != null)
                {
                    foreach (var entry in data.entries)
                    {
                        if (!string.IsNullOrEmpty(entry.key))
                        {
                            _currentStrings[entry.key] = entry.value ?? "";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse localization JSON: {e.Message}");
            }
        }

        private string GetLanguageFileName(Language language)
        {
            return language switch
            {
                Language.English => "en",
                Language.French => "fr",
                _ => "en"
            };
        }

        [System.Serializable]
        private class LocalizationData
        {
            public LocalizationEntry[] entries;
        }

        [System.Serializable]
        private class LocalizationEntry
        {
            public string key;
            public string value;
        }
    }
}
