using UnityEngine;
using SurvivorGame.Localization;

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
    public float FlatChainDamageBonus; // Bonus damage per chain jump (0.1 = +10% per bounce)
    public int FlatMulticast; // Number of additional casts (1 = cast twice total)

    [Header("Effets de statut")]
    public float FlatBurnDamage; // Dégâts par tick de Burn
    public float FlatBurnDuration; // Durée du Burn en secondes
    public float FlatSlowFactor; // Facteur de ralentissement (0.1 = +10% de ralentissement, max 0.9 = 90%)
    public float FlatSlowDuration; // Durée du ralentissement en secondes
    public float FlatVulnerabilityDamage; // Dégâts supplémentaires subis par les ennemis ralentis (0.1 = +10%)

    [Header("Coups critiques")]
    public float FlatCritChance; // Chance de critique en % (0.05 = +5%)
    public float FlatCritDamage; // Multiplicateur de dégâts critiques (0.5 = +50%)

    [Header("Minions (Necrotic)")]
    public float FlatMinionChance; // Chance d'invoquer un minion à la mort (0.1 = +10%)
    public float FlatMinionSpeed; // Vitesse de déplacement des minions (+1 = +1 unité/sec)
    public float FlatMinionExplosionRadius; // Rayon d'explosion des minions (+1 = +1 unité)
    public float MinionDamageMult; // Multiplicateur de dégâts d'explosion (0.2 = +20%)

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
        c.FlatChainDamageBonus = a.FlatChainDamageBonus + b.FlatChainDamageBonus;
        c.FlatMulticast = a.FlatMulticast + b.FlatMulticast;

        c.FlatBurnDamage = a.FlatBurnDamage + b.FlatBurnDamage;
        c.FlatBurnDuration = a.FlatBurnDuration + b.FlatBurnDuration;
        c.FlatSlowFactor = a.FlatSlowFactor + b.FlatSlowFactor;
        c.FlatSlowDuration = a.FlatSlowDuration + b.FlatSlowDuration;
        c.FlatVulnerabilityDamage = a.FlatVulnerabilityDamage + b.FlatVulnerabilityDamage;

        c.FlatCritChance = a.FlatCritChance + b.FlatCritChance;
        c.FlatCritDamage = a.FlatCritDamage + b.FlatCritDamage;

        c.FlatMinionChance = a.FlatMinionChance + b.FlatMinionChance;
        c.FlatMinionSpeed = a.FlatMinionSpeed + b.FlatMinionSpeed;
        c.FlatMinionExplosionRadius = a.FlatMinionExplosionRadius + b.FlatMinionExplosionRadius;
        c.MinionDamageMult = a.MinionDamageMult + b.MinionDamageMult;

        c.StatValue = a.StatValue + b.StatValue;
        return c;
    }
}

// Ce que tu configures dans l'inspecteur pour chaque raret�
[System.Serializable]
public class RuneDefinition
{
    [Tooltip("Localized description for this upgrade. Create via Assets > Create > Localization > Localized String")]
    public LocalizedString Description; // Ex: "+2 Projectiles"
    public RuneStats Stats;
}