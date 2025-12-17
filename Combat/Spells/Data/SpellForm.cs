using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Form")]
public class SpellForm : RuneSO
{
    public override RuneType Type => RuneType.Form;

    [Header("Visuals")]
    public GameObject prefab;

    [Header("Strat�gie")]
    public TargetingMode targetingMode = TargetingMode.Nearest;
    public bool requiresLineOfSight = true;
    public SpellTag tags;

    [Header("Stats de Base (Niveau 1)")]
    public float baseCooldown = 1f;
    public int baseCount = 1;
    public int basePierce = 0;
    public float baseSpread = 0f;
    public float baseSpeed = 20f;
    public float baseDuration = 5f;

    // Range par d�faut si non modifi� par stats
    public float baseRange = 20f;

    [Range(0f, 1f)] public float procCoefficient = 1.0f;
}