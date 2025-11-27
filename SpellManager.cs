using UnityEngine;

public class SpellManager : MonoBehaviour
{
    [Header("Inventaire")]
    [SerializeField] private SpellData startingSpell; // Assigne ton ScriptableObject ici

    private float _cooldownTimer;

    // Référence au joueur pour savoir d'où tirer
    private Transform _playerTransform;

    private void Start()
    {
        if (PlayerController.Instance != null)
        {
            _playerTransform = PlayerController.Instance.transform;
        }
    }

    private void Update()
    {
        if (_playerTransform == null || startingSpell == null) return;

        _cooldownTimer -= Time.deltaTime;

        if (_cooldownTimer <= 0f)
        {
            AttemptAttack();
        }
    }

    private void AttemptAttack()
    {
        Vector3 targetPosition;

        // --- MODE A : VISÉE MANUELLE (Skillshot) ---
        if (PlayerController.Instance.IsManualAiming)
        {
            // On tire directement vers la souris
            // Note : MouseWorldPosition est mis à jour par le PlayerController
            targetPosition = PlayerController.Instance.MouseWorldPosition;

            // On tire !
            Fire(targetPosition);
            _cooldownTimer = startingSpell.cooldown;
        }
        // --- MODE B : AUTOMATIQUE (Auto-Aim) ---
        else
        {
            // On cherche une cible intelligente
            Transform target = EnemyManager.Instance.GetTarget(
                _playerTransform.position,
                startingSpell.range,
                startingSpell.targetingMode,
                startingSpell.explosionRadius,
                startingSpell.requiresLineOfSight
            );

            if (target != null)
            {
                targetPosition = target.position;
                Fire(targetPosition);
                _cooldownTimer = startingSpell.cooldown;
            }
        }
    }

    private void Fire(Vector3 targetPos)
    {
        // Direction vers l'ennemi
        Vector3 direction = (targetPos - _playerTransform.position).normalized;
        direction.y = 0; // On garde le tir horizontal

        // Spawn du projectile via le Pool
        // On spawn légèrement devant le joueur (Vector3.up pour la hauteur du torse)
        Vector3 spawnPos = _playerTransform.position + Vector3.up + direction * 0.5f;

        GameObject projObj = ProjectilePool.Instance.Get(startingSpell.projectilePrefab, spawnPos, Quaternion.LookRotation(direction));

        // Initialisation des données du projectile
        if (projObj.TryGetComponent<ProjectileController>(out var controller))
        {
            controller.Initialize(startingSpell, direction);
        }
    }
}