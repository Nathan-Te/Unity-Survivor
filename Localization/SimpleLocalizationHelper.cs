
namespace SurvivorGame.Localization
{
    /// <summary>
    /// Helper methods for common localization tasks with SimpleLocalizationManager.
    /// Provides shortcuts for frequently-used operations.
    /// </summary>
    public static class SimpleLocalizationHelper
    {
        // ===== Quick Access =====

        public static string Get(string key, string fallback = "")
        {
            return SimpleLocalizationManager.Instance?.GetString(key, fallback) ?? fallback;
        }

        public static string GetFormatted(string key, params object[] args)
        {
            return SimpleLocalizationManager.Instance?.GetFormattedString(key, args) ?? "";
        }

        // ===== HUD =====

        public static string FormatEnemyCount(int count)
        {
            return GetFormatted("HUD_ENEMIES", count);
        }

        public static string FormatKillCount(int count)
        {
            return GetFormatted("HUD_KILLS", count);
        }

        public static string FormatScore(int score)
        {
            return GetFormatted("HUD_SCORE", score);
        }

        public static string FormatCombo(int combo)
        {
            return GetFormatted("HUD_COMBO", combo);
        }

        public static string FormatMultiplier(float multiplier)
        {
            return GetFormatted("HUD_MULTIPLIER", multiplier);
        }

        public static string FormatLevel(int level)
        {
            return GetFormatted("HUD_LEVEL", level);
        }

        public static string FormatHealth(float current, float max)
        {
            return GetFormatted("HUD_HEALTH", current, max);
        }

        public static string FormatGold(int gold)
        {
            return GetFormatted("HUD_GOLD", gold);
        }

        // ===== Level Up UI =====

        public static string GetLevelUpTitle()
        {
            return Get("LEVELUP_TITLE");
        }

        public static string GetLevelUpSpecial()
        {
            return Get("LEVELUP_SPECIAL");
        }

        public static string GetLevelUpChoose()
        {
            return Get("LEVELUP_CHOOSE");
        }

        public static string GetBanTitle()
        {
            return Get("BAN_TITLE");
        }

        public static string FormatRerollCost(int cost)
        {
            return GetFormatted("REROLL_COST", cost);
        }

        public static string FormatBanStock(int stock)
        {
            return GetFormatted("BAN_STOCK", stock);
        }

        // ===== Inventory UI =====

        public static string FormatApplyOn(string upgradeName)
        {
            return GetFormatted("APPLY_ON", upgradeName);
        }

        public static string GetIncompatibleForm()
        {
            return Get("INCOMPATIBLE_FORM");
        }

        public static string GetErrorAddModifier()
        {
            return Get("ERROR_ADD_MODIFIER");
        }

        public static string GetReplaceModifier()
        {
            return Get("REPLACE_MODIFIER");
        }

        public static string GetDuplicateSpell()
        {
            return Get("DUPLICATE_SPELL");
        }

        public static string GetInventoryFull()
        {
            return Get("INVENTORY_FULL");
        }

        public static string GetApplyEffect()
        {
            return Get("APPLY_EFFECT");
        }

        public static string GetApplyModifier()
        {
            return Get("APPLY_MODIFIER");
        }

        // ===== Tooltips =====

        public static string FormatLevelWithMax(int level, int maxLevel)
        {
            return GetFormatted("TOOLTIP_MAX_LEVEL", level, maxLevel);
        }

        public static string GetTooltipLevel()
        {
            return Get("TOOLTIP_LEVEL");
        }

        public static string GetTooltipType()
        {
            return Get("TOOLTIP_TYPE");
        }

        public static string GetTooltipTarget()
        {
            return Get("TOOLTIP_TARGET");
        }

        public static string GetStatUpgradeType()
        {
            return Get("TOOLTIP_STAT_UPGRADE");
        }

        // ===== Rune Types =====

