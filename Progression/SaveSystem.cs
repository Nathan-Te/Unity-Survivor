using System;
using System.IO;
using UnityEngine;

namespace SurvivorGame.Progression
{
    /// <summary>
    /// Handles saving and loading of player progression data to/from JSON file.
    /// Uses Application.persistentDataPath for cross-platform compatibility.
    /// </summary>
    public static class SaveSystem
    {
        private static readonly string SaveFileName = "progression.json";
        private static readonly string SaveFilePath = Path.Combine(Application.persistentDataPath, SaveFileName);

        public static event Action<PlayerProgressionData> OnDataLoaded;
        public static event Action<PlayerProgressionData> OnDataSaved;

        /// <summary>
        /// Saves progression data to disk
        /// </summary>
        public static void SaveProgression(PlayerProgressionData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SaveFilePath, json);
                Debug.Log($"[SaveSystem] Progression saved to: {SaveFilePath}");
                OnDataSaved?.Invoke(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to save progression: {e.Message}");
            }
        }

        /// <summary>
        /// Loads progression data from disk. Returns default data if file doesn't exist.
        /// </summary>
        public static PlayerProgressionData LoadProgression()
        {
            PlayerProgressionData data;

            if (File.Exists(SaveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SaveFilePath);
                    data = JsonUtility.FromJson<PlayerProgressionData>(json);
                    Debug.Log($"[SaveSystem] Progression loaded from: {SaveFilePath}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveSystem] Failed to load progression: {e.Message}. Using default data.");
                    data = PlayerProgressionData.CreateDefault();
                }
            }
            else
            {
                Debug.Log("[SaveSystem] No save file found. Creating default progression.");
                data = PlayerProgressionData.CreateDefault();
            }

            OnDataLoaded?.Invoke(data);
            return data;
        }

        /// <summary>
        /// Deletes the save file (for debugging or player-initiated reset)
        /// </summary>
        public static void DeleteSave()
        {
            if (File.Exists(SaveFilePath))
            {
                try
                {
                    File.Delete(SaveFilePath);
                    Debug.Log("[SaveSystem] Save file deleted.");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveSystem] Failed to delete save file: {e.Message}");
                }
            }
            else
            {
                Debug.Log("[SaveSystem] No save file to delete.");
            }
        }

        /// <summary>
        /// Checks if a save file exists
        /// </summary>
        public static bool SaveExists()
        {
            return File.Exists(SaveFilePath);
        }

        /// <summary>
        /// Gets the full path to the save file
        /// </summary>
        public static string GetSavePath()
        {
            return SaveFilePath;
        }
    }
}
