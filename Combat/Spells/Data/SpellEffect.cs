using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Effect")]
public class SpellEffect : RuneSO
{
    public override RuneType Type => RuneType.Effect;

    [Header("�l�ment")]
    public ElementType element;
    public Color tintColor = Color.white;

    [Header("Stats de Base (Niveau 1)")]
    public float baseDamage = 10f;
    public float baseKnockback = 0f;

    // Note: damageMultiplier de base est souvent 1.0 (100%)
    public float baseDamageMultiplier = 1.0f;

    [Header("Sp�cial")]
    public bool applyBurn;
    public float burnDamagePerTick = 2f;
    public float burnDuration = 3f;

    public bool applySlow;
    [Tooltip("Speed reduction factor (0.5 = 50% slower, 0.9 = 90% slower)")]
    public float slowFactor = 0.5f;
    public float slowDuration = 2f;

    public int baseChainCount = 0;
    public float chainRange = 8f;
    [Tooltip("Bonus damage per chain jump (0.1 = +10% damage per bounce, applies to each bounce independently)")]
    public float chainDamageBonus = 0.1f;

    [Range(0f, 1f)] public float minionSpawnChance = 0f;
    public GameObject minionPrefab;
    [Tooltip("Base movement speed of spawned minions (units/sec)")]
    public float minionBaseSpeed = 5f;
    [Tooltip("Base explosion radius of spawned minions")]
    public float minionBaseExplosionRadius = 3f;
    [Tooltip("Base explosion damage of spawned minions")]
    public float minionBaseExplosionDamage = 10f;

    public float aoeRadius = 0f;
}