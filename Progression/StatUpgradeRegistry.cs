using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Registry to map StatType to StatUpgradeSO for HUD display
/// </summary>
public class StatUpgradeRegistry : MonoBehaviour
{
    [SerializeField] private List<StatUpgradeSO> allStatUpgrades;

    private static StatUpgradeRegistry _instance;
    private Dictionary<StatType, StatUpgradeSO> _statMap;

    public static StatUpgradeRegistry Instance => _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        BuildStatMap();
    }

    private void BuildStatMap()
    {
        _statMap = new Dictionary<StatType, StatUpgradeSO>();

        if (allStatUpgrades == null)
            return;

        foreach (var statSO in allStatUpgrades)
        {
            if (statSO != null)
            {
                _statMap[statSO.targetStat] = statSO;
            }
        }
    }

    /// <summary>
    /// Gets the StatUpgradeSO for a given StatType
    /// </summary>
    public StatUpgradeSO GetStatUpgrade(StatType statType)
    {
        if (_statMap != null && _statMap.TryGetValue(statType, out var statSO))
        {
            return statSO;
        }
        return null;
    }

    /// <summary>
    /// Initializes the registry with a list of stat upgrades (called by LevelUpUI)
    /// </summary>
    public void Initialize(List<StatUpgradeSO> stats)
    {
        allStatUpgrades = stats;
        BuildStatMap();
    }
}
