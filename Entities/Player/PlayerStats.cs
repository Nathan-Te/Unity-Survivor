using UnityEngine;

public class PlayerStats : Singleton<PlayerStats>
{
    [Header("Global Combat Stats (Multiplicateurs)")]
    public float Might = 1.0f;          // D�g�ts
    public float CooldownSpeed = 1.0f;  // Vitesse recharge
    public float AreaSize = 1.0f;       // Taille
    public float ProjectileSpeed = 1.0f;
    public int AdditionalAmount = 0;    // +1 Projectile

    [Header("Critical Hit Stats")]
    public float CritChance = 0.0f;     // Chance de coup critique (0-1, ex: 0.15 = 15%)
    public float CritDamage = 1.5f;     // Multiplicateur de d�g�ts critiques (1.5 = 150%)

    [Header("Utility Stats")]
    public float ExperienceMultiplier = 1.0f;

    private PlayerController _controller;
    private PlayerCollector _collector;

    protected override void Awake()
    {
        base.Awake();

        if (Instance == this)
        {
            _controller = GetComponent<PlayerController>();
            _collector = GetComponent<PlayerCollector>();
        }
    }

    public void ApplyUpgrade(StatType type, float value)
    {
        Debug.Log($"[PlayerStats] ApplyUpgrade called - Type: {type}, Value: {value}, Controller exists: {_controller != null}");

        switch (type)
        {
            // SURVIE
            case StatType.MoveSpeed:
                Debug.Log($"[PlayerStats] Applying MoveSpeed: {value}");
                if (_controller) _controller.ModifySpeed(value);
                break;
            case StatType.MaxHealth:
                if (_controller) _controller.ModifyMaxHealth(value);
                break;
            case StatType.HealthRegen:
                if (_controller) _controller.ModifyRegen(value);
                break;
            case StatType.Armor:
                if (_controller) _controller.ModifyArmor(value);
                break;

            // UTILITAIRE
            case StatType.MagnetArea:
                if (_collector) _collector.magnetRadius += value;
                break;
            case StatType.ExperienceGain:
                ExperienceMultiplier += value;
                break;

            // COMBAT GLOBAL
            case StatType.GlobalDamage: Might += value; break;
            case StatType.GlobalCooldown: CooldownSpeed += value; break;
            case StatType.GlobalArea: AreaSize += value; break;
            case StatType.GlobalSpeed: ProjectileSpeed += value; break;
            case StatType.GlobalCount: AdditionalAmount += (int)value; break;

            // CRITICAL HIT
            case StatType.CritChance: CritChance += value; break;
            case StatType.CritDamage: CritDamage += value; break;
        }

        Debug.Log($"Stat Applied: {type} += {value}");

        // Recalculate all spell stats when global stats change
        RecalculateAllSpells();
    }

    private void RecalculateAllSpells()
    {
        // Find SpellManager and recalculate all active spells
        var spellManager = FindFirstObjectByType<SpellManager>();
        if (spellManager != null)
        {
            spellManager.RecalculateAllSpells();
        }
    }
}