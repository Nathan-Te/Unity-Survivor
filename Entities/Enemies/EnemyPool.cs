using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance { get; private set; }

    // Dictionnaire : ID du Prefab -> File d'attente
    private Dictionary<int, Queue<GameObject>> _pools = new Dictionary<int, Queue<GameObject>>();

    [SerializeField] private int maxPoolSizePerPrefab = 50;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        _pools?.Clear();
        Instance = null;
    }

    // On demande un ennemi spécifique (prefab)
    public GameObject GetEnemy(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int key = prefab.GetInstanceID();

        if (!_pools.ContainsKey(key))
        {
            _pools.Add(key, new Queue<GameObject>());
        }

        if (_pools[key].Count > 0)
        {
            GameObject obj = _pools[key].Dequeue();
            if (obj == null)
            {
                return Instantiate(prefab, position, rotation, transform);
            }
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // On instancie si la file est vide
            GameObject newObj = Instantiate(prefab, position, rotation, transform);
            return newObj;
        }
    }

    public void ReturnToPool(GameObject enemy, GameObject originalPrefab)
    {
        enemy.SetActive(false);
        int key = originalPrefab.GetInstanceID();

        if (!_pools.ContainsKey(key))
        {
            _pools.Add(key, new Queue<GameObject>());
        }

        if (_pools[key].Count >= maxPoolSizePerPrefab)
        {
            Destroy(enemy);
            return;
        }

        _pools[key].Enqueue(enemy);
    }

    public void ClearAll()
    {
        foreach (var kvp in _pools)
        {
            while (kvp.Value.Count > 0)
            {
                GameObject obj = kvp.Value.Dequeue();
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
        }
        _pools.Clear();

        Debug.Log("[EnemyPool] Pool vidé et objets détruits");
    }

    public void DestroyAll()
    {
        ClearAll();

        // Détruit aussi tous les enfants de ce GameObject (ennemis actifs)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        Debug.Log("[EnemyPool] TOUS les ennemis détruits (pool + actifs)");
    }
}