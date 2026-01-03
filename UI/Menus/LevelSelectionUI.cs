using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SurvivorGame.Progression;
using SurvivorGame.Localization;
using SurvivorGame.Core;
using TMPro;
using System.Collections.Generic;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Manages level selection screen.
    /// Displays available levels and handles loading selected level.
    /// </summary>
    public class LevelSelectionUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button backButton;
        [SerializeField] private Transform levelButtonContainer;
        [SerializeField] private LevelSelectButton levelButtonPrefab;
        [SerializeField] private CanvasGroup levelSelectionCanvasGroup;

        [Header("Level Definitions")]
        [SerializeField] private List<LevelDefinition> availableLevels = new List<LevelDefinition>();

        [Header("Settings")]
        [SerializeField] private bool verboseLogging = false;

        private List<LevelSelectButton> _spawnedButtons = new List<LevelSelectButton>();

        private void Start()
        {
            if (backButton) backButton.onClick.AddListener(OnBackPressed);

            // Subscribe to progression to refresh when unlocks change
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.OnProgressionChanged += OnProgressionChanged;
            }

            PopulateLevelButtons();

            if (verboseLogging)
                Debug.Log("[LevelSelectionUI] Initialized");
        }

        private void OnDestroy()
        {
            if (backButton) backButton.onClick.RemoveListener(OnBackPressed);

            // Use FindFirstObjectByType to avoid Singleton getter error during scene unload
            var progressionManager = FindFirstObjectByType<ProgressionManager>();
            if (progressionManager != null)
            {
                progressionManager.OnProgressionChanged -= OnProgressionChanged;
            }
        }

        private void OnProgressionChanged(PlayerProgressionData data)
        {
            RefreshLevelButtons();
        }

        /// <summary>
        /// Creates level selection buttons from available levels
        /// </summary>
        private void PopulateLevelButtons()
        {
            if (levelButtonPrefab == null || levelButtonContainer == null)
            {
                Debug.LogWarning("[LevelSelectionUI] Missing prefab or container reference");
                return;
            }

            // Clear existing buttons
            foreach (var button in _spawnedButtons)
            {
                if (button != null) Destroy(button.gameObject);
            }
            _spawnedButtons.Clear();

            // Create button for each level
            foreach (var levelDef in availableLevels)
            {
                var buttonObj = Instantiate(levelButtonPrefab, levelButtonContainer);
                buttonObj.Initialize(levelDef, OnLevelSelected);
                _spawnedButtons.Add(buttonObj);
            }

            if (verboseLogging)
                Debug.Log($"[LevelSelectionUI] Spawned {_spawnedButtons.Count} level buttons");
        }

        /// <summary>
        /// Refreshes button states without recreating them
        /// </summary>
        private void RefreshLevelButtons()
        {
            foreach (var button in _spawnedButtons)
            {
                if (button != null) button.Refresh();
            }
        }

        /// <summary>
        /// Called when a level is selected
        /// </summary>
        private void OnLevelSelected(LevelDefinition level)
        {
            if (level == null) return;

            // Check if level is unlocked
            var progression = ProgressionManager.Instance?.CurrentProgression;
            if (progression != null && !progression.IsLevelUnlocked(level.levelId))
            {
                Debug.LogWarning($"[LevelSelectionUI] Attempted to select locked level: {level.levelId}");
                return;
            }

            if (verboseLogging)
                Debug.Log($"[LevelSelectionUI] Loading level: {level.sceneName}");

            // CRITICAL: Hide level selection UI BEFORE loading screen appears
            // This prevents the panel from appearing over the loading screen
            HideLevelSelectionPanel();

            // Load the level scene using SceneLoader (shows loading screen)
            // Note: GameStateController will be initialized in the game scene
            // and will automatically set state to Playing in its Awake/Start
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(level.sceneName);
            }
            else
            {
                Debug.LogWarning("[LevelSelectionUI] SceneLoader not found, using fallback SceneManager.LoadScene");
                SceneManager.LoadScene(level.sceneName);
            }
        }

        /// <summary>
        /// Hides the level selection panel (prevents overlap with loading screen)
        /// </summary>
        private void HideLevelSelectionPanel()
        {
            if (levelSelectionCanvasGroup != null)
            {
                levelSelectionCanvasGroup.alpha = 0f;
                levelSelectionCanvasGroup.interactable = false;
                levelSelectionCanvasGroup.blocksRaycasts = false;

                if (verboseLogging)
                    Debug.Log("[LevelSelectionUI] Level selection panel hidden");
            }
            else
            {
                Debug.LogWarning("[LevelSelectionUI] levelSelectionCanvasGroup is not assigned! Panel may appear over loading screen.");
            }
        }

        private void OnBackPressed()
        {
            var mainMenu = FindFirstObjectByType<MainMenuUI>();
            if (mainMenu != null)
            {
                mainMenu.ReturnToMainMenu();
            }
        }
    }

    /// <summary>
    /// Data definition for a playable level
    /// </summary>
    [System.Serializable]
    public class LevelDefinition
    {
        [Tooltip("Unique identifier for this level (used in progression save data)")]
        public string levelId;

        [Tooltip("Display name localization key")]
        public string nameKey;

        [Tooltip("Scene name to load when selected")]
        public string sceneName;

        [Tooltip("Level icon/thumbnail")]
        public Sprite icon;

        [Tooltip("Level description localization key")]
        public string descriptionKey;

        [Tooltip("Difficulty rating (1-5)")]
        [Range(1, 5)]
        public int difficulty = 1;
    }
}
