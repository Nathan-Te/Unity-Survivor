using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance { get; private set; }

    // Dictionnaire : ID du Prefab -> File d'attente
    private Dictionary<int, Queue<GameObject>> _pools = new Dictionary<int, Queue<GameObject>>();

    private void Awake()
    {
        Instance = this;
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

        _pools[key].Enqueue(enemy);
    }
}