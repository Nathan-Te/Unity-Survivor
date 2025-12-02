using UnityEngine;

public abstract class ZonePOI : PointOfInterest
{
    [Header("Zone Settings")]
    [SerializeField] private float radius = 5f;
    [SerializeField] private float requiredCharge = 100f;

    [Header("Conditions")]
    [SerializeField] private bool chargeByTime = true; // Si true, charge quand le joueur est dedans
    [SerializeField] private float chargeSpeed = 10f;  // Points par seconde

    [SerializeField] private bool chargeByKills = false; // Si true, charge par ennemis tués
    [SerializeField] private float chargePerKill = 20f;

    private float _currentCharge = 0f;
    private bool _playerInside = false;

    private void Start()
    {
        // S'abonner aux morts
        if (chargeByKills && EnemyManager.Instance != null)
        {
            EnemyManager.Instance.OnEnemyDeathPosition += OnEnemyDeath;
        }
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

        CheckPlayerDistance();

        if (_playerInside && chargeByTime)
        {
            AddCharge(chargeSpeed * Time.deltaTime);
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
        if (isCompleted || !_playerInside) return; // Le joueur doit être dans la zone pour capter les âmes ?

        // Vérifier si l'ennemi est mort DANS la zone
        if (Vector3.Distance(transform.position, pos) <= radius)
        {
            AddCharge(chargePerKill);
        }
    }

    private void AddCharge(float amount)
    {
        _currentCharge += amount;
        // Debug.Log($"Charge: {_currentCharge}/{requiredCharge}");
        // TODO: Mettre à jour une barre de progression UI (World Space Canvas)

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