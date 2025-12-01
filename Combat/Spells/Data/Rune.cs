using UnityEngine;

[System.Serializable]
public class Rune
{
    public RuneSO Data;
    public int Level;

    // Somme de toutes les améliorations reçues
    public RuneStats AccumulatedStats;

    // Constructeur simple (Corrige l'erreur CS1729)
    public Rune(RuneSO data)
    {
        Data = data;
        Level = 1;
        AccumulatedStats = RuneStats.Zero;

        // Cas particulier : Pour un Modifier, on applique ses stats de base dès le niveau 1
        if (data is SpellModifier mod)
        {
            AccumulatedStats = mod.BaseStats;
        }
    }

    // Appelé quand on choisit une carte d'amélioration
    public void ApplyUpgrade(RuneDefinition upgradeDef)
    {
        Level++;
        // On additionne les stats de l'upgrade aux stats actuelles
        AccumulatedStats += upgradeDef.Stats;
    }

    // Helpers
    public SpellForm AsForm => Data as SpellForm;
    public SpellEffect AsEffect => Data as SpellEffect;
    public SpellModifier AsModifier => Data as SpellModifier;
}