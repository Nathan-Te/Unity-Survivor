using System.Collections.Generic;
using UnityEngine;

public class MapObjectPool : Singleton<MapObjectPool>
{
    // Dictionnaire : ID du Prefab -> File d'attente d'objets inactifs
    private Dictionary<int, Queue<GameObject>> _pools = new Dictionary<int, Queue<GameObject>>();

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
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
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.transform.SetParent(parent); // On le remet dans le bon Chunk
            obj.SetActive(true);
        }
        else
        {
            // Si la r�serve est vide, on cr�e du neuf
            obj = Instantiate(prefab, position, rotation, parent);
        }

        return obj;
    }

    public void ReturnToPool(GameObject obj, GameObject originalPrefab)
    {
        if (obj == null) return;

        obj.SetActive(false);
        obj.transform.SetParent(transform); // On le range sous le Manager pour pas polluer la hi�rarchie

        int key = originalPrefab.GetInstanceID();
        if (!_pools.ContainsKey(key))
        {
            _pools.Add(key, new Queue<GameObject>());
        }

        _pools[key].Enqueue(obj);
    }

    /// <summary>
    /// Deactivates all map objects (children of this pool).
    /// Called during scene transitions to clean up before reload.
    /// Does NOT destroy objects - they'll be destroyed with scene reload.
    /// </summary>
    public void ClearAll()
    {
        // Deactivate all children (active map objects)
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

        Debug.Log($"[MapObjectPool] Deactivated {deactivatedCount} map objects. Pool has {_pools.Count} prefab types.");
    }
}