using System.Collections.Generic;
using UnityEngine;
using SurvivorGame.Localization;

public enum RuneType { Form, Effect, Modifier }

public abstract class RuneSO : ScriptableObject
{
    [Header("Info")]
    [Tooltip("JSON key for this rune's localized name (e.g., 'RUNE_BOLT', 'RUNE_FIRE'). Names are defined in en.json/fr.json")]
    public string runeNameKey;

    [Tooltip("DEPRECATED: Use runeNameKey instead. Kept for backwards compatibility.")]
    public LocalizedString runeName;

    public Sprite icon;

    public abstract RuneType Type { get; }

    /// <summary>
    /// Gets the localized name of this rune from JSON using runeNameKey.
    /// Falls back to runeName LocalizedString if runeNameKey is not set (backwards compatibility).
    /// </summary>
    public string GetLocalizedName()
    {
        if (!string.IsNullOrEmpty(runeNameKey))
        {
            return SimpleLocalizationHelper.Get(runeNameKey, name);
        }

        // Fallback to old LocalizedString system
        if (runeName != null)
        {
            return runeName.GetText();
        }

        return name; // Last resort: use asset name
    }

    // Listes manuelles : Tu remplis �a dans l'�diteur
    [Header("Upgrades par Raret�")]
    public List<RuneDefinition> CommonUpgrades;
    public List<RuneDefinition> RareUpgrades;
    public List<RuneDefinition> EpicUpgrades;
    public List<RuneDefinition> LegendaryUpgrades;

    public RuneDefinition GetRandomUpgrade(Rarity rarity)
    {
        List<RuneDefinition> list = null;
        switch (rarity)
        {
            case Rarity.Common: list = CommonUpgrades; break;
            case Rarity.Rare: list = RareUpgrades; break;
            case Rarity.Epic: list = EpicUpgrades; break;
            case Rarity.Legendary: list = LegendaryUpgrades; break;
        }

        if (list != null && list.Count > 0)
        {
            return list[Random.Range(0, list.Count)];
        }

        // Fallback
        return new RuneDefinition { Description = null, Stats = RuneStats.Zero };
    }

    /// <summary>
    /// Checks if a rune at the given level has reached its maximum level (uses global config)
    /// </summary>
    public bool IsMaxLevel(int currentLevel)
    {
        if (RuneMaxLevelConfig.Instance == null)
            return false;

        return RuneMaxLevelConfig.Instance.IsMaxLevel(Type, currentLevel);
    }

    /// <summary>
    /// Gets the max level for this rune type from global config
    /// </summary>
    public int GetMaxLevel()
    {
        if (RuneMaxLevelConfig.Instance == null)
            return 0;

        return RuneMaxLevelConfig.Instance.GetMaxLevel(Type);
    }
}