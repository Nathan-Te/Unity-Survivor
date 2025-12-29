
namespace SurvivorGame.Localization
{
    /// <summary>
    /// Helper methods for common localization tasks.
    /// Simplifies code and provides shortcuts for frequently-used operations.
    /// </summary>
    public static class LocalizationHelper
    {
        // ===== UI Shortcuts =====

        public static string GetUIString(string key, string fallback = "")
        {
            return LocalizationManager.Instance?.GetString(LocalizationKeys.TABLE_UI, key, fallback) ?? fallback;
        }

        public static string GetUIFormatted(string key, params object[] args)
        {
            return LocalizationManager.Instance?.GetFormattedString(LocalizationKeys.TABLE_UI, key, args) ?? "";
        }

        public static string GetStatsString(string key, string fallback = "")
        {
            return LocalizationManager.Instance?.GetString(LocalizationKeys.TABLE_STATS, key, fallback) ?? fallback;
        }

        public static string GetCombatString(string key, string fallback = "")
        {
            return LocalizationManager.Instance?.GetString(LocalizationKeys.TABLE_COMBAT, key, fallback) ?? fallback;
        }

        public static string GetCombatFormatted(string key, params object[] args)
        {
            return LocalizationManager.Instance?.GetFormattedString(LocalizationKeys.TABLE_COMBAT, key, args) ?? "";
        }

        // ===== Common UI Patterns =====

        public static string FormatEnemyCount(int count)
        {
            return GetUIFormatted(LocalizationKeys.UI_ENEMIES, count);
        }

        public static string FormatKillCount(int count)
        {
            return GetUIFormatted(LocalizationKeys.UI_KILLS, count);
        }

        public static string FormatScore(int score)
        {
            return GetUIFormatted(LocalizationKeys.UI_SCORE, score);
        }

        public static string FormatCombo(int combo)
        {
            return GetUIFormatted(LocalizationKeys.UI_COMBO, combo);
        }

        public static string FormatMultiplier(float multiplier)
        {
            return GetUIFormatted(LocalizationKeys.UI_MULTIPLIER, multiplier);
        }

        public static string FormatLevel(int level)
        {
            return GetUIFormatted(LocalizationKeys.UI_LEVEL, level);
        }

        public static string FormatHealth(float current, float max)
        {
            return GetUIFormatted(LocalizationKeys.UI_HEALTH, current, max);
        }

        public static string FormatLevelWithMax(int level, int maxLevel)
        {
            return GetUIFormatted(LocalizationKeys.UI_MAX_LEVEL, level, maxLevel);
        }

        public static string FormatRerollCost(int cost)
        {
            return GetUIFormatted(LocalizationKeys.UI_REROLL_COST, cost);
        }

        public static string FormatBanStock(int stock)
        {
            return GetUIFormatted(LocalizationKeys.UI_BAN_STOCK, stock);
        }

        // ===== Combat Patterns =====

        public static string FormatBurn(float damagePerTick, float duration)
        {
            return GetCombatFormatted(LocalizationKeys.COMBAT_BURN, damagePerTick, duration);
        }

        public static string FormatChain(int count)
        {
            return GetCombatFormatted(LocalizationKeys.COMBAT_CHAIN, count);
        }

        public static string FormatAoE(float radius)
        {
            return GetCombatFormatted(LocalizationKeys.COMBAT_AOE, radius);
        }

        public static string FormatSummon(float chance)
        {
            return GetCombatFormatted(LocalizationKeys.COMBAT_SUMMON, chance);
        }

        public static string GetSlowLabel()
        {
            return GetCombatString(LocalizationKeys.COMBAT_SLOW);
        }

        public static string GetHomingLabel()
        {
            return GetCombatString(LocalizationKeys.COMBAT_HOMING);
        }

        // ===== Stat Labels =====

        public static string GetDamageLabel()
        {
            return GetCombatString(LocalizationKeys.COMBAT_DAMAGE);
        }

        public static string GetCooldownLabel()
        {
            return GetCombatString(LocalizationKeys.COMBAT_COOLDOWN);
        }

        public static string GetCountLabel()
        {
            return GetCombatString(LocalizationKeys.COMBAT_COUNT);
        }

        public static string GetPierceLabel()
        {
            return GetCombatString(LocalizationKeys.COMBAT_PIERCE);
        }

        public static string GetSpreadLabel()
        {
            return GetCombatString(LocalizationKeys.COMBAT_SPREAD);
        }

        public static string GetRangeLabel()
        {
            return GetCombatString(LocalizationKeys.COMBAT_RANGE);
        }

        public static string GetCritChanceLabel()
        {
            return GetCombatString(LocalizationKeys.COMBAT_CRIT_CHANCE);
        }

        public static string GetCritDamageLabel()
        {
            return GetCombatString(LocalizationKeys.COMBAT_CRIT_DAMAGE);
        }

        public static string GetSizeLabel()
        {
            return GetCombatString(LocalizationKeys.COMBAT_SIZE);
        }

        public static string GetSpeedLabel()
        {
            return GetCombatString(LocalizationKeys.COMBAT_SPEED);
        }

        public static string GetDurationLabel()
        {
            return GetCombatString(LocalizationKeys.COMBAT_DURATION);
        }

        public static string GetKnockbackLabel()
        {
            return GetCombatString(LocalizationKeys.COMBAT_KNOCKBACK);
        }

        public static string GetMulticastLabel()
        {
            return GetCombatString(LocalizationKeys.COMBAT_MULTICAST);
        }
    }
}
