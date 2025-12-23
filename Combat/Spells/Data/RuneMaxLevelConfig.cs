using UnityEngine;

/// <summary>
/// Global configuration for max levels per rune type.
/// Create one instance via Assets > Create > Config > Rune Max Level Config
/// </summary>
[CreateAssetMenu(menuName = "Config/Rune Max Level Config", fileName = "RuneMaxLevelConfig")]
public class RuneMaxLevelConfig : ScriptableObject
{
    [Header("Max Levels by Rune Type")]
    [Tooltip("Maximum level for all Form runes (0 = unlimited)")]
    public int formMaxLevel = 0;

    [Tooltip("Maximum level for all Effect runes (0 = unlimited)")]
    public int effectMaxLevel = 0;

    [Tooltip("Maximum level for all Modifier runes (0 = unlimited)")]
    public int modifierMaxLevel = 0;

    [Header("Stat Upgrade Limits")]
    [Tooltip("Maximum number of DIFFERENT stat types the player can acquire (0 = unlimited)")]
    public int maxStatTypes = 0;

    [Header("Build Lock System")]
    [Tooltip("When enabled, once the player's build is complete (all slots filled), only allow upgrades of owned runes")]
    public bool lockBuildWhenFull = true;

    /// <summary>
    /// Gets the max level for a specific rune type
    /// </summary>
    public int GetMaxLevel(RuneType type)
    {
        switch (type)
        {
            case RuneType.Form:
                return formMaxLevel;
            case RuneType.Effect:
                return effectMaxLevel;
            case RuneType.Modifier:
                return modifierMaxLevel;
            default:
                return 0;
        }
    }

    /// <summary>
    /// Checks if a rune at the given level has reached its max level
    /// </summary>
    public bool IsMaxLevel(RuneType type, int currentLevel)
    {
        int maxLevel = GetMaxLevel(type);
        return maxLevel > 0 && currentLevel >= maxLevel;
    }

    // Singleton-like accessor (optional, for convenience)
    private static RuneMaxLevelConfig _instance;
    public static RuneMaxLevelConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<RuneMaxLevelConfig>("RuneMaxLevelConfig");

                if (_instance == null)
                {
                    Debug.LogWarning("[RuneMaxLevelConfig] No config found in Resources folder. Using unlimited levels.");
                }
            }
            return _instance;
        }
    }
}
