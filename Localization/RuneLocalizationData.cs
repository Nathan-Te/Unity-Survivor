using UnityEngine;
using System;

namespace SurvivorGame.Localization
{
    /// <summary>
    /// Centralized localization data for a single rune.
    /// Contains the rune name and all upgrade descriptions organized by rarity.
    /// This replaces individual LocalizedString assets with a single consolidated asset per rune.
    /// </summary>
    [CreateAssetMenu(fileName = "RuneLoc_", menuName = "Localization/Rune Localization Data")]
    public class RuneLocalizationData : ScriptableObject
    {
        [Header("Rune Name")]
        [Tooltip("Localized name of this rune (displayed in tooltips and UI)")]
        public LocalizedString runeName;

        [Header("Upgrade Descriptions by Rarity")]
        [Tooltip("Upgrade descriptions for Common rarity upgrades")]
        public LocalizedString[] commonDescriptions = new LocalizedString[0];

        [Tooltip("Upgrade descriptions for Rare rarity upgrades")]
        public LocalizedString[] rareDescriptions = new LocalizedString[0];

        [Tooltip("Upgrade descriptions for Epic rarity upgrades")]
        public LocalizedString[] epicDescriptions = new LocalizedString[0];

        [Tooltip("Upgrade descriptions for Legendary rarity upgrades")]
        public LocalizedString[] legendaryDescriptions = new LocalizedString[0];

        /// <summary>
        /// Gets the description for a specific upgrade by rarity and index.
        /// Returns null if the index is out of bounds.
        /// </summary>
        public LocalizedString GetDescription(Rarity rarity, int index)
        {
            LocalizedString[] descriptions = rarity switch
            {
                Rarity.Common => commonDescriptions,
                Rarity.Rare => rareDescriptions,
                Rarity.Epic => epicDescriptions,
                Rarity.Legendary => legendaryDescriptions,
                _ => null
            };

            if (descriptions == null || index < 0 || index >= descriptions.Length)
                return null;

            return descriptions[index];
        }

        /// <summary>
        /// Gets all descriptions for a specific rarity.
        /// </summary>
        public LocalizedString[] GetDescriptions(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => commonDescriptions,
                Rarity.Rare => rareDescriptions,
                Rarity.Epic => epicDescriptions,
                Rarity.Legendary => legendaryDescriptions,
                _ => new LocalizedString[0]
            };
        }

#if UNITY_EDITOR
        [Header("Editor Utilities")]
        [Tooltip("Auto-populate from linked RuneSO (fill this field and click 'Import From RuneSO')")]
        public RuneSO linkedRune;

        [ContextMenu("Import From Linked RuneSO")]
        private void ImportFromLinkedRune()
        {
            if (linkedRune == null)
            {
                Debug.LogWarning("No linked RuneSO to import from!");
                return;
            }

            // Import name
            runeName = linkedRune.runeName;

            // Import descriptions by rarity
            commonDescriptions = ImportDescriptions(linkedRune.CommonUpgrades);
            rareDescriptions = ImportDescriptions(linkedRune.RareUpgrades);
            epicDescriptions = ImportDescriptions(linkedRune.EpicUpgrades);
            legendaryDescriptions = ImportDescriptions(linkedRune.LegendaryUpgrades);

            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"Imported localization data from {linkedRune.name}");
        }

        private LocalizedString[] ImportDescriptions(System.Collections.Generic.List<RuneDefinition> upgrades)
        {
            if (upgrades == null || upgrades.Count == 0)
                return new LocalizedString[0];

            LocalizedString[] descriptions = new LocalizedString[upgrades.Count];
            for (int i = 0; i < upgrades.Count; i++)
            {
                descriptions[i] = upgrades[i].Description;
            }
            return descriptions;
        }

        [ContextMenu("Export To Linked RuneSO")]
        private void ExportToLinkedRune()
        {
            if (linkedRune == null)
            {
                Debug.LogWarning("No linked RuneSO to export to!");
                return;
            }

            // Export name
            linkedRune.runeName = runeName;

            // Export descriptions by rarity
            ExportDescriptions(linkedRune.CommonUpgrades, commonDescriptions);
            ExportDescriptions(linkedRune.RareUpgrades, rareDescriptions);
            ExportDescriptions(linkedRune.EpicUpgrades, epicDescriptions);
            ExportDescriptions(linkedRune.LegendaryUpgrades, legendaryDescriptions);

            UnityEditor.EditorUtility.SetDirty(linkedRune);
            Debug.Log($"Exported localization data to {linkedRune.name}");
        }

        private void ExportDescriptions(System.Collections.Generic.List<RuneDefinition> upgrades, LocalizedString[] descriptions)
        {
            if (upgrades == null || descriptions == null)
                return;

            int count = Mathf.Min(upgrades.Count, descriptions.Length);
            for (int i = 0; i < count; i++)
            {
                upgrades[i].Description = descriptions[i];
            }
        }
#endif
    }
}
