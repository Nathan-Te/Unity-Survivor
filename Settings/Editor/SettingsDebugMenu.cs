using UnityEngine;
using UnityEditor;

namespace SurvivorGame.Settings.Editor
{
    /// <summary>
    /// Unity Editor menu for debugging game settings save system.
    /// Access via Tools > Settings System in Unity menu bar.
    /// </summary>
    public static class SettingsDebugMenu
    {
        [MenuItem("Tools/Settings System/Show Settings File Location")]
        public static void ShowSettingsLocation()
        {
            string path = SettingsSaveSystem.GetSavePath();
            Debug.Log($"Settings file location: {path}");
            EditorUtility.DisplayDialog(
                "Settings File Location",
                $"Settings are saved at:\n\n{path}\n\nCheck the console for the clickable path.",
                "OK"
            );
        }

        [MenuItem("Tools/Settings System/Open Settings Folder")]
        public static void OpenSettingsFolder()
        {
            string folder = Application.persistentDataPath;
            EditorUtility.RevealInFinder(folder);
        }

        [MenuItem("Tools/Settings System/Delete Settings File")]
        public static void DeleteSettings()
        {
            if (EditorUtility.DisplayDialog(
                "Delete Settings File",
                "Are you sure you want to delete the settings file? This cannot be undone.",
                "Delete",
                "Cancel"))
            {
                SettingsSaveSystem.DeleteSettings();
                Debug.Log("[SettingsDebugMenu] Settings file deleted.");
            }
        }

        [MenuItem("Tools/Settings System/Create Test Settings (Default)")]
        public static void CreateTestSettings()
        {
            var settings = GameSettings.CreateDefault();
            SettingsSaveSystem.SaveSettings(settings);

            Debug.Log("[SettingsDebugMenu] Test settings created with default values.");
            EditorUtility.DisplayDialog(
                "Test Settings Created",
                "Default settings have been created and saved.\n\n" +
                "Language: " + settings.languageCode + "\n" +
                "Master Volume: " + settings.masterVolume,
                "OK"
            );
        }

        [MenuItem("Tools/Settings System/View Settings File Content")]
        public static void ViewSettingsContent()
        {
            if (!SettingsSaveSystem.SettingsExists())
            {
                EditorUtility.DisplayDialog(
                    "No Settings File",
                    "Settings file does not exist yet. Create one by running the game or using 'Create Test Settings'.",
                    "OK"
                );
                return;
            }

            var settings = SettingsSaveSystem.LoadSettings();
            string content = JsonUtility.ToJson(settings, true);

            Debug.Log($"[SettingsDebugMenu] Settings file content:\n{content}");

            EditorUtility.DisplayDialog(
                "Settings File Content",
                "Settings content has been printed to the console.\n\n" +
                $"Language: {settings.languageCode}\n" +
                $"Master Volume: {settings.masterVolume}\n" +
                $"Quality: {settings.qualityLevel}",
                "OK"
            );
        }

        [MenuItem("Tools/Settings System/Force Create Default Settings")]
        public static void ForceCreateDefaultSettings()
        {
            var settings = GameSettings.CreateDefault();
            SettingsSaveSystem.SaveSettings(settings);

            Debug.Log("[SettingsDebugMenu] Forced creation of default settings.");
            EditorUtility.DisplayDialog(
                "Default Settings Created",
                "Default settings have been forcefully created.\n\n" +
                "Any existing settings have been overwritten.",
                "OK"
            );
        }

        [MenuItem("Tools/Settings System/Create Custom Test Settings")]
        public static void CreateCustomTestSettings()
        {
            var settings = new GameSettings
            {
                masterVolume = 0.5f,
                musicVolume = 0.3f,
                sfxVolume = 0.7f,
                languageCode = "fr",
                qualityLevel = 1,
                fullscreen = false,
                showDamageNumbers = true,
                screenShake = true,
                screenShakeIntensity = 0.8f
            };

            SettingsSaveSystem.SaveSettings(settings);

            Debug.Log("[SettingsDebugMenu] Custom test settings created (French, reduced volumes, medium quality).");
            EditorUtility.DisplayDialog(
                "Custom Test Settings Created",
                "Custom test settings created:\n\n" +
                "Language: French\n" +
                "Master Volume: 0.5\n" +
                "Quality: Medium",
                "OK"
            );
        }
    }
}
