using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Modifier")]
public class SpellModifier : RuneSO
{
    public override RuneType Type => RuneType.Modifier;

    // ... (Champs existants) ...
    [Header("Restriction")]
    public SpellTag requiredTag;

    [Header("Multiplicateurs de Stats")]
    public float damageMult = 1f;
    public float cooldownMult = 1f;
    public float sizeMult = 1f;
    public float speedMult = 1f;
    public float durationMult = 1f;

    [Header("Croissance")]
    public float damageMultGrowth = 0.05f;

    [Header("Additions")]
    public int addCount = 0;
    public int addPierce = 0;
    public float addSpread = 0f;
    public bool enableHoming;

    // IMPLEMENTATION
    public override string GetLevelUpDescription(int level)
    {
        // Exemple pour les dégâts
        float growth = damageMultGrowth * (level - 1);
        float totalMult = damageMult + growth;

        // On affiche en pourcentage (x1.5 = +50%)
        return $"Puissance : x{totalMult:F2}";
    }
}