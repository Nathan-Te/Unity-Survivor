using UnityEngine;

[System.Serializable]
public class Rune
{
    public RuneSO Data;

    // Le niveau affiché au joueur (1, 2, 3...)
    public int Level;

    // La puissance réelle accumulée (1.0, 2.5, 5.5...)
    // C'est cette valeur qui sera utilisée dans les formules de dégâts.
    public float TotalPower;

    public Rune(RuneSO data, float initialPower = 1.0f)
    {
        Data = data;
        Level = 1;
        TotalPower = initialPower; // Une rune commence généralement avec 1.0 de puissance (base)
    }

    // Appelé quand on choisit une carte d'amélioration
    public void Upgrade(Rarity rarity)
    {
        Level++; // Toujours +1 niveau
        TotalPower += RarityUtils.GetPowerBoost(rarity); // Mais la puissance bondit selon la rareté
    }

    // Helpers
    public SpellForm AsForm => Data as SpellForm;
    public SpellEffect AsEffect => Data as SpellEffect;
    public SpellModifier AsModifier => Data as SpellModifier;
}