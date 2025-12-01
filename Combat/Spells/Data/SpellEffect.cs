using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Effect")]
public class SpellEffect : RuneSO
{
    public override RuneType Type => RuneType.Effect;

    [Header("Élément")]
    public ElementType element;
    public Color tintColor = Color.white;

    [Header("Stats de Base (Niveau 1)")]
    public float baseDamage = 10f;
    public float baseKnockback = 0f;

    // Note: damageMultiplier de base est souvent 1.0 (100%)
    public float baseDamageMultiplier = 1.0f;

    [Header("Spécial")]
    public bool applyBurn;
    public bool applySlow;

    public int baseChainCount = 0;
    public float chainRange = 8f;
    public float chainDamageReduction = 0.7f;

    [Range(0f, 1f)] public float minionSpawnChance = 0f;
    public GameObject minionPrefab;

    public float aoeRadius = 0f;
}