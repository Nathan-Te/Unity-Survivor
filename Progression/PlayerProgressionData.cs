using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivorGame.Progression
{
    /// <summary>
    /// Serializable data structure for player progression across runs.
    /// Contains persistent upgrades, unlocks, and meta-progression currency.
    /// </summary>
    [Serializable]
    public class PlayerProgressionData
    {
        [Header("Meta Currency")]
        public int gold = 0;

        [Header("Spell Slots")]
        public int maxSpellSlots = 4; // Default starting spell slots

        [Header("Rune Unlocks")]
        public List<RuneUnlockData> runeUnlocks = new List<RuneUnlockData>();

        [Header("Level Unlocks")]
        public List<string> unlockedLevelIds = new List<string>(); // Scene names or level IDs

        [Header("Statistics")]
        public int totalRunsCompleted = 0;
        public int totalEnemiesKilled = 0;
        public float bestRunTime = 0f;
        public int highestLevel = 0;
        public int highestScore = 0;

        /// <summary>
        /// Creates default starting progression data.
        /// CUSTOMIZE HERE: Modify these values to set the initial player progression.
        /// </summary>
        public static PlayerProgressionData CreateDefault()
        {
            var data = new PlayerProgressionData
            {
                // Starting gold (meta-currency for upgrades)
                gold = 0,

                // Starting spell slots (default: 4)
                maxSpellSlots = 4,

                // Starting rune unlocks (empty = all locked, requires upgrades to unlock)
                runeUnlocks = new List<RuneUnlockData>(),

                // Starting unlocked levels (add more to unlock levels by default)
                unlockedLevelIds = new List<string>
                {
                    "Level_Tutorial"  // Tutorial is unlocked by default
                    // "Level_Forest",  // Uncomment to unlock Forest by default
                    // "Level_Dungeon", // Uncomment to unlock Dungeon by default
                },

                // Statistics (always start at 0)
                totalRunsCompleted = 0,
                totalEnemiesKilled = 0,
                bestRunTime = 0f,
                highestLevel = 0,
                highestScore = 0
            };

            // Optional: Unlock some runes by default (example commented out)
            // data.UnlockRune("Bolt");
            // data.UpgradeRuneMaxLevel("Bolt", 5);
            // data.UnlockRune("Fire");
            // data.UpgradeRuneMaxLevel("Fire", 5);

            return data;
        }

        /// <summary>
        /// Checks if a rune is unlocked (playable)
        /// </summary>
        public bool IsRuneUnlocked(string runeId)
        {
            var unlock = runeUnlocks.Find(r => r.runeId == runeId);
            return unlock != null && unlock.isUnlocked;
        }

        /// <summary>
        /// Gets the maximum level for a specific rune type
        /// </summary>
        public int GetRuneMaxLevel(string runeId)
        {
            var unlock = runeUnlocks.Find(r => r.runeId == runeId);
            return unlock?.maxLevel ?? 1; // Default to level 1 if not found
        }

        /// <summary>
        /// Unlocks a rune for use in-game
        /// </summary>
        public void UnlockRune(string runeId)
        {
            var unlock = runeUnlocks.Find(r => r.runeId == runeId);
            if (unlock != null)
            {
                unlock.isUnlocked = true;
            }
            else
            {
                runeUnlocks.Add(new RuneUnlockData
                {
                    runeId = runeId,
                    isUnlocked = true,
                    maxLevel = 1
                });
            }
        }

        /// <summary>
        /// Upgrades the maximum level for a rune
        /// </summary>
        public void UpgradeRuneMaxLevel(string runeId, int newMaxLevel)
        {
            var unlock = runeUnlocks.Find(r => r.runeId == runeId);
            if (unlock != null)
            {
                unlock.maxLevel = Mathf.Max(unlock.maxLevel, newMaxLevel);
            }
            else
            {
                runeUnlocks.Add(new RuneUnlockData
                {
                    runeId = runeId,
                    isUnlocked = false,
                    maxLevel = newMaxLevel
                });
            }
        }

        /// <summary>
        /// Unlocks a level for selection
        /// </summary>
        public void UnlockLevel(string levelId)
        {
            if (!unlockedLevelIds.Contains(levelId))
            {
                unlockedLevelIds.Add(levelId);
            }
        }

        /// <summary>
        /// Checks if a level is unlocked
        /// </summary>
        public bool IsLevelUnlocked(string levelId)
        {
            return unlockedLevelIds.Contains(levelId);
        }

        /// <summary>
        /// Adds gold to the player's total
        /// </summary>
        public void AddGold(int amount)
        {
            gold += amount;
        }

        /// <summary>
        /// Attempts to spend gold. Returns true if successful.
        /// </summary>
        public bool SpendGold(int amount)
        {
            if (gold >= amount)
            {
                gold -= amount;
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Data for a single rune's unlock status and maximum level
    /// </summary>
    [Serializable]
    public class RuneUnlockData
    {
        public string runeId;      // Asset name or unique identifier
        public bool isUnlocked;    // Can be used in-game
        public int maxLevel;       // Maximum level this rune can reach (unlocked via meta upgrades)
    }
}
