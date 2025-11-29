using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Modifier")]
public class SpellModifier : RuneSO // <-- Hérite de RuneSO
{
    public override RuneType Type => RuneType.Modifier;

    [Header("Restriction")]
    public SpellTag requiredTag;

    [Header("Multiplicateurs de Stats")]
    public float damageMult = 1f;
    public float cooldownMult = 1f;
    public float sizeMult = 1f;
    public float speedMult = 1f;
    public float durationMult = 1f;

    // --- NOUVEAU : CROISSANCE ---
    [Header("Croissance")]
    public float damageMultGrowth = 0.05f; // +5% dégâts par niveau

    [Header("Additions")]
    public int addCount = 0;
    public int addPierce = 0;
    public bool enableHoming;

    public override string GetLevelUpDescription(int nextLevel)
    {
        return $"Boost Dégâts amélioré de {damageMultGrowth * 100}%";
    }
}