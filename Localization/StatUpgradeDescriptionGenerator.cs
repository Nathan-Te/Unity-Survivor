using UnityEngine;

namespace SurvivorGame.Localization
{
    /// <summary>
    /// Generates randomized, localized descriptions for StatUpgrade runes.
    /// Provides variety by choosing from multiple description variants.
    /// </summary>
    public static class StatUpgradeDescriptionGenerator
    {
        private const int VARIANT_COUNT = 5;

        /// <summary>
        /// Generates a random localized description for a stat upgrade.
        /// Format: "[Variant] [StatName]\n+[FormattedValue]"
        /// Example: "Increases your Speed\n+10.0%"
        /// </summary>
        /// <param name="statType">The stat being upgraded</param>
        /// <param name="statValue">The value of the upgrade (e.g., 0.1 for +10%)</param>
        /// <returns>A formatted, localized description with color coding</returns>
        public static string GenerateRandomDescription(StatType statType, float statValue)
        {
            if (statValue == 0)
                return SimpleLocalizationHelper.Get("UPGRADE_NO_STATS", "No bonus stats");

            // Pick a random variant (1-5)
            int variantIndex = Random.Range(1, VARIANT_COUNT + 1);
            string variantKey = $"STAT_DESC_VAR{variantIndex}";

            // Get stat name
            string statName = SimpleLocalizationHelper.GetStatName(statType);

            // Get the variant template (e.g., "Increases your {0}")
            string template = SimpleLocalizationHelper.Get(variantKey, "Boosts {0}");

            // Format the description
            string description = string.Format(template, statName);

            // Add the formatted value with color
            string formattedValue = FormatStatValue(statType, statValue);
            description += $"\n<color=green>{formattedValue}</color>";

            return description;
        }

        /// <summary>
        /// Generates a description using a specific variant index (for testing/consistency)
        /// </summary>
        public static string GenerateDescription(StatType statType, float statValue, int variantIndex)
        {
            if (statValue == 0)
                return SimpleLocalizationHelper.Get("UPGRADE_NO_STATS", "No bonus stats");

            // Clamp variant index to valid range
            variantIndex = Mathf.Clamp(variantIndex, 1, VARIANT_COUNT);
            string variantKey = $"STAT_DESC_VAR{variantIndex}";

            string statName = SimpleLocalizationHelper.GetStatName(statType);
            string template = SimpleLocalizationHelper.Get(variantKey, "Boosts {0}");
            string description = string.Format(template, statName);

            string formattedValue = FormatStatValue(statType, statValue);
            description += $"\n<color=green>{formattedValue}</color>";

            return description;
        }

        /// <summary>
        /// Formats a stat value based on the stat type (percentage vs flat value)
        /// </summary>
        private static string FormatStatValue(StatType statType, float value)
        {
            switch (statType)
            {
                // Percentage-based stats
                case StatType.MoveSpeed:
                case StatType.GlobalDamage:
                case StatType.GlobalCooldown:
                case StatType.GlobalArea:
                case StatType.GlobalSpeed:
                case StatType.ExperienceGain:
                case StatType.Armor:
                case StatType.CritChance:
                case StatType.CritDamage:
                    return $"+{(value * 100f).ToString("F1")}%";

                // Flat value stats
                case StatType.MaxHealth:
                case StatType.HealthRegen:
                case StatType.MagnetArea:
                    return $"+{value.ToString("F1")}";

                // Integer stats
                case StatType.GlobalCount:
                    return $"+{((int)value).ToString()}";

                default:
                    return $"+{value.ToString("F1")}";
            }
        }
    }
}
