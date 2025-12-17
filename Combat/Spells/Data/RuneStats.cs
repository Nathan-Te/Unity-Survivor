using UnityEngine;

// Conteneur de toutes les modifications possibles
[System.Serializable]
public struct RuneStats
{
    [Header("Multiplicateurs (0.1 = +10%)")]
    public float DamageMult;
    public float CooldownMult; // N�gatif pour r�duire (-0.1)
    public float SizeMult;
    public float SpeedMult;
    public float DurationMult;

    [Header("Additions (Valeurs fixes)")]
    public int FlatCount;
    public int FlatPierce;
    public float FlatSpread;
    public float FlatRange;
    public float FlatKnockback;
    public int FlatChainCount;

    [Header("Effets de statut")]
    public float FlatBurnDamage; // Dégâts par tick de Burn
    public float FlatBurnDuration; // Durée du Burn en secondes

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

        c.FlatCount = a.FlatCount + b.FlatCount;
        c.FlatPierce = a.FlatPierce + b.FlatPierce;
        c.FlatSpread = a.FlatSpread + b.FlatSpread;
        c.FlatRange = a.FlatRange + b.FlatRange;
        c.FlatKnockback = a.FlatKnockback + b.FlatKnockback;
        c.FlatChainCount = a.FlatChainCount + b.FlatChainCount;

        c.FlatBurnDamage = a.FlatBurnDamage + b.FlatBurnDamage;
        c.FlatBurnDuration = a.FlatBurnDuration + b.FlatBurnDuration;

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