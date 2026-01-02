using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Object pool for VFX effects (impacts, explosions, etc).
/// Auto-returns VFX to pool after their duration expires.
/// </summary>
public class VFXPool : Singleton<VFXPool>
{
    // Dictionary: Prefab ID -> Queue of pooled VFX objects
    private Dictionary<int, Queue<GameObject>> _pools = new Dictionary<int, Queue<GameObject>>();

    // Track active VFX with their lifetime
    private List<VFXInstance> _activeVfx = new List<VFXInstance>();

    private class VFXInstance
    {
        public GameObject GameObject;
        public GameObject OriginalPrefab;
        public float TimeRemaining;
    }

    protected override void OnDestroy()
    {
        _activeVfx.Clear();
        _pools.Clear();

        base.OnDestroy();
    }

    private void Update()
    {
        // SAFETY: Stop executing if scene is restarting/loading
        if (SingletonGlobalState.IsSceneLoading || SingletonGlobalState.IsApplicationQuitting)
            return;

        float dt = Time.deltaTime;

        // Update all active VFX lifetimes
        for (int i = _activeVfx.Count - 1; i >= 0; i--)
        {
            var vfx = _activeVfx[i];
            vfx.TimeRemaining -= dt;

            if (vfx.TimeRemaining <= 0f)
            {
                // Return to pool
                ReturnToPool(vfx.GameObject, vfx.OriginalPrefab);
                _activeVfx.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Spawns a VFX effect at the specified position with auto-cleanup after duration
    /// </summary>
    /// <param name="prefab">VFX prefab to spawn</param>
    /// <param name="position">World position</param>
    /// <param name="rotation">World rotation</param>
    /// <param name="duration">Lifetime in seconds before auto-cleanup</param>
    /// <param name="scale">Uniform scale multiplier (default 1.0)</param>
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, float duration = 2f, float scale = 1f)
    {
        if (prefab == null)
        {
            Debug.LogWarning("[VFXPool] Attempted to spawn VFX with null prefab!");
            return null;
        }

        int key = prefab.GetInstanceID();
        if (!_pools.ContainsKey(key))
            _pools.Add(key, new Queue<GameObject>());

        GameObject obj;
        if (_pools[key].Count > 0)
        {
            obj = _pools[key].Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.transform.localScale = Vector3.one * scale;
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab, position, rotation, transform);
            obj.transform.localScale = Vector3.one * scale;
        }

        // Reset particle systems if present
        ParticleSystem[] particles = obj.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particles)
        {
            ps.Clear();
            ps.Play();
        }

        // Track this VFX for auto-cleanup
        _activeVfx.Add(new VFXInstance
        {
            GameObject = obj,
            OriginalPrefab = prefab,
            TimeRemaining = duration
        });

        return obj;
    }

    /// <summary>
    /// Manually returns a VFX to the pool (usually called automatically)
    /// </summary>
    public void ReturnToPool(GameObject obj, GameObject originalPrefab)
    {
        if (obj == null || originalPrefab == null)
            return;

        obj.SetActive(false);

        int key = originalPrefab.GetInstanceID();
        if (!_pools.ContainsKey(key))
            _pools.Add(key, new Queue<GameObject>());

        _pools[key].Enqueue(obj);
    }

    /// <summary>
    /// Deactivates all active VFX.
    /// Called during scene transitions to clean up before reload.
    /// Does NOT destroy objects - they'll be destroyed with scene reload.
    /// </summary>
    public void ClearAll()
    {
        // Deactivate all active VFX
        int deactivatedCount = 0;
        foreach (var vfx in _activeVfx)
        {
            if (vfx.GameObject != null && vfx.GameObject.activeSelf)
            {
                vfx.GameObject.SetActive(false);
                deactivatedCount++;
            }
        }

        // Clear the active list
        _activeVfx.Clear();

        Debug.Log($"[VFXPool] Deactivated {deactivatedCount} active VFX. Pool has {_pools.Count} prefab types.");
    }
}
