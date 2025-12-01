using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Modifier")]
public class SpellModifier : RuneSO
{
    public override RuneType Type => RuneType.Modifier;

    [Header("Restriction")]
    public SpellTag requiredTag;

    [Header("Stats de Base (Niveau 1)")]
    // Pour un Modifier, les stats de base sont définies directement via RuneStats
    public RuneStats baseStats;

    [Header("Comportement")]
    public bool enableHoming;
}