using UnityEngine;

[System.Serializable]
public class Rune
{
    public RuneSO Data;
    public int Level;

    // C'est ici qu'on stocke le cumul de toutes les upgrades choisies
    public RuneStats AccumulatedStats;

    public Rune(RuneSO data)
    {
        Data = data;
        Level = 1;
        AccumulatedStats = RuneStats.Zero;

        // Si c'est un Modifier, il commence avec ses stats de base
        if (data is SpellModifier mod)
        {
            AccumulatedStats = mod.baseStats;
        }
        // Form et Effect n'ont pas de "RuneStats" de base dans ce syst�me, 
        // leurs valeurs de base sont dans les champs float du SO (baseDamage, etc.)
    }

    // Appel� pour une nouvelle rune (Niveau 1, mais avec les stats bonus de la carte)
    public void InitializeWithStats(RuneDefinition upgradeDef)
    {
        // On n'incr�mente PAS le niveau ici
        AccumulatedStats += upgradeDef.Stats;
    }

    // Appel� pour un Level Up
    public void ApplyUpgrade(RuneDefinition upgradeDef)
    {
        Level++;
        AccumulatedStats += upgradeDef.Stats;
    }

    // Helpers
    public SpellForm AsForm => Data as SpellForm;
    public SpellEffect AsEffect => Data as SpellEffect;
    public SpellModifier AsModifier => Data as SpellModifier;
}