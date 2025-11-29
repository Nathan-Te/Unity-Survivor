using UnityEngine;

public enum UpgradeType { NewSpell, StatBoost, Modifier }

[System.Serializable]
public class UpgradeData
{
    public string Name;
    public string Description;
    public Sprite Icon;
    public UpgradeType Type;

    // Références (L'un de ces champs sera rempli selon le type)
    public SpellForm TargetForm;       // Pour un nouveau sort
    public SpellModifier TargetModifier; // Pour modifier un sort existant
    // public StatUpgrade TargetStat; // Pour plus tard (Vitesse, MaxHP...)

    // Constructeurs rapides
    public UpgradeData(SpellForm form)
    {
        Type = UpgradeType.NewSpell;
        Name = $"Nouveau : {form.formName}";
        Description = form.description;
        Icon = form.icon;
        TargetForm = form;
    }

    public UpgradeData(SpellModifier mod)
    {
        Type = UpgradeType.Modifier;
        Name = $"Module : {mod.modifierName}";
        Description = mod.description;
        // Icon = mod.icon; // Ajoute un sprite à SpellModifier si tu veux
        TargetModifier = mod;
    }
}