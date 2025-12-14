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

    public void ClearAll()
    {
        foreach (var kvp in _pools)
        {
            while (kvp.Value.Count > 0)
            {
                GameObject obj = kvp.Value.Dequeue();
                if (obj != null) Destroy(obj);
            }
        }
        _pools.Clear();
        _activeProjectiles.Clear();

        Debug.Log("[ProjectilePool] Pool vid�");
    }
}