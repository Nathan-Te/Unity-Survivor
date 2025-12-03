using UnityEngine;

public abstract class DestructiblePOI : PointOfInterest, IDamageable
{
    [Header("Destructible Settings")]
    [SerializeField] private float maxHp = 50f;
    private float _currentHp;

    protected virtual void Start()
    {
        _currentHp = maxHp;
    }

    public void TakeDamage(float amount)
    {
        if (isCompleted) return;
        _currentHp -= amount;

        if (DamageTextPool.Instance != null)
            DamageTextPool.Instance.Spawn(amount, transform.position);

        if (_currentHp <= 0) CompletePOI();
    }
}