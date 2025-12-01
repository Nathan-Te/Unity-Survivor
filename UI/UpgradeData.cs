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

    // La définition précise (Stats) choisie pour cette carte
    public RuneDefinition UpgradeDefinition;

    public RuneSO TargetRuneSO; // Référence générique

    // Casting Helpers
    public SpellForm TargetForm => TargetRuneSO as SpellForm;
    public SpellModifier TargetModifier => TargetRuneSO as SpellModifier;
    public SpellEffect TargetEffect => TargetRuneSO as SpellEffect;
    public StatUpgradeSO TargetStat => TargetRuneSO as StatUpgradeSO;

    // Constructeur Générique (Fonctionne pour tout type de RuneSO)
    public UpgradeData(RuneSO so, Rarity rarity)
    {
        TargetRuneSO = so;
        Rarity = rarity;

        // On pioche l'upgrade aléatoire dans les listes manuelles du SO
        UpgradeDefinition = so.GetRandomUpgrade(rarity);

        // Setup UI
        Name = so.runeName;
        Icon = so.icon;
        Description = UpgradeDefinition.Description;

        // Déduction du type
        if (so is SpellForm) Type = UpgradeType.NewSpell;
        else if (so is SpellEffect) Type = UpgradeType.Effect;
        else if (so is SpellModifier) Type = UpgradeType.Modifier;
        else if (so is StatUpgradeSO) Type = UpgradeType.StatBoost;
    }

    // Méthode pour changer le type (utilisée par LevelUpUI si on possède déjà le sort)
    public void SetAsUpgrade()
    {
        if (Type == UpgradeType.NewSpell)
        {
            Type = UpgradeType.SpellUpgrade;
            Name = $"{TargetRuneSO.runeName} (Upgrade)";
        }
    }
}