using System.Text;
using UnityEngine;

namespace SurvivorGame.Localization
{
    /// <summary>
    /// Automatically generates localized descriptions for rune upgrades based on their stats.
    /// Only displays stats that are > 0, creating clean, concise descriptions.
    /// </summary>
    public static class RuneDescriptionGenerator
    {
        private static StringBuilder _sb = new StringBuilder(256);

        /// <summary>
        /// Generates a description for a RuneDefinition by analyzing its stats.
        /// Only includes stats that are strictly greater than 0.
        /// </summary>
        public static string GenerateDescription(RuneDefinition definition)
        {
            if (definition == null)
                return "";

            _sb.Clear();

            // Get the stats from the definition
            RuneStats stats = definition.Stats;

            // Build description based on non-zero stats
            bool hasContent = false;

            // Multiplicative bonuses (displayed as percentages)
            hasContent |= AppendMultiplier(stats.DamageMult, SimpleLocalizationHelper.GetDamageLabel());
            hasContent |= AppendMultiplier(stats.CooldownMult, SimpleLocalizationHelper.GetCooldownLabel());
            hasContent |= AppendMultiplier(stats.SizeMult, SimpleLocalizationHelper.GetSizeLabel());
            hasContent |= AppendMultiplier(stats.SpeedMult, SimpleLocalizationHelper.GetSpeedLabel());
            hasContent |= AppendMultiplier(stats.DurationMult, SimpleLocalizationHelper.GetDurationLabel());

            // Additive flat bonuses
            hasContent |= AppendFlat(stats.FlatCooldown, SimpleLocalizationHelper.GetCooldownLabel(), "s", allowNegative: true);
            hasContent |= AppendFlat(stats.FlatCount, SimpleLocalizationHelper.GetCountLabel());
            hasContent |= AppendFlat(stats.FlatPierce, SimpleLocalizationHelper.GetPierceLabel());
            hasContent |= AppendFlat(stats.FlatSpread, SimpleLocalizationHelper.GetSpreadLabel(), "°");
            hasContent |= AppendFlat(stats.FlatRange, SimpleLocalizationHelper.GetRangeLabel(), "m");
            hasContent |= AppendFlat(stats.FlatKnockback, SimpleLocalizationHelper.GetKnockbackLabel());
            hasContent |= AppendFlat(stats.FlatChainCount, "Chain");
            hasContent |= AppendFlat(stats.FlatMulticast, SimpleLocalizationHelper.GetMulticastLabel());

            // Burn bonuses
            hasContent |= AppendFlat(stats.FlatBurnDamage, "Burn " + SimpleLocalizationHelper.GetDamageLabel());
            hasContent |= AppendFlat(stats.FlatBurnDuration, "Burn " + SimpleLocalizationHelper.GetDurationLabel(), "s");

            // Crit bonuses (displayed as percentages)
            hasContent |= AppendCritChance(stats.FlatCritChance);
            hasContent |= AppendCritDamage(stats.FlatCritDamage);

            // NOTE: StatUpgrade runes (with StatValue) are handled separately
            // via their Description field, since they need StatUpgradeSO.targetStat
            // which isn't available in RuneStats alone

            // If nothing was added, return a default message
            if (!hasContent)
            {
                return SimpleLocalizationHelper.Get("UPGRADE_NO_STATS", "No bonus stats");
            }

            return _sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Appends a multiplicative bonus (e.g., "+20% Damage")
        /// Only appends if mult != 0
        /// </summary>
        private static bool AppendMultiplier(float mult, string label)
        {
            if (mult == 0)
                return false;

            float percent = mult * 100f;
            string sign = mult > 0 ? "+" : "";
            _sb.Append(sign).Append(percent.ToString("F0")).Append("% ").Append(label).Append("\n");
            return true;
        }

        /// <summary>
        /// Appends a flat bonus (e.g., "+3 Projectiles")
        /// Only appends if value != 0
        /// </summary>
        private static bool AppendFlat(float value, string label, string suffix = "", bool allowNegative = false)
        {
            if (value == 0)
                return false;

            // Skip negative values unless explicitly allowed
            if (!allowNegative && value < 0)
                return false;

            string sign = value > 0 ? "+" : "";
            _sb.Append(sign).Append(value.ToString("F0")).Append(suffix).Append(" ").Append(label).Append("\n");
            return true;
        }

        /// <summary>
        /// Appends crit chance bonus (e.g., "+15% Crit Chance")
        /// Only appends if critChance > 0
        /// </summary>
        private static bool AppendCritChance(float critChance)
        {
            if (critChance <= 0)
                return false;

            float percent = critChance * 100f;
            _sb.Append("+").Append(percent.ToString("F1")).Append("% ").Append(SimpleLocalizationHelper.GetCritChanceLabel()).Append("\n");
            return true;
        }

        /// <summary>
        /// Appends crit damage bonus (e.g., "+50% Crit Damage")
        /// Only appends if critDamage > 0
        /// </summary>
        private static bool AppendCritDamage(float critDamage)
        {
            if (critDamage <= 0)
                return false;

            float percent = critDamage * 100f;
            _sb.Append("+").Append(percent.ToString("F1")).Append("% ").Append(SimpleLocalizationHelper.GetCritDamageLabel()).Append("\n");
            return true;
        }

        /// <summary>
        /// Generates a compact single-line description (for upgrade cards)
        /// </summary>
        public static string GenerateCompactDescription(RuneDefinition definition)
        {
            string fullDesc = GenerateDescription(definition);

            // Replace newlines with commas for compact display
            return fullDesc.Replace("\n", ", ").TrimEnd(',', ' ');
        }

        /// <summary>
        /// Generates a rich text description with color coding
        /// </summary>
        public static string GenerateRichDescription(RuneDefinition definition)
        {
            if (definition == null)
                return "";

            _sb.Clear();

            RuneStats stats = definition.Stats;
            bool hasContent = false;

            // Positive bonuses in green
            hasContent |= AppendRichMultiplier(stats.DamageMult, SimpleLocalizationHelper.GetDamageLabel(), "green");
            hasContent |= AppendRichMultiplier(stats.SizeMult, SimpleLocalizationHelper.GetSizeLabel(), "green");
            hasContent |= AppendRichMultiplier(stats.SpeedMult, SimpleLocalizationHelper.GetSpeedLabel(), "green");
            hasContent |= AppendRichMultiplier(stats.DurationMult, SimpleLocalizationHelper.GetDurationLabel(), "green");

            // Cooldown in green if negative (reduction), red if positive
            if (stats.CooldownMult != 0)
            {
                string color = stats.CooldownMult < 0 ? "green" : "red";
                hasContent |= AppendRichMultiplier(stats.CooldownMult, SimpleLocalizationHelper.GetCooldownLabel(), color);
            }

            // Flat bonuses
            hasContent |= AppendRichFlat(stats.FlatCount, SimpleLocalizationHelper.GetCountLabel());
            hasContent |= AppendRichFlat(stats.FlatPierce, SimpleLocalizationHelper.GetPierceLabel());
            hasContent |= AppendRichFlat(stats.FlatMulticast, SimpleLocalizationHelper.GetMulticastLabel());

            // Crit bonuses in gold
            hasContent |= AppendRichCrit(stats.FlatCritChance, SimpleLocalizationHelper.GetCritChanceLabel());
            hasContent |= AppendRichCrit(stats.FlatCritDamage, SimpleLocalizationHelper.GetCritDamageLabel());

            if (!hasContent)
            {
                return SimpleLocalizationHelper.Get("UPGRADE_NO_STATS", "No bonus stats");
            }

            return _sb.ToString().TrimEnd();
        }

        private static bool AppendRichMultiplier(float mult, string label, string color)
        {
            if (mult == 0)
                return false;

            float percent = mult * 100f;
            string sign = mult > 0 ? "+" : "";
            _sb.Append("<color=").Append(color).Append(">")
              .Append(sign).Append(percent.ToString("F0")).Append("% ")
              .Append(label).Append("</color>\n");
            return true;
        }

        private static bool AppendRichFlat(float value, string label)
        {
            if (value <= 0)
                return false;

            _sb.Append("<color=green>+").Append(value.ToString("F0")).Append(" ")
              .Append(label).Append("</color>\n");
            return true;
        }

        private static bool AppendRichCrit(float value, string label)
        {
            if (value <= 0)
                return false;

            float percent = value * 100f;
            _sb.Append("<color=#FFD700>+").Append(percent.ToString("F1")).Append("% ")
              .Append(label).Append("</color>\n");
            return true;
        }
    }
}
