using UnityEngine;
using System;

namespace SurvivorGame.Progression
{
    /// <summary>
    /// DontDestroyOnLoad singleton that manages player progression across sessions.
    /// Handles loading/saving progression data and provides access to persistent upgrades.
    /// </summary>
    public class ProgressionManager : Singleton<ProgressionManager>
    {
        [Header("Settings")]
        [SerializeField] private bool autoSaveOnChange = true;
        [SerializeField] private bool verboseLogging = true;

        private PlayerProgressionData _currentProgression;

        public PlayerProgressionData CurrentProgression => _currentProgression;

        // Events for UI to listen to
        public event Action<PlayerProgressionData> OnProgressionLoaded;
        public event Action<PlayerProgressionData> OnProgressionChanged;

        protected override void Awake()
        {
            base.Awake();

            // CRITICAL: Always initialize, never check "if (Instance == this)"
            // This ensures proper initialization even after scene restarts
            // (See CLAUDE.md rule: NEVER use `if (Instance == this)` in Singleton Awake())

            // Ensure this GameObject is at root level for DontDestroyOnLoad
            if (transform.parent != null)
            {
                Debug.LogWarning("[ProgressionManager] ProgressionManager must be on a root GameObject. Moving to root.");
                transform.SetParent(null);
            }

            DontDestroyOnLoad(gameObject);

            // Load progression on startup (ALWAYS, even after restart)
            LoadProgression();
        }

        /// <summary>
        /// Loads progression from disk
        /// </summary>
        public void LoadProgression()
        {
            _currentProgression = SaveSystem.LoadProgression();

            if (verboseLogging)
                Debug.Log($"[ProgressionManager] Loaded progression: Gold={_currentProgression.gold}, Spell Slots={_currentProgression.maxSpellSlots}");

            OnProgressionLoaded?.Invoke(_currentProgression);
        }

        /// <summary>
        /// Saves progression to disk
        /// </summary>
        public void SaveProgression()
        {
            if (_currentProgression == null)
            {
                Debug.LogWarning("[ProgressionManager] Cannot save null progression data.");
                return;
            }

            SaveSystem.SaveProgression(_currentProgression);

            if (verboseLogging)
                Debug.Log($"[ProgressionManager] Saved progression: Gold={_currentProgression.gold}");
        }

        /// <summary>
        /// Awards gold to the player and optionally saves
        /// </summary>
        public void AwardGold(int amount)
        {
            if (_currentProgression == null) return;

            _currentProgression.AddGold(amount);

            if (verboseLogging)
                Debug.Log($"[ProgressionManager] Awarded {amount} gold. Total: {_currentProgression.gold}");

            OnProgressionChanged?.Invoke(_currentProgression);

            if (autoSaveOnChange)
                SaveProgression();
        }

        /// <summary>
        /// Attempts to spend gold. Returns true if successful.
        /// </summary>
        public bool SpendGold(int amount)
        {
            if (_currentProgression == null) return false;

            bool success = _currentProgression.SpendGold(amount);

            if (success)
            {
                if (verboseLogging)
                    Debug.Log($"[ProgressionManager] Spent {amount} gold. Remaining: {_currentProgression.gold}");

                OnProgressionChanged?.Invoke(_currentProgression);

                if (autoSaveOnChange)
                    SaveProgression();
            }

            return success;
        }

        /// <summary>
        /// Unlocks a rune for use in-game
        /// </summary>
        public void UnlockRune(string runeId)
        {
            if (_currentProgression == null) return;

            _currentProgression.UnlockRune(runeId);

            if (verboseLogging)
                Debug.Log($"[ProgressionManager] Unlocked rune: {runeId}");

            OnProgressionChanged?.Invoke(_currentProgression);

            if (autoSaveOnChange)
                SaveProgression();
        }

        /// <summary>
        /// Upgrades the maximum level for a rune
        /// </summary>
        public void UpgradeRuneMaxLevel(string runeId, int newMaxLevel)
        {
            if (_currentProgression == null) return;

            _currentProgression.UpgradeRuneMaxLevel(runeId, newMaxLevel);

            if (verboseLogging)
                Debug.Log($"[ProgressionManager] Upgraded rune max level: {runeId} -> {newMaxLevel}");

            OnProgressionChanged?.Invoke(_currentProgression);

            if (autoSaveOnChange)
                SaveProgression();
        }

        /// <summary>
        /// Unlocks a level for selection
        /// </summary>
        public void UnlockLevel(string levelId)
        {
            if (_currentProgression == null) return;

            _currentProgression.UnlockLevel(levelId);

            if (verboseLogging)
                Debug.Log($"[ProgressionManager] Unlocked level: {levelId}");

            OnProgressionChanged?.Invoke(_currentProgression);

            if (autoSaveOnChange)
                SaveProgression();
        }

        /// <summary>
        /// Increases max spell slots
        /// </summary>
        public void UpgradeMaxSpellSlots(int amount)
        {
            if (_currentProgression == null) return;

            _currentProgression.maxSpellSlots += amount;

            if (verboseLogging)
                Debug.Log($"[ProgressionManager] Upgraded max spell slots: {_currentProgression.maxSpellSlots}");

            OnProgressionChanged?.Invoke(_currentProgression);

            if (autoSaveOnChange)
                SaveProgression();
        }

        /// <summary>
        /// Records statistics from a completed run
        /// </summary>
        public void RecordRunStats(int enemiesKilled, float runTime, int playerLevel, int score)
        {
            if (_currentProgression == null) return;

            _currentProgression.totalRunsCompleted++;
            _currentProgression.totalEnemiesKilled += enemiesKilled;

            if (runTime > _currentProgression.bestRunTime)
                _currentProgression.bestRunTime = runTime;

            if (playerLevel > _currentProgression.highestLevel)
                _currentProgression.highestLevel = playerLevel;

            if (score > _currentProgression.highestScore)
                _currentProgression.highestScore = score;

            if (verboseLogging)
                Debug.Log($"[ProgressionManager] Recorded run stats: Enemies={enemiesKilled}, Time={runTime:F1}s, Level={playerLevel}");

            OnProgressionChanged?.Invoke(_currentProgression);

            if (autoSaveOnChange)
                SaveProgression();
        }

        /// <summary>
        /// Resets all progression data (for debugging)
        /// </summary>
        public void ResetProgression()
        {
            _currentProgression = PlayerProgressionData.CreateDefault();
            SaveProgression();

            if (verboseLogging)
                Debug.Log("[ProgressionManager] Progression reset to default.");

            OnProgressionChanged?.Invoke(_currentProgression);
        }
    }
}
