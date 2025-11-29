using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Effect")]
public class SpellEffect : RuneSO
{
    public override RuneType Type => RuneType.Effect;

    // ... (Tous tes champs existants) ...
    [Header("Identité")]
    public string effectName;
    public ElementType element;
    public Color tintColor = Color.white;

    [Header("Stats de Base")]
    public float baseDamage = 10f;
    public float damageMultiplier = 1.0f;
    public float knockbackForce = 0f;

    [Header("Croissance")]
    public float damageGrowth = 2f;
    public float multiplierGrowth = 0.1f;

    // ... (Status, Chain, Necro, Zone...) ...
    [Header("Status / Spécial")]
    public bool applyBurn;
    public bool applySlow;
    public bool chainLightning;
    public bool spawnMinionOnDeath;

    [Header("Foudre (Chain)")]
    public int baseChainCount = 0;
    public float chainRange = 8f;
    public float chainDamageReduction = 0.7f;

    [Header("Nécrotique")]
    [Range(0f, 1f)] public float minionSpawnChance = 0f;
    public GameObject minionPrefab;

    [Header("Zone")]
    public float aoeRadius = 0f;

    // IMPLEMENTATION
    public override string GetLevelUpDescription(int level)
    {
        float dmg = baseDamage + (damageGrowth * (level - 1));
        // float mult = damageMultiplier + (multiplierGrowth * (level - 1)); // Si tu veux afficher le mult aussi

        return $"Dégâts de base : {dmg:F1}";
    }
}