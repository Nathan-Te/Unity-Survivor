using System.Collections.Generic;
using UnityEngine;

public enum RuneType { Form, Effect, Modifier }

public abstract class RuneSO : ScriptableObject
{
    [Header("Info")]
    public string runeName;
    public Sprite icon;

    public abstract RuneType Type { get; }

    // Listes manuelles : Tu remplis ça dans l'éditeur
    [Header("Upgrades par Rareté")]
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
        return new RuneDefinition { Description = "Upgrade Vide", Stats = RuneStats.Zero };
    }
}