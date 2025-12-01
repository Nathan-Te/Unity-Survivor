using UnityEngine;

// Structure universelle pour les stats (Base + Upgrades)
[System.Serializable]
public struct RuneStats
{
    [Header("Multiplicateurs (%)")]
    public float DamageMult;   // 0.1 = +10%
    public float CooldownMult; // -0.1 = -10%
    public float SizeMult;
    public float SpeedMult;
    public float DurationMult;

    [Header("Additions (Flat)")]
    public int FlatCount;      // +1 Projectile
    public int FlatPierce;     // +1 Pierce
    public float FlatSpread;   // +15°
    public float FlatRange;    // +5m
    public float FlatKnockback; // +2 Force

    [Header("Spécial")]
    public int FlatChainCount;

    // Constructeur vide
    public static RuneStats Zero => new RuneStats();

    // Opérateur + pour additionner facilement deux stats
    public static RuneStats operator +(RuneStats a, RuneStats b)
    {
        RuneStats c = new RuneStats();
        c.DamageMult = a.DamageMult + b.DamageMult;
        c.CooldownMult = a.CooldownMult + b.CooldownMult;
        c.SizeMult = a.SizeMult + b.SizeMult;
        c.SpeedMult = a.SpeedMult + b.SpeedMult;
        c.DurationMult = a.DurationMult + b.DurationMult;

        c.FlatCount = a.FlatCount + b.FlatCount;
        c.FlatPierce = a.FlatPierce + b.FlatPierce;
        c.FlatSpread = a.FlatSpread + b.FlatSpread;
        c.FlatRange = a.FlatRange + b.FlatRange;
        c.FlatKnockback = a.FlatKnockback + b.FlatKnockback;
        c.FlatChainCount = a.FlatChainCount + b.FlatChainCount;
        return c;
    }
}

[System.Serializable]
public class RuneDefinition
{
    [TextArea] public string Description; // Ex: "+1 Projectile et +10% Dégâts"
    public RuneStats Stats;
}