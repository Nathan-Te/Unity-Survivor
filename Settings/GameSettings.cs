using System;
using SurvivorGame.Localization;
using UnityEngine;

namespace SurvivorGame.Settings
{
    /// <summary>
    /// Serializable data structure for game settings.
    /// Saved separately from player progression in settings.json
    /// </summary>
    [Serializable]
    public class GameSettings
    {
        [Header("Audio Settings")]
        public float masterVolume = 1.0f;
        public float musicVolume = 0.7f;
        public float sfxVolume = 0.8f;
        public bool muteAll = false;

        [Header("Localization")]
        public string languageCode = "en"; // "en", "fr", etc.

        [Header("Graphics Settings")]
        public int qualityLevel = 2; // 0 = Low, 1 = Medium, 2 = High
        public bool fullscreen = true;
        public int targetFrameRate = 60;
        public bool vsync = true;

        [Header("Gameplay Settings")]
        public bool showDamageNumbers = true;
        public bool screenShake = true;
        public float screenShakeIntensity = 1.0f;

        [Header("Accessibility")]
        public bool colorBlindMode = false;
        public float uiScale = 1.0f;

        /// <summary>
        /// Creates default settings
        /// </summary>
        public static GameSettings CreateDefault()
        {
            return new GameSettings
            {
                // Audio
                masterVolume = 1.0f,
                musicVolume = 0.7f,
                sfxVolume = 0.8f,
                muteAll = false,

                // Localization (system language or default to English)
                languageCode = GetSystemLanguageCode(),

                // Graphics
                qualityLevel = QualitySettings.GetQualityLevel(),
                fullscreen = Screen.fullScreen,
                targetFrameRate = 60,
                vsync = QualitySettings.vSyncCount > 0,

                // Gameplay
                showDamageNumbers = true,
                screenShake = true,
                screenShakeIntensity = 1.0f,

                // Accessibility
                colorBlindMode = false,
                uiScale = 1.0f
            };
        }

        /// <summary>
        /// Gets the system language code (en, fr, etc.)
        /// </summary>
        private static string GetSystemLanguageCode()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.French:
                    return "fr";
                case SystemLanguage.English:
                default:
                    return "en";
            }
        }

        /// <summary>
        /// Converts language code to Language enum
        /// </summary>
        public Language GetLanguageEnum()
        {
            switch (languageCode.ToLower())
            {
                case "fr":
                    return Language.French;
                case "en":
                default:
                    return Language.English;
            }
        }

        /// <summary>
        /// Sets language code from Language enum
        /// </summary>
        public void SetLanguage(Language language)
        {
            switch (language)
            {
                case Language.French:
                    languageCode = "fr";
                    break;
                case Language.English:
                default:
                    languageCode = "en";
                    break;
            }
        }
    }
}
