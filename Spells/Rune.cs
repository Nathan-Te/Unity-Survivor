using UnityEngine;

[System.Serializable]
public class Rune
{
    public RuneSO Data; // La donnée statique (ScriptableObject)
    public int Level;   // La donnée dynamique

    // Constructeur
    public Rune(RuneSO data, int level = 1)
    {
        Data = data;
        Level = level;
    }

    public void LevelUp()
    {
        Level++;
    }

    // Helpers de Cast pour récupérer le bon type facilement
    public SpellForm AsForm => Data as SpellForm;
    public SpellEffect AsEffect => Data as SpellEffect;
    public SpellModifier AsModifier => Data as SpellModifier;
}