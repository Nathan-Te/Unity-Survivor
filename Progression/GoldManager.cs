using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages player's gold currency across the game session.
/// DontDestroyOnLoad singleton that persists across scene reloads.
/// </summary>
public class GoldManager : Singleton<GoldManager>
{
    [Header("État")]
    [SerializeField] private int _currentSessionGold = 0; // Gold collected during current run
    [SerializeField] private int _totalGold = 0; // Persistent gold (used for meta-progression)

    public int CurrentSessionGold => _currentSessionGold;
    public int TotalGold => _totalGold;

    // Events
    public UnityEvent<int> OnSessionGoldChanged;
    public UnityEvent<int> OnTotalGoldChanged;

    protected override void Awake()
    {
        base.Awake();

        // Mark as persistent across scenes
        if (Instance == this)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// Adds gold to the current session (in-game pickup).
    /// </summary>
    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        _currentSessionGold += amount;
        OnSessionGoldChanged?.Invoke(_currentSessionGold);

        Debug.Log($"[GoldManager] +{amount} gold. Session total: {_currentSessionGold}");
    }

    /// <summary>
    /// Called at the end of a run to transfer session gold to persistent total.
    /// </summary>
    public void ConvertSessionGoldToTotal()
    {
        if (_currentSessionGold > 0)
        {
            _totalGold += _currentSessionGold;
            OnTotalGoldChanged?.Invoke(_totalGold);

            Debug.Log($"[GoldManager] Session ended. Converted {_currentSessionGold} gold to total. New total: {_totalGold}");

            _currentSessionGold = 0;
            OnSessionGoldChanged?.Invoke(_currentSessionGold);
        }
    }

    /// <summary>
    /// Spends gold from the persistent total (meta-progression upgrades).
    /// Returns true if successful, false if not enough gold.
    /// </summary>
    public bool SpendGold(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("[GoldManager] Cannot spend negative or zero gold.");
            return false;
        }

        if (_totalGold >= amount)
        {
            _totalGold -= amount;
            OnTotalGoldChanged?.Invoke(_totalGold);
            Debug.Log($"[GoldManager] Spent {amount} gold. Remaining: {_totalGold}");
            return true;
        }
        else
        {
            Debug.LogWarning($"[GoldManager] Not enough gold. Need {amount}, have {_totalGold}");
            return false;
        }
    }

    /// <summary>
    /// Resets session gold (called on new game start).
    /// </summary>
    public void ResetSession()
    {
        _currentSessionGold = 0;
        OnSessionGoldChanged?.Invoke(_currentSessionGold);
        Debug.Log("[GoldManager] Session gold reset.");
    }

    /// <summary>
    /// CHEAT: Add gold directly to total (for testing meta-progression).
    /// </summary>
    public void CheatAddTotalGold(int amount)
    {
        _totalGold += amount;
        OnTotalGoldChanged?.Invoke(_totalGold);
        Debug.Log($"[GoldManager] CHEAT: Added {amount} gold to total. New total: {_totalGold}");
    }

    /// <summary>
    /// Save/Load support (to be integrated with save system later).
    /// </summary>
    public void LoadGold(int totalGold)
    {
        _totalGold = totalGold;
        OnTotalGoldChanged?.Invoke(_totalGold);
    }

    public int GetSaveData()
    {
        return _totalGold;
    }
}
