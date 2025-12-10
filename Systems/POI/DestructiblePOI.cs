using UnityEngine;
using System;

public abstract class DestructiblePOI : PointOfInterest, IDamageable
{
    public event Action<float> OnHealthChanged;

    [Header("Destructible Settings")]
    [SerializeField] private float maxHp = 50f;
    private float _currentHp;

    protected virtual void Start()
    {
        _currentHp = maxHp;
        OnHealthChanged?.Invoke(1f);
    }

    public void TakeDamage(float amount)
    {
        if (isCompleted) return;
        _currentHp -= amount;

        if (DamageTextPool.Instance != null)
            DamageTextPool.Instance.Spawn(amount, transform.position);

        float ratio = Mathf.Clamp01(_currentHp / maxHp);
        OnHealthChanged?.Invoke(ratio);

        if (_currentHp <= 0) CompletePOI();
    } 
}