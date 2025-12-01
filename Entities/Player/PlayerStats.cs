using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("Global Combat Stats (Multiplicateurs)")]
    public float Might = 1.0f;          // Dégâts
    public float CooldownSpeed = 1.0f;  // Vitesse recharge
    public float AreaSize = 1.0f;       // Taille
    public float ProjectileSpeed = 1.0f;
    public int AdditionalAmount = 0;    // +1 Projectile

    [Header("Utility Stats")]
    public float ExperienceMultiplier = 1.0f;

    private PlayerController _controller;
    private PlayerCollector _collector;

    private void Awake()
    {
        Instance = this;
        _controller = GetComponent<PlayerController>();
        _collector = GetComponent<PlayerCollector>();
    }

    public void ApplyUpgrade(StatType type, float value)
    {
        switch (type)
        {
            // SURVIE
            case StatType.MoveSpeed:
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
        }

        Debug.Log($"Stat Applied: {type} += {value}");
    }
}