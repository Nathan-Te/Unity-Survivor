using System.Collections.Generic;
using UnityEngine;

public enum RuneType { Form, Effect, Modifier }

public abstract class RuneSO : ScriptableObject
{
    [Header("Info")]
    public string runeName;
    public Sprite icon;

    public abstract RuneType Type { get; }

    // Listes manuelles d'améliorations par rareté
    [Header("Améliorations Possibles")]
    public List<RuneDefinition> CommonUpgrades;
    public List<RuneDefinition> RareUpgrades;
    public List<RuneDefinition> EpicUpgrades;
    public List<RuneDefinition> LegendaryUpgrades;

    // Pioche une upgrade au hasard selon la rareté
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

        return new RuneDefinition { Description = "Aucun bonus (Liste vide)", Stats = RuneStats.Zero };
    }
}