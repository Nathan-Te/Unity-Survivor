using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main coordinator for the ghost minion system.
/// Manages minion lifecycle and enforces max minion limits.
/// Ghost minions use simple straight-line movement (no Job System needed).
/// </summary>
[DefaultExecutionOrder(-50)]
public class MinionManager : Singleton<MinionManager>
{
    [Header("Performance")]
    [SerializeField] private int maxMinionsCapacity = 50;

    // Active minions tracking
    private List<MinionController> _activeMinions = new List<MinionController>();
    private HashSet<MinionController> _minionSet = new HashSet<MinionController>();

    public int ActiveMinionCount => _activeMinions.Count;

    protected override void Awake()
    {
        base.Awake();

        // Always initialize state (even after scene reload)
        _activeMinions.Clear();
        _minionSet.Clear();
    }

    /// <summary>
    /// Spawns a ghost minion at the specified position with upgraded stats
    /// No limit on minion count - allows unlimited chain reactions
    /// </summary>
    public MinionController SpawnMinion(MinionData data, Vector3 position, float speed, float explosionRadius, float explosionDamage, float critChance, float critDamageMultiplier, GameObject spawnerEnemy = null)
    {
        if (data == null || data.prefab == null)
        {
            Debug.LogError("[MinionManager] Invalid MinionData or prefab!");
            return null;
        }

        // Get minion from pool
        GameObject minionObj = MinionPool.Instance.GetMinion(data.prefab, position, Quaternion.identity);
        if (minionObj == null)
        {
            Debug.LogError("[MinionManager] Failed to get minion from pool!");
            return null;
        }

        // Get or add MinionController
        MinionController controller = minionObj.GetComponent<MinionController>();
        if (controller == null)
        {
            controller = minionObj.AddComponent<MinionController>();
        }

        // Initialize controller with data
        controller.SetData(data);

        // Apply upgraded stats from spell bonuses (including crit stats)
        controller.SetUpgradedStats(speed, explosionRadius, explosionDamage, critChance, critDamageMultiplier);

        // Set the spawner enemy to ignore in collision
        if (spawnerEnemy != null)
        {
            controller.SetSpawnerEnemy(spawnerEnemy);
        }

        // Register minion
        RegisterMinion(controller);

        Debug.Log($"[MinionManager] Spawned ghost minion: {data.minionName} (Total: {_activeMinions.Count}) [Speed: {speed}, Radius: {explosionRadius}, Damage: {explosionDamage}, Crit: {critChance * 100f}%/{critDamageMultiplier}x]");

        return controller;
    }

    /// <summary>
    /// Registers a minion to the active pool
    /// </summary>
    public void RegisterMinion(MinionController minion)
    {
        if (minion == null || _minionSet.Contains(minion))
            return;

        _activeMinions.Add(minion);
        _minionSet.Add(minion);
    }

    /// <summary>
    /// Unregisters a minion from the active pool
    /// </summary>
    public void UnregisterMinion(MinionController minion)
    {
        if (minion == null || !_minionSet.Contains(minion))
            return;

        _activeMinions.Remove(minion);
        _minionSet.Remove(minion);
    }

    /// <summary>
    /// Clears all active minions (for scene reload/reset)
    /// </summary>
    public void ClearAllMinions()
    {
        _activeMinions.Clear();
        _minionSet.Clear();

        if (MinionPool.Instance != null)
        {
            MinionPool.Instance.DestroyAll();
        }
    }

    /// <summary>
    /// Gets the current number of active minions
    /// </summary>
    public int GetActiveMinionCount()
    {
        return _activeMinions.Count;
    }

    protected override void OnApplicationQuit()
    {
        ClearAllMinions();
        base.OnApplicationQuit();
    }

    protected override void OnDestroy()
    {
        ClearAllMinions();
        base.OnDestroy();
    }
}
