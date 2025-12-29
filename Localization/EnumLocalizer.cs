
namespace SurvivorGame.Localization
{
    /// <summary>
    /// Utility class for getting localized names of enums.
    /// Maps enum values to localization keys.
    /// </summary>
    public static class EnumLocalizer
    {
        /// <summary>
        /// Gets the localized name of a StatType.
        /// </summary>
        public static string GetStatName(StatType statType)
        {
            string key = statType switch
            {
                StatType.MoveSpeed => LocalizationKeys.STAT_MOVE_SPEED,
                StatType.MaxHealth => LocalizationKeys.STAT_MAX_HEALTH,
                StatType.HealthRegen => LocalizationKeys.STAT_HEALTH_REGEN,
                StatType.Armor => LocalizationKeys.STAT_ARMOR,
                StatType.MagnetArea => LocalizationKeys.STAT_MAGNET_AREA,
                StatType.ExperienceGain => LocalizationKeys.STAT_EXPERIENCE_GAIN,
                StatType.GlobalDamage => LocalizationKeys.STAT_GLOBAL_DAMAGE,
                StatType.GlobalCooldown => LocalizationKeys.STAT_GLOBAL_COOLDOWN,
                StatType.GlobalArea => LocalizationKeys.STAT_GLOBAL_AREA,
                StatType.GlobalSpeed => LocalizationKeys.STAT_GLOBAL_SPEED,
                StatType.GlobalCount => LocalizationKeys.STAT_GLOBAL_COUNT,
                StatType.CritChance => LocalizationKeys.STAT_CRIT_CHANCE,
                StatType.CritDamage => LocalizationKeys.STAT_CRIT_DAMAGE,
                _ => statType.ToString()
            };

            if (LocalizationManager.Instance != null)
            {
                return LocalizationManager.Instance.GetString(LocalizationKeys.TABLE_STATS, key, statType.ToString());
            }

            return statType.ToString();
        }

        /// <summary>
        /// Gets the localized name of an ElementType.
        /// </summary>
        public static string GetElementName(ElementType elementType)
        {
            string key = elementType switch
            {
                ElementType.Physical => LocalizationKeys.ELEMENT_PHYSICAL,
                ElementType.Fire => LocalizationKeys.ELEMENT_FIRE,
                ElementType.Ice => LocalizationKeys.ELEMENT_ICE,
                ElementType.Lightning => LocalizationKeys.ELEMENT_LIGHTNING,
                ElementType.Necrotic => LocalizationKeys.ELEMENT_NECROTIC,
                _ => elementType.ToString()
            };

            if (LocalizationManager.Instance != null)
            {
                return LocalizationManager.Instance.GetString(LocalizationKeys.TABLE_COMBAT, key, elementType.ToString());
            }

            return elementType.ToString();
        }

        /// <summary>
        /// Gets the localized name of a Rarity.
        /// </summary>
        public static string GetRarityName(Rarity rarity)
        {
            string key = rarity switch
            {
                Rarity.Common => LocalizationKeys.RARITY_COMMON,
                Rarity.Rare => LocalizationKeys.RARITY_RARE,
                Rarity.Epic => LocalizationKeys.RARITY_EPIC,
                Rarity.Legendary => LocalizationKeys.RARITY_LEGENDARY,
                _ => rarity.ToString()
            };

            if (LocalizationManager.Instance != null)
            {
                return LocalizationManager.Instance.GetString(LocalizationKeys.TABLE_COMBAT, key, rarity.ToString());
            }

            return rarity.ToString();
        }
    }
}
