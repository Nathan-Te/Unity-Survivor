using UnityEngine;
using UnityEngine.UI;
using SurvivorGame.Localization;
using SurvivorGame.Progression;
using TMPro;
using System.Collections.Generic;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Meta-progression upgrades menu.
    /// Allows players to spend gold to unlock runes, upgrade max levels, increase spell slots, etc.
    /// </summary>
    public class UpgradesMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private Transform upgradeButtonContainer;
        [SerializeField] private UpgradeCardButton upgradeCardPrefab;

        [Header("Upgrade Definitions")]
        [SerializeField] private List<MetaUpgradeDefinition> availableUpgrades = new List<MetaUpgradeDefinition>();

        [Header("Settings")]
        [SerializeField] private bool verboseLogging = false;

        private List<UpgradeCardButton> _spawnedCards = new List<UpgradeCardButton>();

        private void Start()
        {
            if (backButton) backButton.onClick.AddListener(OnBackPressed);

            // Subscribe to progression changes
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.OnProgressionChanged += OnProgressionChanged;
            }

            // Subscribe to language changes
            SimpleLocalizationManager.OnLanguageChanged += RefreshText;

            PopulateUpgradeCards();
            RefreshText();

            if (verboseLogging)
                Debug.Log("[UpgradesMenuUI] Initialized");
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

            SimpleLocalizationManager.OnLanguageChanged -= RefreshText;
        }

        private void OnProgressionChanged(PlayerProgressionData data)
        {
            RefreshText();
            RefreshUpgradeCards();
        }

        private void RefreshText()
        {
            if (titleText != null)
            {
                titleText.text = SimpleLocalizationHelper.Get("MENU_UPGRADES", "Upgrades");
            }

            var data = ProgressionManager.Instance?.CurrentProgression;
            if (goldText != null && data != null)
            {
                //goldText.text = SimpleLocalizationHelper.FormatGold(data.gold);
                goldText.text = data.gold.ToString();
            }
        }

        /// <summary>
        /// Creates upgrade card buttons
        /// </summary>
        private void PopulateUpgradeCards()
        {
            if (upgradeCardPrefab == null || upgradeButtonContainer == null)
            {
                Debug.LogWarning("[UpgradesMenuUI] Missing prefab or container reference");
                return;
            }

            // Clear existing cards
            foreach (var card in _spawnedCards)
            {
                if (card != null) Destroy(card.gameObject);
            }
            _spawnedCards.Clear();

            // Create card for each upgrade
            foreach (var upgrade in availableUpgrades)
            {
                var cardObj = Instantiate(upgradeCardPrefab, upgradeButtonContainer);
                cardObj.Initialize(upgrade, OnUpgradePurchased);
                _spawnedCards.Add(cardObj);
            }

            if (verboseLogging)
                Debug.Log($"[UpgradesMenuUI] Spawned {_spawnedCards.Count} upgrade cards");
        }

        /// <summary>
        /// Refreshes card states without recreating them
        /// </summary>
        private void RefreshUpgradeCards()
        {
            foreach (var card in _spawnedCards)
            {
                if (card != null) card.Refresh();
            }
        }

        /// <summary>
        /// Called when an upgrade is purchased
        /// </summary>
        private void OnUpgradePurchased(MetaUpgradeDefinition upgrade)
        {
            if (upgrade == null || ProgressionManager.Instance == null) return;

            var progression = ProgressionManager.Instance.CurrentProgression;

            // Check if player has enough gold
            if (!ProgressionManager.Instance.SpendGold(upgrade.cost))
            {
                Debug.LogWarning($"[UpgradesMenuUI] Not enough gold for {upgrade.nameKey}");
                return;
            }

            // Apply upgrade effect
            switch (upgrade.upgradeType)
            {
                case MetaUpgradeType.UnlockRune:
                    if (!string.IsNullOrEmpty(upgrade.targetRuneId))
                    {
                        ProgressionManager.Instance.UnlockRune(upgrade.targetRuneId);
                    }
                    break;

                case MetaUpgradeType.IncreaseRuneMaxLevel:
                    if (!string.IsNullOrEmpty(upgrade.targetRuneId))
                    {
                        int currentMax = progression.GetRuneMaxLevel(upgrade.targetRuneId);
                        ProgressionManager.Instance.UpgradeRuneMaxLevel(upgrade.targetRuneId, currentMax + 1);
                    }
                    break;

                case MetaUpgradeType.IncreaseSpellSlots:
                    ProgressionManager.Instance.UpgradeMaxSpellSlots(1);
                    break;

                case MetaUpgradeType.UnlockLevel:
                    if (!string.IsNullOrEmpty(upgrade.targetLevelId))
                    {
                        ProgressionManager.Instance.UnlockLevel(upgrade.targetLevelId);
                    }
                    break;
            }

            if (verboseLogging)
                Debug.Log($"[UpgradesMenuUI] Purchased upgrade: {upgrade.nameKey}");
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
    /// Types of meta-progression upgrades
    /// </summary>
    public enum MetaUpgradeType
    {
        UnlockRune,
        IncreaseRuneMaxLevel,
        IncreaseSpellSlots,
        UnlockLevel
    }

    /// <summary>
    /// Definition for a purchasable meta-upgrade
    /// </summary>
    [System.Serializable]
    public class MetaUpgradeDefinition
    {
        [Tooltip("Display name localization key")]
        public string nameKey;

        [Tooltip("Description localization key")]
        public string descriptionKey;

        [Tooltip("Gold cost")]
        public int cost;

        [Tooltip("Type of upgrade")]
        public MetaUpgradeType upgradeType;

        [Tooltip("Target rune ID (for rune-related upgrades)")]
        public string targetRuneId;

        [Tooltip("Target level ID (for level unlocks)")]
        public string targetLevelId;

        [Tooltip("Icon for this upgrade")]
        public Sprite icon;
    }
}
