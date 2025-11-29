using UnityEngine;

public enum UpgradeType { NewSpell, StatBoost, Modifier, Effect }

[System.Serializable]
public class UpgradeData
{
    public string Name;
    public string Description;
    public Sprite Icon;
    public UpgradeType Type;
    public int Level = 1;

    public SpellForm TargetForm;
    public SpellModifier TargetModifier;
    public SpellEffect TargetEffect;

    public UpgradeData(SpellForm form)
    {
        Type = UpgradeType.NewSpell;
        Name = $"Nouveau : {form.runeName}";
        Description = form.description;
        Icon = form.icon;
        TargetForm = form;
    }

    public UpgradeData(SpellEffect effect)
    {
        Type = UpgradeType.Effect;
        Name = $"Effet : {effect.runeName}";
        Description = effect.description;
        Icon = effect.icon; // <--- DÉCOMMENTÉ (Assure-toi d'avoir mis une icône dans l'Asset !)
        TargetEffect = effect;
    }

    public UpgradeData(SpellModifier mod)
    {
        Type = UpgradeType.Modifier;
        Name = $"Module : {mod.runeName}";
        Description = mod.description;
        Icon = mod.icon; // <--- DÉCOMMENTÉ (Même chose ici)
        TargetModifier = mod;
    }
}