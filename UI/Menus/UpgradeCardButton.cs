using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SurvivorGame.Progression;
using SurvivorGame.Localization;
using System;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Individual upgrade card in the upgrades menu.
    /// Displays upgrade info and handles purchase.
    /// </summary>
    public class UpgradeCardButton : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject purchasedOverlay;

        private MetaUpgradeDefinition _upgradeData;
        private Action<MetaUpgradeDefinition> _onPurchased;

        private void Awake()
        {
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClicked);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClicked);
            }
        }

        /// <summary>
        /// Initializes the card with upgrade data
        /// </summary>
        public void Initialize(MetaUpgradeDefinition upgradeData, Action<MetaUpgradeDefinition> onPurchased)
        {
            _upgradeData = upgradeData;
            _onPurchased = onPurchased;

            Refresh();
        }

        /// <summary>
        /// Refreshes the card display based on current progression
        /// </summary>
        public void Refresh()
        {
            if (_upgradeData == null) return;

            // Update name
            if (nameText != null)
            {
                nameText.text = SimpleLocalizationHelper.Get(_upgradeData.nameKey, _upgradeData.nameKey);
            }

            // Update description
            if (descriptionText != null)
            {
                descriptionText.text = SimpleLocalizationHelper.Get(_upgradeData.descriptionKey, "");
            }

            // Update cost
            if (costText != null)
            {
                costText.text = SimpleLocalizationHelper.FormatGold(_upgradeData.cost);
            }

            // Update icon
            if (iconImage != null && _upgradeData.icon != null)
            {
                iconImage.sprite = _upgradeData.icon;
            }

            // Check if can afford and if already purchased
            var progression = ProgressionManager.Instance?.CurrentProgression;
            bool canAfford = progression != null && progression.gold >= _upgradeData.cost;
            bool isPurchased = IsUpgradePurchased(progression);

            // Update purchased state
            if (purchasedOverlay != null)
            {
                purchasedOverlay.SetActive(isPurchased);
            }

            // Enable/disable button
            if (button != null)
            {
                button.interactable = canAfford && !isPurchased;
            }
        }

        /// <summary>
        /// Checks if this upgrade has already been purchased
        /// </summary>
        private bool IsUpgradePurchased(PlayerProgressionData progression)
        {
            if (progression == null || _upgradeData == null) return false;

            switch (_upgradeData.upgradeType)
            {
                case MetaUpgradeType.UnlockRune:
                    return progression.IsRuneUnlocked(_upgradeData.targetRuneId);

                case MetaUpgradeType.UnlockLevel:
                    return progression.IsLevelUnlocked(_upgradeData.targetLevelId);

                case MetaUpgradeType.IncreaseRuneMaxLevel:
                case MetaUpgradeType.IncreaseSpellSlots:
                    // These can be purchased multiple times, so never mark as "purchased"
                    return false;

                default:
                    return false;
            }
        }

        private void OnButtonClicked()
        {
            _onPurchased?.Invoke(_upgradeData);
        }
    }
}
