using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance { get; private set; }

    [Header("Pool Config")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int initialPoolSize = 50;

    private Queue<GameObject> _pool = new Queue<GameObject>();

    private void Awake()
    {
        Instance = this;
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewEnemy();
        }
    }

    private GameObject CreateNewEnemy()
    {
        GameObject obj = Instantiate(enemyPrefab, transform);
        obj.SetActive(false);
        _pool.Enqueue(obj);
        return obj;
    }

    public GameObject GetEnemy(Vector3 position, Quaternion rotation)
    {
        if (_pool.Count == 0)
        {
            CreateNewEnemy(); // Agrandissement dynamique si nécessaire
        }

        GameObject enemy = _pool.Dequeue();
        enemy.transform.position = position;
        enemy.transform.rotation = rotation;
        enemy.SetActive(true);
        return enemy;
    }

    public void ReturnToPool(GameObject enemy)
    {
        enemy.SetActive(false);
        _pool.Enqueue(enemy);
    }
}