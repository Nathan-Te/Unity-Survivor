using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : Singleton<ProjectilePool>
{
    // Dictionnaire : Prefab ID -> File d'attente d'objets
    private Dictionary<int, Queue<GameObject>> _pools = new Dictionary<int, Queue<GameObject>>();

    private List<ProjectileController> _activeProjectiles = new List<ProjectileController>();

    protected override void OnDestroy()
    {
        _activeProjectiles.Clear();
        _pools.Clear();

        base.OnDestroy();
    }

    private void Update()
    {
        // SAFETY: Stop executing if scene is restarting/loading
        if (SingletonGlobalState.IsSceneLoading || SingletonGlobalState.IsApplicationQuitting)
            return;

        float dt = Time.deltaTime;
        for (int i = _activeProjectiles.Count - 1; i >= 0; i--)
        {
            if (_activeProjectiles[i] == null || !_activeProjectiles[i].gameObject.activeSelf)
            {
                _activeProjectiles.RemoveAt(i);
                continue;
            }

            _activeProjectiles[i].ManualUpdate(dt);
        }
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            Debug.LogError("[ProjectilePool] Attempted to get a projectile with null prefab!");
            return null;
        }

        int key = prefab.GetInstanceID();
        if (!_pools.ContainsKey(key)) _pools.Add(key, new Queue<GameObject>());

        GameObject obj;
        if (_pools[key].Count > 0)
        {
            obj = _pools[key].Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            // Note: TrailRenderer.Clear() is now handled in ProjectileController.Initialize()
        }
        else
        {
            obj = Instantiate(prefab, position, rotation, transform);
        }

        // ENREGISTREMENT POUR UPDATE
        if (obj.TryGetComponent<ProjectileController>(out var ctrl))
        {
            _activeProjectiles.Add(ctrl);
        }

        return obj;
    }

    public void ReturnToPool(GameObject obj, GameObject originalPrefab)
    {
        // D�SINSCRIPTION DE L'UPDATE
        if (obj.TryGetComponent<ProjectileController>(out var ctrl))
        {
            _activeProjectiles.Remove(ctrl);
        }

        obj.SetActive(false);

        // Null check for originalPrefab
        if (originalPrefab == null)
        {
            Debug.LogError("[ProjectilePool] Attempted to return a projectile with null originalPrefab! Destroying instead.");
            Destroy(obj);
            return;
        }

        int key = originalPrefab.GetInstanceID();
        if (!_pools.ContainsKey(key)) _pools.Add(key, new Queue<GameObject>());
        _pools[key].Enqueue(obj);
    }

    /// <summary>
    /// Despawns all active projectiles that match a specific SpellForm
    /// Useful when replacing a spell to clean up old projectiles (especially Orbits)
    /// </summary>
    public void DespawnProjectilesWithForm(SpellForm form)
    {
        if (form == null) return;

        // Iterate backwards to safely remove items while iterating
        for (int i = _activeProjectiles.Count - 1; i >= 0; i--)
        {
            var projectile = _activeProjectiles[i];
            if (projectile == null) continue;

            // Check if this projectile's definition matches the form we're looking for
            if (projectile.Definition?.Form == form)
            {
                projectile.Despawn();
            }
        }
    }

    /// <summary>
    /// Deactivates all active projectiles and returns them to pool.
    /// Called during scene transitions to clean up before reload.
    /// </summary>
    public void ClearAll()
    {
        // Deactivate all active projectiles (iterate backwards for safe removal)
        for (int i = _activeProjectiles.Count - 1; i >= 0; i--)
        {
            var projectile = _activeProjectiles[i];
            if (projectile != null && projectile.gameObject.activeSelf)
            {
                projectile.gameObject.SetActive(false);
            }
        }

        // Clear the active list
        _activeProjectiles.Clear();

        Debug.Log($"[ProjectilePool] Cleared all active projectiles. Pool has {_pools.Count} prefab types.");
    }
}