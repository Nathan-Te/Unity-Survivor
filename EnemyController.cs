using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private EnemyData data;

    [Header("État (Read Only)")]
    public float currentHp;
    public float currentDamage;
    public float currentSpeed;

    private Rigidbody _rb;
    private Collider _myCollider; // Référence locale mise en cache

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _myCollider = GetComponent<Collider>(); // On le récupère UNE fois ici
        if (_myCollider == null)
        {
            _myCollider = GetComponentInChildren<Collider>();
        }
        InitializeStats();
    }

    public void InitializeStats()
    {
        if (data == null)
        {
            currentHp = 10f;
            currentDamage = 5f;
            currentSpeed = 3f;
        }
        else
        {
            currentHp = data.baseHp;
            currentDamage = data.baseDamage;
            currentSpeed = data.baseSpeed;
            if (_rb) _rb.mass = data.mass;
        }
    }

    private void OnEnable()
    {
        if (EnemyManager.Instance != null)
        {
            // On passe notre collider pour l'inscription dans le dictionnaire
            EnemyManager.Instance.RegisterEnemy(this, _myCollider);
        }
    }

    private void OnDisable()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterEnemy(this, _myCollider);
        }
    }
}