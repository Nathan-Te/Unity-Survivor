using UnityEngine;

public enum ElementType { Physical, Fire, Ice, Lightning, Necrotic }

[CreateAssetMenu(menuName = "Spells/Effect")]
public class SpellEffect : RuneSO // <-- Hérite de RuneSO
{
    public override RuneType Type => RuneType.Effect;

    [Header("Élément")]
    public ElementType element;
    public Color tintColor = Color.white;

    [Header("Stats de Base")]
    public float baseDamage = 10f;
    public float damageMultiplier = 1.0f;
    public float knockbackForce = 0f;

    // --- NOUVEAU : CROISSANCE ---
    [Header("Croissance")]
    public float damageGrowth = 2f; // +2 dégâts par niveau
    public float multiplierGrowth = 0.1f; // +10% par niveau

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

    public override string GetLevelUpDescription(int nextLevel, Rarity rarity)
    {
        float mult = RarityUtils.GetMultiplier(rarity);
        float dmg = (baseDamage + (damageGrowth * (nextLevel - 1))) * mult;

        return $"Dégâts : {dmg:F1} <color=grey>(Rareté x{mult})</color>";
    }
}