using UnityEngine;
using System;

public abstract class ZonePOI : PointOfInterest
{
    public event Action<float> OnProgressChanged;

    [Header("Zone Settings")]
    [SerializeField] private float radius = 5f;
    [SerializeField] private float requiredCharge = 100f;

    [Header("Conditions")]
    [SerializeField] private bool chargeByTime = true; // Si true, charge quand le joueur est dedans
    [SerializeField] private float baseChargeSpeed = 10f;  // Points par seconde (base value, will be scaled)

    [SerializeField] private bool chargeByKills = false; // Si true, charge par ennemis tues
    [SerializeField] private float baseChargePerKill = 20f; // Charge per kill (base value, will be scaled)

    private float _currentCharge = 0f;
    private bool _playerInside = false;

    // Cached scaled values (recalculated each frame based on game time)
    private float _scaledChargeSpeed;
    private float _scaledChargePerKill;

    private void Start()
    {
        // S'abonner aux morts
        if (chargeByKills && EnemyManager.Instance != null)
        {
            EnemyManager.Instance.OnEnemyDeathPosition += OnEnemyDeath;
        }

        OnProgressChanged?.Invoke(0f);
    }

    private void OnDestroy()
    {
        if (chargeByKills && EnemyManager.Instance != null)
        {
            EnemyManager.Instance.OnEnemyDeathPosition -= OnEnemyDeath;
        }
    }

    private void Update()
    {
        if (isCompleted) return;

        // Update scaled values each frame based on current game time
        UpdateScaledValues();

        CheckPlayerDistance();

        if (_playerInside && chargeByTime)
        {
            AddCharge(_scaledChargeSpeed * Time.deltaTime);
        }
    }

    private void UpdateScaledValues()
    {
        if (ZonePOIScalingManager.Instance != null)
        {
            _scaledChargeSpeed = ZonePOIScalingManager.Instance.GetScaledChargeSpeed(baseChargeSpeed);
            _scaledChargePerKill = ZonePOIScalingManager.Instance.GetScaledChargePerKill(baseChargePerKill);
        }
        else
        {
            // Fallback to base values if manager doesn't exist
            _scaledChargeSpeed = baseChargeSpeed;
            _scaledChargePerKill = baseChargePerKill;
        }
    }

    private void CheckPlayerDistance()
    {
        if (PlayerController.Instance == null) return;
        float dist = Vector3.Distance(transform.position, PlayerController.Instance.transform.position);
        _playerInside = dist <= radius;
    }

    private void OnEnemyDeath(Vector3 pos)
    {
        if (isCompleted || !_playerInside) return; // Le joueur doit etre dans la zone pour capter les ames ?

        // Verifier si l'ennemi est mort DANS la zone
        if (Vector3.Distance(transform.position, pos) <= radius)
        {
            AddCharge(_scaledChargePerKill);
        }
    }

    private void AddCharge(float amount)
    {
        _currentCharge += amount;

        float ratio = Mathf.Clamp01(_currentCharge / requiredCharge);
        OnProgressChanged?.Invoke(ratio);

        if (_currentCharge >= requiredCharge)
        {
            CompletePOI();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _playerInside ? Color.green : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
