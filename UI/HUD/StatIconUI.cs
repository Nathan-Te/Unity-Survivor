using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays a single stat icon in the HUD with tooltip support
/// </summary>
public class StatIconUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;

    private StatUpgradeSO _statSO;
    private StatType _statType;
    private StatUpgradeTooltipTrigger _tooltipTrigger;

    /// <summary>
    /// Initializes this stat icon with the given stat type
    /// </summary>
    public void Initialize(StatType statType, StatUpgradeSO statSO)
    {
        _statType = statType;
        _statSO = statSO;

        // Display stat name
        if (nameText != null)
        {
            nameText.text = GetStatDisplayName(statType);
        }

        // Display icon if available
        if (iconImage != null && statSO != null && statSO.icon != null)
        {
            iconImage.sprite = statSO.icon;
            iconImage.enabled = true;
        }
        else if (iconImage != null)
        {
            iconImage.enabled = false;
        }

        // Add tooltip trigger first
        if (statSO != null)
        {
            AddTooltipTrigger();
        }

        // Update display (level and tooltip)
        UpdateDisplay();

        // Subscribe to stat changes for live updates
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnStatsChanged += UpdateDisplay;
        }
    }

    /// <summary>
    /// Updates the level display and tooltip with current stat rune data
    /// </summary>
    private void UpdateDisplay()
    {
        // Display level if we have the stat rune
        if (levelText != null && PlayerStats.Instance != null)
        {
            Rune statRune = PlayerStats.Instance.GetStatRune(_statType);
            if (statRune != null)
            {
                levelText.text = $"Lvl {statRune.Level}";
                levelText.gameObject.SetActive(true);

                // Update tooltip with current rune data
                if (_tooltipTrigger != null)
                {
                    _tooltipTrigger.SetStatRune(statRune);
                }
            }
            else
            {
                levelText.gameObject.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        // Use FindFirstObjectByType to avoid Singleton getter error during scene unload
        var playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.OnStatsChanged -= UpdateDisplay;
        }
    }

    private void AddTooltipTrigger()
    {
        // Add tooltip to the icon if it exists, otherwise to the whole object
        GameObject tooltipTarget = iconImage != null ? iconImage.gameObject : gameObject;

        _tooltipTrigger = tooltipTarget.GetComponent<StatUpgradeTooltipTrigger>();
        if (_tooltipTrigger == null)
        {
            _tooltipTrigger = tooltipTarget.AddComponent<StatUpgradeTooltipTrigger>();
        }
    }

    private string GetStatDisplayName(StatType statType)
    {
        switch (statType)
        {
            // Survival
            case StatType.MoveSpeed: return "Speed";
            case StatType.MaxHealth: return "Health";
            case StatType.HealthRegen: return "Regen";
            case StatType.Armor: return "Armor";

            // Utility
            case StatType.MagnetArea: return "Magnet";
            case StatType.ExperienceGain: return "XP Gain";

            // Combat Global
            case StatType.GlobalDamage: return "Damage";
            case StatType.GlobalCooldown: return "Cooldown";
            case StatType.GlobalArea: return "Area";
            case StatType.GlobalSpeed: return "Proj Speed";
            case StatType.GlobalCount: return "Count";

            // Critical
            case StatType.CritChance: return "Crit %";
            case StatType.CritDamage: return "Crit Dmg";

            default: return statType.ToString();
        }
    }
}
