using UnityEngine;

[System.Serializable]
public class SpellDefinition
{
    public SpellForm Form;
    public SpellEffect Effect;
    public GameObject Prefab;
    public GameObject ImpactVfxPrefab; // VFX spawned on hit/impact

    // Smite Timing (Only used for Smite spells)
    public float SmiteImpactDelay;
    public float SmiteVfxSpawnDelay;
    public float SmiteLifetime;

    // Stats
    public float Damage;
    public float Cooldown;
    public float Speed;
    public float Size;
    public float Range;
    public float Duration;
    public float Spread;
    public float Knockback;

    public int Count;
    public int Pierce;

    public TargetingMode Mode;
    public bool RequiresLoS;
    public bool IsHoming;

    // Stats Sp�ciales
    public int ChainCount;
    public float ChainRange;
    public float ChainDamageReduction;

    public float MinionChance;
    public GameObject MinionPrefab;

    // Burn stats (calculated from SpellEffect + bonuses)
    public float BurnDamagePerTick;
    public float BurnDuration;

    // Critical hit stats (calculated from global player stats + rune bonuses)
    public float CritChance; // 0-1 range (0.25 = 25% crit chance)
    public float CritDamageMultiplier; // 1.5 = 150% damage on crit

    public SpellDefinition() { }
}