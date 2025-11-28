using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Stats de Base")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private float _currentHp;

    [Header("Interaction Ennemis")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float hitRadius = 1.0f;
    [SerializeField] private float slowFactor = 0.5f;
    [SerializeField] private LayerMask groundMask;
    
    private bool _isGodMode = false;

    // --- NOUVEAU : Gestion de la visée ---
    public bool IsManualAiming { get; private set; }
    public Vector3 MouseWorldPosition { get; private set; } // Pour que le SpellManager sache où on vise

    private CharacterController _controller;
    private Camera _mainCamera;
    private Vector2 _moveInput;
    private Collider[] _hitBuffer = new Collider[20];

    private void Awake()
    {
        Instance = this;
        _controller = GetComponent<CharacterController>();
        _mainCamera = Camera.main;
        _currentHp = maxHp;
    }

    private void Update()
    {
        // 1. Inputs Mouvement
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        _moveInput = new Vector2(h, v);

        // 2. Input Visée (Clic Droit maintenu pour viser, sinon Auto)
        if (Input.GetMouseButtonDown(1))
        {
            IsManualAiming = !IsManualAiming;
        }

        // 3. Gestion des Ennemis
        float currentSpeed = moveSpeed;
        CheckEnemyContact(ref currentSpeed);

        // 4. Mouvements & Rotation
        HandleMovement(currentSpeed);
        HandleRotation();
    }

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
        if (_isGodMode) return; // <--- LE HOOK EST ICI

        _currentHp -= amount;
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
        finalMove.y = -9.81f; // Gravité simple

        _controller.Move(finalMove * Time.deltaTime);
    }

    private void HandleRotation()
    {
        // CAS 1 : Visée Manuelle (On regarde la souris)
        if (IsManualAiming)
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundMask))
            {
                // On stocke la position pour le SpellManager
                MouseWorldPosition = hit.point;

                Vector3 targetPosition = new Vector3(hit.point.x, transform.position.y, hit.point.z);
                Vector3 direction = (targetPosition - transform.position).normalized;

                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotationSpeed);
                }
            }
        }
        // CAS 2 : Visée Auto (On regarde dans le sens de la marche)
        else
        {
            Vector3 moveDir = new Vector3(_moveInput.x, 0, _moveInput.y);
            if (moveDir.sqrMagnitude > 0.01f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(moveDir.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            }
            // Si on ne bouge pas, on garde la dernière rotation (ne rien faire)
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}