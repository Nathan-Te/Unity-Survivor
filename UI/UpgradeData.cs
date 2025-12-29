using UnityEngine;

public enum UpgradeType { NewSpell, SpellUpgrade, Modifier, Effect, StatBoost }

[System.Serializable]
public class UpgradeData
{
    public string Name;
    public string Description;
    public Sprite Icon;
    public UpgradeType Type;
    public Rarity Rarity;

    // La d�finition pr�cise (Stats) choisie pour cette carte
    public RuneDefinition UpgradeDefinition;

    public RuneSO TargetRuneSO; // R�f�rence g�n�rique

    // Casting Helpers
    public SpellForm TargetForm => TargetRuneSO as SpellForm;
    public SpellModifier TargetModifier => TargetRuneSO as SpellModifier;
    public SpellEffect TargetEffect => TargetRuneSO as SpellEffect;
    public StatUpgradeSO TargetStat => TargetRuneSO as StatUpgradeSO;

    // Constructeur G�n�rique (Fonctionne pour tout type de RuneSO)
    public UpgradeData(RuneSO so, Rarity rarity)
    {
        TargetRuneSO = so;
        Rarity = rarity;

        // On pioche l'upgrade al�atoire dans les listes manuelles du SO
        UpgradeDefinition = so.GetRandomUpgrade(rarity);

        // Setup UI
        Name = so.GetLocalizedName();
        Icon = so.icon;
        Description = UpgradeDefinition.Description;

        // D�duction du type
        if (so is SpellForm) Type = UpgradeType.NewSpell;
        else if (so is SpellEffect) Type = UpgradeType.Effect;
        else if (so is SpellModifier) Type = UpgradeType.Modifier;
        else if (so is StatUpgradeSO) Type = UpgradeType.StatBoost;
    }
}