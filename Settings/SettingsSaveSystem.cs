using System;
using System.IO;
using UnityEngine;

namespace SurvivorGame.Settings
{
    /// <summary>
    /// Handles saving and loading of game settings to/from settings.json
    /// Separate from player progression save system.
    /// </summary>
    public static class SettingsSaveSystem
    {
        private const string SETTINGS_FILE_NAME = "settings.json";

        private static string SaveFilePath => Path.Combine(Application.persistentDataPath, SETTINGS_FILE_NAME);

        /// <summary>
        /// Event fired when settings are loaded
        /// </summary>
        public static event Action<GameSettings> OnSettingsLoaded;

        /// <summary>
        /// Event fired when settings are saved
        /// </summary>
        public static event Action<GameSettings> OnSettingsSaved;

        /// <summary>
        /// Loads settings from disk. Creates default if file doesn't exist.
        /// </summary>
        public static GameSettings LoadSettings()
        {
            GameSettings settings;

            if (File.Exists(SaveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SaveFilePath);
                    settings = JsonUtility.FromJson<GameSettings>(json);

                    if (settings == null)
                    {
                        Debug.LogWarning("[SettingsSaveSystem] Failed to deserialize settings. Using default.");
                        settings = GameSettings.CreateDefault();
                    }
                    else
                    {
                        Debug.Log($"[SettingsSaveSystem] Settings loaded from: {SaveFilePath}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SettingsSaveSystem] Error loading settings: {e.Message}");
                    settings = GameSettings.CreateDefault();
                }
            }
            else
            {
                Debug.Log("[SettingsSaveSystem] No settings file found. Creating default settings.");
                settings = GameSettings.CreateDefault();
                SaveSettings(settings); // Save default settings immediately
            }

            OnSettingsLoaded?.Invoke(settings);
            return settings;
        }

        /// <summary>
        /// Saves settings to disk
        /// </summary>
        public static void SaveSettings(GameSettings settings)
        {
            if (settings == null)
            {
                Debug.LogError("[SettingsSaveSystem] Cannot save null settings.");
                return;
            }

            try
            {
                string json = JsonUtility.ToJson(settings, true);
                File.WriteAllText(SaveFilePath, json);

                Debug.Log($"[SettingsSaveSystem] Settings saved to: {SaveFilePath}");
                OnSettingsSaved?.Invoke(settings);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SettingsSaveSystem] Error saving settings: {e.Message}");
            }
        }

        /// <summary>
        /// Checks if settings file exists
        /// </summary>
        public static bool SettingsExists()
        {
            return File.Exists(SaveFilePath);
        }

        /// <summary>
        /// Deletes the settings file (for debugging)
        /// </summary>
        public static void DeleteSettings()
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
                Debug.Log($"[SettingsSaveSystem] Settings file deleted: {SaveFilePath}");
            }
            else
            {
                Debug.LogWarning("[SettingsSaveSystem] No settings file to delete.");
            }
        }

        /// <summary>
        /// Gets the full path to the settings file
        /// </summary>
        public static string GetSavePath()
        {
            return SaveFilePath;
        }
    }
}