        public static string GetRuneTypeName(UpgradeType upgradeType)
        {
            string key = upgradeType switch
            {
                UpgradeType.NewSpell => "RUNE_TYPE_FORM",
                UpgradeType.Effect => "RUNE_TYPE_EFFECT",
                UpgradeType.Modifier => "RUNE_TYPE_MODIFIER",
                UpgradeType.StatBoost => "RUNE_TYPE_STAT",
                UpgradeType.SpellUpgrade => "RUNE_TYPE_FORM", // Spell upgrade is also a form
                _ => ""
            };

            return Get(key, upgradeType.ToString());
        }

        // ===== Stats =====

        public static string GetStatName(StatType statType)
        {
            string key = statType switch
            {
                StatType.MoveSpeed => "STAT_MOVE_SPEED",
                StatType.MaxHealth => "STAT_MAX_HEALTH",
                StatType.HealthRegen => "STAT_HEALTH_REGEN",
                StatType.Armor => "STAT_ARMOR",
                StatType.MagnetArea => "STAT_MAGNET_AREA",
                StatType.ExperienceGain => "STAT_EXPERIENCE_GAIN",
                StatType.GlobalDamage => "STAT_GLOBAL_DAMAGE",
                StatType.GlobalCooldown => "STAT_GLOBAL_COOLDOWN",
                StatType.GlobalArea => "STAT_GLOBAL_AREA",
                StatType.GlobalSpeed => "STAT_GLOBAL_SPEED",
                StatType.GlobalCount => "STAT_GLOBAL_COUNT",
                StatType.CritChance => "STAT_CRIT_CHANCE",
                StatType.CritDamage => "STAT_CRIT_DAMAGE",
                _ => ""
            };

            return Get(key, statType.ToString());
        }

        // ===== Elements =====

        public static string GetElementName(ElementType elementType)
        {
            string key = elementType switch
            {
                ElementType.Physical => "ELEMENT_PHYSICAL",
                ElementType.Fire => "ELEMENT_FIRE",
                ElementType.Ice => "ELEMENT_ICE",
                ElementType.Lightning => "ELEMENT_LIGHTNING",
                ElementType.Necrotic => "ELEMENT_NECROTIC",
                _ => ""
            };

            return Get(key, elementType.ToString());
        }

        // ===== Rarities =====

        public static string GetRarityName(Rarity rarity)
        {
            string key = rarity switch
            {
                Rarity.Common => "RARITY_COMMON",
                Rarity.Rare => "RARITY_RARE",
                Rarity.Epic => "RARITY_EPIC",
                Rarity.Legendary => "RARITY_LEGENDARY",
                _ => ""
            };

            return Get(key, rarity.ToString());
        }

        // ===== Combat Effects =====

        public static string FormatBurn(float damagePerTick, float duration)
        {
            return GetFormatted("EFFECT_BURN", damagePerTick, duration);
        }

        public static string GetSlow()
        {
            return Get("EFFECT_SLOW");
        }

        public static string FormatChain(int count)
        {
            return GetFormatted("EFFECT_CHAIN", count);
        }

        public static string FormatAoE(float radius)
        {
            return GetFormatted("EFFECT_AOE", radius);
        }

        public static string FormatSummon(float chance)
        {
            return GetFormatted("EFFECT_SUMMON", chance);
        }

        public static string GetHoming()
        {
            return Get("EFFECT_HOMING");
        }

        // ===== Combat Labels =====

        public static string GetDamageLabel() => Get("LABEL_DAMAGE");
        public static string GetCooldownLabel() => Get("LABEL_COOLDOWN");
        public static string GetCountLabel() => Get("LABEL_COUNT");
        public static string GetPierceLabel() => Get("LABEL_PIERCE");
        public static string GetSpreadLabel() => Get("LABEL_SPREAD");
        public static string GetRangeLabel() => Get("LABEL_RANGE");
        public static string GetCritChanceLabel() => Get("LABEL_CRIT_CHANCE");
        public static string GetCritDamageLabel() => Get("LABEL_CRIT_DAMAGE");
        public static string GetSizeLabel() => Get("LABEL_SIZE");
        public static string GetSpeedLabel() => Get("LABEL_SPEED");
        public static string GetDurationLabel() => Get("LABEL_DURATION");
        public static string GetKnockbackLabel() => Get("LABEL_KNOCKBACK");
        public static string GetMulticastLabel() => Get("LABEL_MULTICAST");
    }
}
