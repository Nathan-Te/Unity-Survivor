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
    public int MulticastCount; // Number of times to repeat the cast (1 = cast twice total)
    public float MulticastDelay; // Delay in seconds between each cast (default: 0.1s)

    public TargetingMode Mode;
    public bool RequiresLoS;
    public bool IsHoming;

    // Stats Sp�ciales
    public int ChainCount;
    public float ChainRange;
    public float ChainDamageBonus; // Bonus damage per chain jump (0.1 = +10% per bounce)

    public float MinionChance;
    public GameObject MinionPrefab;
    public float MinionSpeed; // Calculated ghost speed (base + upgrades)
    public float MinionExplosionRadius; // Calculated explosion radius (base + upgrades)
    public float MinionExplosionDamage; // Calculated explosion damage (base * multiplier)
    public float MinionCritChance; // Crit chance for ghost explosions (inherited from spell)
    public float MinionCritDamageMultiplier; // Crit damage multiplier for ghost explosions (inherited from spell)

    // Burn stats (calculated from SpellEffect + bonuses)
    public float BurnDamagePerTick;
    public float BurnDuration;

    // Slow stats (calculated from SpellEffect + bonuses)
    public float SlowFactor; // Speed reduction factor (0.5 = 50% slower)
    public float SlowDuration; // Slow duration in seconds

    // Vulnerability stats (calculated from rune bonuses)
    public float VulnerabilityDamage; // Bonus damage multiplier against slowed enemies (0.1 = +10%)

    // Critical hit stats (calculated from global player stats + rune bonuses)
    public float CritChance; // 0-1 range (0.25 = 25% crit chance)
    public float CritDamageMultiplier; // 1.5 = 150% damage on crit

    public SpellDefinition() { }
}