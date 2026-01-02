using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Object pooling system for minions to avoid instantiate/destroy overhead
/// </summary>
public class MinionPool : Singleton<MinionPool>
{
    // Dictionary: Prefab ID -> Queue of pooled instances
    private Dictionary<int, Queue<GameObject>> _pools = new Dictionary<int, Queue<GameObject>>();

    [Header("Pool Settings")]
    [SerializeField] private int maxPoolSizePerPrefab = 20;

    [Header("Spawn Pop Effect")]
    [SerializeField] private float spawnPopDuration = 0.3f;
    [SerializeField] private AnimationCurve spawnPopCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    protected override void OnDestroy()
    {
        _pools?.Clear();
        base.OnDestroy();
    }

    /// <summary>
    /// Get a minion from the pool or instantiate a new one
    /// </summary>
    public GameObject GetMinion(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int key = prefab.GetInstanceID();

        if (!_pools.ContainsKey(key))
        {
            _pools.Add(key, new Queue<GameObject>());
        }

        GameObject obj;

        if (_pools[key].Count > 0)
        {
            obj = _pools[key].Dequeue();
            if (obj == null)
            {
                obj = Instantiate(prefab, position, rotation, transform);
            }
            else
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);
            }
        }
        else
        {
            // Instantiate if pool is empty
            obj = Instantiate(prefab, position, rotation, transform);
        }

        // Spawn pop effect
        StartCoroutine(SpawnPopAnimation(obj));

        return obj;
    }

    private IEnumerator SpawnPopAnimation(GameObject minion)
    {
        if (minion == null) yield break;

        Transform minionTransform = minion.transform;
        Vector3 originalScale = minionTransform.localScale;

        // Start at scale 0
        minionTransform.localScale = Vector3.zero;

        float elapsed = 0f;

        while (elapsed < spawnPopDuration)
        {
            if (minion == null || !minion.activeInHierarchy)
            {
                yield break; // Stop if minion is destroyed or deactivated
            }

            elapsed += Time.deltaTime;
            float t = elapsed / spawnPopDuration;

            // Use animation curve for more control
            float curveValue = spawnPopCurve.Evaluate(t);

            minionTransform.localScale = originalScale * curveValue;
            yield return null;
        }

        // Ensure final scale is exact
        if (minion != null && minion.activeInHierarchy)
        {
            minionTransform.localScale = originalScale;
        }
    }

    /// <summary>
    /// Return a minion to the pool for reuse
    /// </summary>
    public void ReturnToPool(GameObject minion, GameObject originalPrefab)
    {
        minion.SetActive(false);
        int key = originalPrefab.GetInstanceID();

        if (!_pools.ContainsKey(key))
        {
            _pools.Add(key, new Queue<GameObject>());
        }

        if (_pools[key].Count >= maxPoolSizePerPrefab)
        {
            Destroy(minion);
            return;
        }

        _pools[key].Enqueue(minion);
    }

    /// <summary>
    /// Deactivates all active minions (children of this pool).
    /// Called during scene transitions to clean up before reload.
    /// Does NOT destroy objects - they'll be destroyed with scene reload.
    /// </summary>
    public void ClearAll()
    {
        // Deactivate all active minions (children of this GameObject)
        int deactivatedCount = 0;
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
                deactivatedCount++;
            }
        }

        Debug.Log($"[MinionPool] Deactivated {deactivatedCount} active minions. Pool has {_pools.Count} prefab types.");
    }

    /// <summary>
    /// Destroy all minions (pooled and active)
    /// </summary>
    public void DestroyAll()
    {
        ClearAll();

        // Destroy all children of this GameObject (active minions)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        Debug.Log("[MinionPool] TOUS les minions détruits (pool + actifs)");
    }
}
