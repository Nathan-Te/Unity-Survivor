using UnityEngine;

// Conteneur de toutes les modifications possibles
[System.Serializable]
public struct RuneStats
{
    [Header("Multiplicateurs (0.1 = +10%)")]
    public float DamageMult;
    public float CooldownMult; // Positif = attaques plus rapides (-10% cooldown), N�gatif = plus lent
    public float SizeMult;
    public float SpeedMult;
    public float DurationMult;

    [Header("Additions (Valeurs fixes)")]
    public float FlatCooldown; // Override/modify base cooldown (useful for Form runes)
    public int FlatCount;
    public int FlatPierce;
    public float FlatSpread;
    public float FlatRange;
    public float FlatKnockback;
    public int FlatChainCount;
    public int FlatMulticast; // Number of additional casts (1 = cast twice total)

    [Header("Effets de statut")]
    public float FlatBurnDamage; // Dégâts par tick de Burn
    public float FlatBurnDuration; // Durée du Burn en secondes

    [Header("Coups critiques")]
    public float FlatCritChance; // Chance de critique en % (0.05 = +5%)
    public float FlatCritDamage; // Multiplicateur de dégâts critiques (0.5 = +50%)

    [Header("Sp�cial (Pour StatUpgrade)")]
    public float StatValue; // Valeur g�n�rique pour MoveSpeed, MaxHealth, etc.

    // Constructeur vide
    public static RuneStats Zero => new RuneStats();

    // Permet d'additionner deux stats facilement (A + B)
    public static RuneStats operator +(RuneStats a, RuneStats b)
    {
        RuneStats c = new RuneStats();
        c.DamageMult = a.DamageMult + b.DamageMult;
        c.CooldownMult = a.CooldownMult + b.CooldownMult;
        c.SizeMult = a.SizeMult + b.SizeMult;
        c.SpeedMult = a.SpeedMult + b.SpeedMult;
        c.DurationMult = a.DurationMult + b.DurationMult;

        c.FlatCooldown = a.FlatCooldown + b.FlatCooldown;
        c.FlatCount = a.FlatCount + b.FlatCount;
        c.FlatPierce = a.FlatPierce + b.FlatPierce;
        c.FlatSpread = a.FlatSpread + b.FlatSpread;
        c.FlatRange = a.FlatRange + b.FlatRange;
        c.FlatKnockback = a.FlatKnockback + b.FlatKnockback;
        c.FlatChainCount = a.FlatChainCount + b.FlatChainCount;
        c.FlatMulticast = a.FlatMulticast + b.FlatMulticast;

        c.FlatBurnDamage = a.FlatBurnDamage + b.FlatBurnDamage;
        c.FlatBurnDuration = a.FlatBurnDuration + b.FlatBurnDuration;

        c.FlatCritChance = a.FlatCritChance + b.FlatCritChance;
        c.FlatCritDamage = a.FlatCritDamage + b.FlatCritDamage;

        c.StatValue = a.StatValue + b.StatValue;
        return c;
    }
}

// Ce que tu configures dans l'inspecteur pour chaque raret�
[System.Serializable]
public class RuneDefinition
{
    [TextArea] public string Description; // Ex: "+2 Projectiles"
    public RuneStats Stats;
}