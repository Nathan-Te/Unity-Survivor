using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Form")]
public class SpellForm : RuneSO
{
    public override RuneType Type => RuneType.Form;

    // ... (Tout tes champs existants : prefab, targetingMode, stats...) ...
    [Header("Visuals")]
    public GameObject prefab;

    [Header("Stratégie")]
    public TargetingMode targetingMode = TargetingMode.Nearest;
    public bool requiresLineOfSight = true;
    public SpellTag tags;

    [Header("Stats de Base")]
    public float baseCooldown = 1f;
    public int baseCount = 1;
    public int basePierce = 0;
    public float baseSpread = 0f;

    [Header("Croissance")]
    public float cooldownReductionPerLevel = 0.05f;
    public int countIncreaseEveryXLevels = 5;

    [Header("Mouvement")]
    public float baseSpeed = 20f;
    public float impactDelay = 0f;
    public float baseDuration = 5f;
    [Range(0f, 1f)] public float procCoefficient = 1.0f;

    // IMPLEMENTATION
    public override string GetLevelUpDescription(int level)
    {
        // Calcul Cooldown
        float reduction = cooldownReductionPerLevel * (level - 1);
        float finalCd = Mathf.Max(0.1f, baseCooldown - reduction);

        // Calcul Count (tous les X niveaux)
        int extraCount = (level - 1) / Mathf.Max(1, countIncreaseEveryXLevels);
        int finalCount = baseCount + extraCount;

        string desc = $"Cooldown : {finalCd:F2}s";
        if (extraCount > 0) desc += $"\nProjectiles : {finalCount}";

        return desc;
    }
}