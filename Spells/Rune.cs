using UnityEngine;

[System.Serializable]
public class Rune
{
    public RuneSO Data;
    public int Level;
    public Rarity Rarity; // NOUVEAU

    // Constructeur mis à jour
    public Rune(RuneSO data, int level = 1, Rarity rarity = Rarity.Common)
    {
        Data = data;
        Level = level;
        Rarity = rarity;
    }

    public void LevelUp()
    {
        Level++;
    }

    // Helpers
    public SpellForm AsForm => Data as SpellForm;
    public SpellEffect AsEffect => Data as SpellEffect;
    public SpellModifier AsModifier => Data as SpellModifier;

    // Helper pour récupérer le multiplicateur actuel
    public float PowerMultiplier => RarityUtils.GetMultiplier(Rarity);
}