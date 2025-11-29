using UnityEngine;

public enum UpgradeType { NewSpell, StatBoost, Modifier }

[System.Serializable]
public class UpgradeData
{
    public string Name;
    public string Description;
    public Sprite Icon;
    public UpgradeType Type;

    public SpellForm TargetForm;
    public SpellModifier TargetModifier;

    // Constructeur pour un nouveau sort
    public UpgradeData(SpellForm form)
    {
        Type = UpgradeType.NewSpell;
        // CORRECTION : 'formName' devient 'runeName' (hérité de RuneSO)
        Name = $"Nouveau : {form.runeName}";
        Description = form.description;
        Icon = form.icon;
        TargetForm = form;
    }

    public UpgradeData(SpellModifier mod)
    {
        Type = UpgradeType.Modifier;
        Name = $"Module : {mod.runeName}"; // Assure-toi que c'est runeName (hérité de RuneSO)
        Description = mod.description;

        // CORRECTION : On assigne l'icône ici
        Icon = mod.icon;

        TargetModifier = mod;
    }
}