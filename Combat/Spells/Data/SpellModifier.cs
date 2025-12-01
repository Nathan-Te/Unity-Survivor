using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Modifier")]
public class SpellModifier : RuneSO
{
    public override RuneType Type => RuneType.Modifier;

    [Header("Restriction")]
    public SpellTag requiredTag;

    [Header("Stats de Base (Niveau 1)")]
    // Ici on utilise directement RuneStats car un modificateur EST un paquet de stats
    public RuneStats BaseStats;

    [Header("Comportement")]
    public bool enableHoming;
}