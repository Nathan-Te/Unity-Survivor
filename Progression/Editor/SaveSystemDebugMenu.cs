using UnityEngine;
using UnityEditor;
using System.IO;

namespace SurvivorGame.Progression.Editor
{
    /// <summary>
    /// Unity Editor menu for debugging the save system.
    /// Provides quick access to save file location and management.
    /// </summary>
    public static class SaveSystemDebugMenu
    {
        private const string MenuPath = "Tools/Save System/";

        [MenuItem(MenuPath + "Show Save File Location")]
        public static void ShowSaveLocation()
        {
            string path = SaveSystem.GetSavePath();
            string folder = Path.GetDirectoryName(path);

            Debug.Log($"[SaveSystem] Save file location:\n{path}");
            Debug.Log($"[SaveSystem] Folder: {folder}");
            Debug.Log($"[SaveSystem] Exists: {SaveSystem.SaveExists()}");

            // Open folder in explorer
            if (Directory.Exists(folder))
            {
                EditorUtility.RevealInFinder(path);
            }
            else
            {
                Debug.LogWarning($"[SaveSystem] Folder does not exist yet: {folder}\nIt will be created when the first save is made.");
            }
        }

        [MenuItem(MenuPath + "Open Save Folder")]
        public static void OpenSaveFolder()
        {
            string path = SaveSystem.GetSavePath();
            string folder = Path.GetDirectoryName(path);

            if (Directory.Exists(folder))
            {
                EditorUtility.RevealInFinder(folder);
            }
            else
            {
                Debug.LogWarning($"[SaveSystem] Folder does not exist yet: {folder}\nIt will be created when the first save is made.");

                // Create the folder
                Directory.CreateDirectory(folder);
                Debug.Log($"[SaveSystem] Created folder: {folder}");
                EditorUtility.RevealInFinder(folder);
            }
        }

        [MenuItem(MenuPath + "Delete Save File")]
        public static void DeleteSaveFile()
        {
            if (SaveSystem.SaveExists())
            {
                bool confirm = EditorUtility.DisplayDialog(
                    "Delete Save File",
                    "Are you sure you want to delete the save file?\n\nThis action cannot be undone!",
                    "Delete",
                    "Cancel"
                );

                if (confirm)
                {
                    SaveSystem.DeleteSave();
                    Debug.Log("[SaveSystem] Save file deleted successfully.");
                }
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "No Save File",
                    "No save file exists to delete.",
                    "OK"
                );
            }
        }

        [MenuItem(MenuPath + "Create Test Save (1000 Gold)")]
        public static void CreateTestSave()
        {
            var testData = PlayerProgressionData.CreateDefault();
            testData.gold = 1000;
            testData.maxSpellSlots = 6;
            testData.UnlockLevel("Level_Tutorial");
            testData.UnlockLevel("Level_Forest");
            testData.UnlockLevel("Level_Dungeon");

            SaveSystem.SaveProgression(testData);
            Debug.Log("[SaveSystem] Test save created with 1000 gold and all levels unlocked!");
        }

        [MenuItem(MenuPath + "View Save File Content")]
        public static void ViewSaveContent()
        {
            if (SaveSystem.SaveExists())
            {
                string path = SaveSystem.GetSavePath();
                string json = File.ReadAllText(path);

                Debug.Log($"[SaveSystem] Save file content:\n{json}");

                // Also show in a popup
                EditorUtility.DisplayDialog(
                    "Save File Content",
                    json,
                    "OK"
                );
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "No Save File",
                    "No save file exists yet.",
                    "OK"
                );
            }
        }

        [MenuItem(MenuPath + "Force Create Default Save")]
        public static void ForceCreateDefaultSave()
        {
            var defaultData = PlayerProgressionData.CreateDefault();
            SaveSystem.SaveProgression(defaultData);
            Debug.Log("[SaveSystem] Default save file created!");
        }
    }
}
