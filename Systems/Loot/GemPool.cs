using System.Collections.Generic;
using UnityEngine;

public class GemPool : MonoBehaviour
{
    public static GemPool Instance { get; private set; }

    [SerializeField] private GameObject gemPrefab;
    [SerializeField] private int initialSize = 100;

    private Queue<GameObject> _pool = new Queue<GameObject>();

    private void Awake()
    {
        Instance = this;
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Instantiate(gemPrefab, transform);
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    public void Spawn(Vector3 position, int xpValue)
    {
        if (_pool.Count == 0)
        {
            GameObject newObj = Instantiate(gemPrefab, transform);
            newObj.SetActive(false);
            _pool.Enqueue(newObj);
        }

        GameObject gem = _pool.Dequeue();
        gem.transform.position = position;
        gem.SetActive(true);

        if (gem.TryGetComponent<ExperienceGem>(out var gemScript))
        {
            gemScript.Initialize(xpValue);
        }
    }

    public void ReturnToPool(GameObject gem)
    {
        gem.SetActive(false);
        _pool.Enqueue(gem);
    }
}