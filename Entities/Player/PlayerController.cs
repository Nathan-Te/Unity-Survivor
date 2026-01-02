using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : Singleton<PlayerController>, IDamageable
{

    [Header("Stats de Base")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private float _currentHp;

    public float MaxHp => maxHp;
    public float CurrentHp => _currentHp;

    // Variables internes stats
    private float _baseMoveSpeed;
    private float _moveSpeedBonus = 0f; // Cumulative % bonus
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

    // Throttle UI updates for regeneration to avoid spam
    private float _lastRegenUIUpdate = 0f;
    private const float REGEN_UI_UPDATE_INTERVAL = 0.1f; // Update UI every 0.1s during regen

    public event System.Action<float, float> OnHealthChanged;

    protected override void Awake()
    {
        base.Awake();

        // Always initialize critical components and state
        // This ensures proper initialization even after scene reload
        _controller = GetComponent<CharacterController>();
        _mainCamera = Camera.main;
        _currentHp = maxHp;
        _baseMoveSpeed = moveSpeed;

        // Reset bonus stats to default
        _moveSpeedBonus = 0f;
        _regenPerSec = 0f;
        _armor = 0f;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(_currentHp, maxHp);
    }

    private void Update()
    {
        // SAFETY: Stop executing if scene is restarting/loading
        if (SingletonGlobalState.IsSceneLoading || SingletonGlobalState.IsApplicationQuitting)
            return;

        // Don't process if game is not in Playing state (handles pause, restart, level-up)
        if (GameStateController.Instance != null && !GameStateController.Instance.IsPlaying)
        {
            return;
        }

        // Safety check: if controller is null, don't process
        if (_controller == null)
        {
            Debug.LogError("[PlayerController] _controller is null in Update!");
            return;
        }

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

        // Health regeneration
        if (_regenPerSec > 0 && _currentHp < maxHp)
        {
            float previousHp = _currentHp;
            _currentHp += _regenPerSec * Time.deltaTime;
            _currentHp = Mathf.Min(_currentHp, maxHp);

            // Throttle UI updates to avoid spamming events every frame
            bool reachedMax = _currentHp >= maxHp && previousHp < maxHp;
            if (Time.time - _lastRegenUIUpdate >= REGEN_UI_UPDATE_INTERVAL || reachedMax)
            {
                OnHealthChanged?.Invoke(_currentHp, maxHp);
                _lastRegenUIUpdate = Time.time;
            }
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

    // --- M�thodes Stats ---
    public void ModifySpeed(float percentAdd)
    {
        Debug.Log($"[PlayerController] ModifySpeed - percentAdd: {percentAdd}, baseMoveSpeed: {_baseMoveSpeed}, OLD bonus: {_moveSpeedBonus}, OLD moveSpeed: {moveSpeed}");
        _moveSpeedBonus += percentAdd;
        moveSpeed = _baseMoveSpeed * (1.0f + _moveSpeedBonus);
        Debug.Log($"[PlayerController] NEW bonus: {_moveSpeedBonus}, NEW moveSpeed: {moveSpeed}");
    }
    public void ModifyMaxHealth(float flatAdd)
    {
        maxHp += flatAdd;
        _currentHp += flatAdd;
        OnHealthChanged?.Invoke(_currentHp, maxHp);
    }
    public void ModifyRegen(float flatAdd)
    {
        _regenPerSec += flatAdd;
        Debug.Log($"[PlayerController] ModifyRegen - flatAdd: {flatAdd}, NEW _regenPerSec: {_regenPerSec}");
    }
    public void ModifyArmor(float flatAdd) => _armor += flatAdd;

    // ----------------------

    private void CheckEnemyContact(ref float currentMoveSpeed)
    {
        // Safety check: don't process if EnemyManager is null (during restart)
        if (EnemyManager.Instance == null) return;

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

        // Play player hit sound (throttled in AudioManager)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerHitSound();
        }

        if (_currentHp <= 0)
        {
            Die();
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

    /// <summary>
    /// Handles player death and triggers game over event
    /// </summary>
    private void Die()
    {
        Debug.Log("[PlayerController] Player died!");

        // Play game over sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameOverSound();
        }

        // Gather statistics for Game Over screen
        float timeSurvived = 0f;
        int levelReached = 1;
        int enemiesKilled = 0;

        // Get time survived from GameTimer
        if (GameTimer.Instance != null)
        {
            timeSurvived = GameTimer.Instance.ElapsedTime;
        }

        // Get level reached from LevelManager
        if (LevelManager.Instance != null)
        {
            levelReached = LevelManager.Instance.currentLevel;
        }

        // Get enemies killed from EnemyManager
        if (EnemyManager.Instance != null)
        {
            enemiesKilled = EnemyManager.Instance.TotalKills;
        }

        // Get score from ArcadeScoreSystem
        int score = 0;
        if (ArcadeScoreSystem.Instance != null)
        {
            score = ArcadeScoreSystem.Instance.TotalScore;
        }

        // Fire death event (GameOverManager will handle the rest)
        GameEvents.OnPlayerDeath.Invoke(timeSurvived, levelReached, enemiesKilled, score);
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