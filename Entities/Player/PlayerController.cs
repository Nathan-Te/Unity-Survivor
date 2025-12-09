using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour, IDamageable
{
    public static PlayerController Instance { get; private set; }

    [Header("Stats de Base")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private float _currentHp;

    public float MaxHp => maxHp;
    public float CurrentHp => _currentHp;

    // Variables internes stats
    private float _baseMoveSpeed;
    private float _regenPerSec = 0f;
    private float _armor = 0f;

    [Header("Interaction Ennemis")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float hitRadius = 1.0f;
    [SerializeField] private float slowFactor = 0.5f;
    [SerializeField] private LayerMask groundMask;

    private bool _isGodMode = false;

    public bool IsManualAiming { get; private set; }
    public Vector3 MouseWorldPosition { get; private set; }

    public Vector2 MoveInput => _moveInput;

    private CharacterController _controller;
    private Camera _mainCamera;
    private Vector2 _moveInput;
    private Collider[] _hitBuffer = new Collider[20];

    public event System.Action<float, float> OnHealthChanged;

    private void Awake()
    {
        Instance = this;
        _controller = GetComponent<CharacterController>();
        _mainCamera = Camera.main;
        _currentHp = maxHp;
        _baseMoveSpeed = moveSpeed;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(_currentHp, maxHp);
    }

    private void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        _moveInput = new Vector2(h, v);

        if (Input.GetMouseButtonDown(1))
        {
            IsManualAiming = !IsManualAiming;
        }

        float currentSpeed = moveSpeed;
        CheckEnemyContact(ref currentSpeed);

        HandleMovement(currentSpeed);
        HandleRotation();

        if (_regenPerSec > 0 && _currentHp < maxHp)
        {
            _currentHp += _regenPerSec * Time.deltaTime;
            _currentHp = Mathf.Min(_currentHp, maxHp);
        }
    }

    public void Heal(float amount)
    {
        _currentHp += amount;
        if (_currentHp > maxHp) _currentHp = maxHp;
        Debug.Log($"Player Healed: +{amount}. HP: {_currentHp}/{maxHp}");
        // Ici tu pourrais ajouter un VFX de soin ou un popup text
        OnHealthChanged?.Invoke(_currentHp, maxHp);
    }

    // --- Méthodes Stats ---
    public void ModifySpeed(float percentAdd)
    {
        moveSpeed = _baseMoveSpeed * (1.0f + percentAdd);
    }
    public void ModifyMaxHealth(float flatAdd)
    {
        maxHp += flatAdd;
        _currentHp += flatAdd;
        OnHealthChanged?.Invoke(_currentHp, maxHp);
    }
    public void ModifyRegen(float flatAdd) => _regenPerSec += flatAdd;
    public void ModifyArmor(float flatAdd) => _armor += flatAdd;

    // ----------------------

    private void CheckEnemyContact(ref float currentMoveSpeed)
    {
        System.Array.Clear(_hitBuffer, 0, _hitBuffer.Length);
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, hitRadius, _hitBuffer, enemyLayer);

        if (numColliders > 0)
        {
            currentMoveSpeed *= slowFactor;
            float totalDamageThisFrame = 0f;

            for (int i = 0; i < numColliders; i++)
            {
                if (_hitBuffer[i] == null) continue;
                if (EnemyManager.Instance.TryGetEnemyByCollider(_hitBuffer[i], out EnemyController enemy))
                {
                    totalDamageThisFrame += enemy.currentDamage;
                }
            }

            if (totalDamageThisFrame > 0)
            {
                TakeDamage(totalDamageThisFrame * Time.deltaTime);
            }
        }
    }

    public void ToggleGodMode()
    {
        _isGodMode = !_isGodMode;
        Debug.Log($"God Mode: {_isGodMode}");
    }

    public void TakeDamage(float amount)
    {
        if (_isGodMode) return;
        float reducedDamage = Mathf.Max(0f, amount - _armor);
        _currentHp -= reducedDamage;

        OnHealthChanged?.Invoke(_currentHp, maxHp);

        if (_currentHp <= 0)
        {
            Debug.Log("GAME OVER");
            Time.timeScale = 0;
        }
    }

    private void HandleMovement(float speed)
    {
        Vector3 move = new Vector3(_moveInput.x, 0, _moveInput.y);
        if (move.magnitude > 1f) move.Normalize();
        Vector3 finalMove = move * speed;
        finalMove.y = -9.81f;
        _controller.Move(finalMove * Time.deltaTime);
    }

    private void HandleRotation()
    {
        if (IsManualAiming)
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundMask))
            {
                MouseWorldPosition = hit.point;
                Vector3 targetPosition = new Vector3(hit.point.x, transform.position.y, hit.point.z);
                Vector3 direction = (targetPosition - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotationSpeed);
                }
            }
        }
        else
        {
            Vector3 moveDir = new Vector3(_moveInput.x, 0, _moveInput.y);
            if (moveDir.sqrMagnitude > 0.01f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(moveDir.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}