using System.Collections.Generic;
using UnityEngine;

public class DamageTextPool : MonoBehaviour
{
    public static DamageTextPool Instance { get; private set; }

    [SerializeField] private DamageText textPrefab;
    [SerializeField] private int initialSize = 50;

    private Queue<DamageText> _pool = new Queue<DamageText>();

    private void Awake()
    {
        Instance = this;
        for (int i = 0; i < initialSize; i++) CreateNew();
    }

    private DamageText CreateNew()
    {
        DamageText t = Instantiate(textPrefab, transform);
        t.gameObject.SetActive(false);
        _pool.Enqueue(t);
        return t;
    }

    public void Spawn(float damage, Vector3 position, bool isCritical = false)
    {
        if (_pool.Count == 0) CreateNew();

        DamageText t = _pool.Dequeue();
        t.Initialize(damage, position, isCritical);
    }

    public void ReturnToPool(DamageText t)
    {
        t.gameObject.SetActive(false);
        _pool.Enqueue(t);
    }
}