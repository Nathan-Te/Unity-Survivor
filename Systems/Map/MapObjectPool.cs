using System.Collections.Generic;
using UnityEngine;

public class MapObjectPool : MonoBehaviour
{
    public static MapObjectPool Instance { get; private set; }

    // Dictionnaire : ID du Prefab -> File d'attente d'objets inactifs
    private Dictionary<int, Queue<GameObject>> _pools = new Dictionary<int, Queue<GameObject>>();

    private void Awake()
    {
        Instance = this;
    }

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
            // Si la réserve est vide, on crée du neuf
            obj = Instantiate(prefab, position, rotation, parent);
        }

        return obj;
    }

    public void ReturnToPool(GameObject obj, GameObject originalPrefab)
    {
        if (obj == null) return;

        obj.SetActive(false);
        obj.transform.SetParent(transform); // On le range sous le Manager pour pas polluer la hiérarchie

        int key = originalPrefab.GetInstanceID();
        if (!_pools.ContainsKey(key))
        {
            _pools.Add(key, new Queue<GameObject>());
        }

        _pools[key].Enqueue(obj);
    }
}