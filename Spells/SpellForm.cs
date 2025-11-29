using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Form")]
public class SpellForm : RuneSO // <-- Hérite de RuneSO
{
    public override RuneType Type => RuneType.Form;

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
    
    // --- NOUVEAU : CROISSANCE ---
    [Header("Croissance (Par Niveau)")]
    public float cooldownReductionPerLevel = 0.05f; // -0.05s par niveau
    public int countIncreaseEveryXLevels = 5;       // +1 projectile tous les 5 niveaux

    [Header("Mouvement")]
    public float baseSpeed = 20f;
    public float impactDelay = 0f;
    public float baseDuration = 5f;
    [Range(0f, 1f)] public float procCoefficient = 1.0f;

    public override string GetLevelUpDescription(int nextLevel)
    {
        return $"Cooldown réduit de {cooldownReductionPerLevel * (nextLevel - 1):F2}s total.";
    }
}