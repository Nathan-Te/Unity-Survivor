namespace SurvivorGame.Localization
{
    /// <summary>
    /// Centralized constants for localization keys.
    /// Use these instead of hardcoded strings to avoid typos and enable refactoring.
    /// </summary>
    public static class LocalizationKeys
    {
        // Table names
        public const string TABLE_UI = "UI";
        public const string TABLE_STATS = "Stats";
        public const string TABLE_COMBAT = "Combat";

        // ===== UI Keys =====
        public const string UI_ENEMIES = "ENEMIES";
        public const string UI_KILLS = "KILLS";
        public const string UI_SCORE = "SCORE";
        public const string UI_COMBO = "COMBO";
        public const string UI_MULTIPLIER = "MULTIPLIER";
        public const string UI_LEVEL = "LEVEL";
        public const string UI_HEALTH = "HEALTH";

        // Level-Up UI
        public const string UI_LEVELUP_TITLE = "LEVELUP_TITLE";
        public const string UI_LEVELUP_SPECIAL = "LEVELUP_SPECIAL";
        public const string UI_LEVELUP_CHOOSE = "LEVELUP_CHOOSE";
        public const string UI_BAN_TITLE = "BAN_TITLE";
        public const string UI_BAN_CHOOSE = "BAN_CHOOSE";
        public const string UI_REROLL_COST = "REROLL_COST";
        public const string UI_BAN_STOCK = "BAN_STOCK";

        // Inventory UI
        public const string UI_APPLY_ON = "APPLY_ON";
        public const string UI_INCOMPATIBLE_FORM = "INCOMPATIBLE_FORM";
        public const string UI_ERROR_ADD_MODIFIER = "ERROR_ADD_MODIFIER";
        public const string UI_REPLACE_MODIFIER = "REPLACE_MODIFIER";

        // Tooltips
        public const string UI_LEVEL_LABEL = "LEVEL_LABEL";
        public const string UI_MAX_LEVEL = "MAX_LEVEL";
        public const string UI_TYPE_LABEL = "TYPE_LABEL";
        public const string UI_TARGET_LABEL = "TARGET_LABEL";
        public const string UI_TYPE_STAT_UPGRADE = "TYPE_STAT_UPGRADE";

        // ===== Stat Type Keys =====
        public const string STAT_MOVE_SPEED = "STAT_MOVE_SPEED";
        public const string STAT_MAX_HEALTH = "STAT_MAX_HEALTH";
        public const string STAT_HEALTH_REGEN = "STAT_HEALTH_REGEN";
        public const string STAT_ARMOR = "STAT_ARMOR";
        public const string STAT_MAGNET_AREA = "STAT_MAGNET_AREA";
        public const string STAT_EXPERIENCE_GAIN = "STAT_EXPERIENCE_GAIN";
        public const string STAT_GLOBAL_DAMAGE = "STAT_GLOBAL_DAMAGE";
        public const string STAT_GLOBAL_COOLDOWN = "STAT_GLOBAL_COOLDOWN";
        public const string STAT_GLOBAL_AREA = "STAT_GLOBAL_AREA";
        public const string STAT_GLOBAL_SPEED = "STAT_GLOBAL_SPEED";
        public const string STAT_GLOBAL_COUNT = "STAT_GLOBAL_COUNT";
        public const string STAT_CRIT_CHANCE = "STAT_CRIT_CHANCE";
        public const string STAT_CRIT_DAMAGE = "STAT_CRIT_DAMAGE";

        // ===== Element Type Keys =====
        public const string ELEMENT_PHYSICAL = "ELEMENT_PHYSICAL";
        public const string ELEMENT_FIRE = "ELEMENT_FIRE";
        public const string ELEMENT_ICE = "ELEMENT_ICE";
        public const string ELEMENT_LIGHTNING = "ELEMENT_LIGHTNING";
        public const string ELEMENT_NECROTIC = "ELEMENT_NECROTIC";

        // ===== Rarity Keys =====
        public const string RARITY_COMMON = "RARITY_COMMON";
        public const string RARITY_RARE = "RARITY_RARE";
        public const string RARITY_EPIC = "RARITY_EPIC";
        public const string RARITY_LEGENDARY = "RARITY_LEGENDARY";

        // ===== Combat Effect Labels =====
        public const string COMBAT_BURN = "BURN";
        public const string COMBAT_SLOW = "SLOW";
        public const string COMBAT_CHAIN = "CHAIN";
        public const string COMBAT_AOE = "AOE";
        public const string COMBAT_SUMMON = "SUMMON";
        public const string COMBAT_HOMING = "HOMING";

        // ===== Combat Stat Labels =====
        public const string COMBAT_DAMAGE = "DAMAGE";
        public const string COMBAT_COOLDOWN = "COOLDOWN";
        public const string COMBAT_COUNT = "COUNT";
        public const string COMBAT_PIERCE = "PIERCE";
        public const string COMBAT_SPREAD = "SPREAD";
        public const string COMBAT_RANGE = "RANGE";
        public const string COMBAT_CRIT_CHANCE = "CRIT_CHANCE";
        public const string COMBAT_CRIT_DAMAGE = "CRIT_DAMAGE";
        public const string COMBAT_SIZE = "SIZE";
        public const string COMBAT_SPEED = "SPEED";
        public const string COMBAT_DURATION = "DURATION";
        public const string COMBAT_KNOCKBACK = "KNOCKBACK";
        public const string COMBAT_MULTICAST = "MULTICAST";
    }
}
