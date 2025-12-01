using UnityEngine;

public enum UpgradeType { NewSpell, SpellUpgrade, Modifier, Effect }
// Note: J'ai ajouté SpellUpgrade pour distinguer "Nouveau" de "Améliorer Forme"

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

    // Constructeur
    public UpgradeData(RuneSO so, Rarity rarity)
    {
        TargetRuneSO = so;
        Rarity = rarity;

        // On pioche l'upgrade maintenant !
        UpgradeDefinition = so.GetRandomUpgrade(rarity);

        // Setup UI
        Name = so.runeName;
        Icon = so.icon;
        // La description vient de l'upgrade piochée
        Description = UpgradeDefinition.Description;

        if (so is SpellForm f) { Type = UpgradeType.NewSpell; TargetForm = f; }
        else if (so is SpellEffect e) { Type = UpgradeType.Effect; TargetEffect = e; }
        else if (so is SpellModifier m) { Type = UpgradeType.Modifier; TargetModifier = m; }
    }

    // Méthode pour changer le type (utilisée par LevelUpUI si on possède déjà le sort)
    public void SetAsUpgrade()
    {
        Type = UpgradeType.SpellUpgrade;
        Name = $"{TargetRuneSO.runeName} (Upgrade)";
    }
}