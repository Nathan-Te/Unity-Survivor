using UnityEngine;

[System.Serializable]
public class SpellDefinition
{
    public SpellForm Form;
    public SpellEffect Effect;
    public GameObject Prefab;

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

    public SpellDefinition() { }
}