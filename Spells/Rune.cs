using UnityEngine;

[System.Serializable]
public class Rune
{
    public RuneSO Data;
    public int Level;
    // public Rarity Rarity; // SUPPRIMÉ : La rune n'a pas de rareté intrinsèque

    public Rune(RuneSO data, int level = 1)
    {
        Data = data;
        Level = level;
    }

    // NOUVEAU : On ajoute X niveaux d'un coup
    public void IncreaseLevel(int amount)
    {
        Level += amount;
    }

    // Helpers
    public SpellForm AsForm => Data as SpellForm;
    public SpellEffect AsEffect => Data as SpellEffect;
    public SpellModifier AsModifier => Data as SpellModifier;

    // SUPPRIMÉ : PowerMultiplier n'existe plus, c'est le Level qui fait tout.
}