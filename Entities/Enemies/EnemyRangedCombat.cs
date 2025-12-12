using UnityEngine;

/// <summary>
/// Handles ranged combat for enemies (attack timing, projectile spawning).
/// </summary>
[RequireComponent(typeof(EnemyController))]
public class EnemyRangedCombat : MonoBehaviour
{
    private EnemyController _controller;
    private EnemyAnimator _enemyAnimator;
    private float _attackTimer;

    private void Awake()
    {
        _controller = GetComponent<EnemyController>();
        _enemyAnimator = GetComponentInChildren<EnemyAnimator>();
    }

    /// <summary>
    /// Updates ranged attack logic. Called from time-sliced logic.
    /// </summary>
    public void UpdateRangedAttack(float deltaTime, EnemyData data)
    {
        if (data == null || data.projectilePrefab == null) return;
        if (PlayerController.Instance == null) return;

        Vector3 playerPos = PlayerController.Instance.transform.position;
        float distSqr = (playerPos - transform.position).sqrMagnitude;

        // Don't attack if fleeing
        float fleeDistSqr = data.fleeDistance * data.fleeDistance;
        if (data.fleeDistance > 0 && distSqr < fleeDistSqr) return;

        // Check if in attack range
        float attackRange = data.stopDistance + 2f;
        float attackRangeSqr = attackRange * attackRange;

        if (distSqr <= attackRangeSqr)
        {
            _attackTimer += deltaTime;

            if (_attackTimer >= data.attackCooldown)
            {
                TriggerAttack();
                _attackTimer = 0f;
            }
        }
        else
        {
            // Reduce timer when out of range
            _attackTimer = Mathf.Max(0, _attackTimer - deltaTime);
        }
    }

    /// <summary>
    /// Triggers the attack (with or without animation)
    /// </summary>
    private void TriggerAttack()
    {
        if (_enemyAnimator != null)
        {
            _enemyAnimator.TriggerAttackAnimation();
        }
        else
        {
            SpawnProjectile();
        }
    }

    /// <summary>
    /// Spawns a projectile towards the player.
    /// Called by animation event or directly if no animator.
    /// </summary>
    public void SpawnProjectile()
    {
        if (PlayerController.Instance == null) return;
        if (_controller.Data == null || _controller.Data.projectilePrefab == null) return;

        // Calculate direction towards player
        Vector3 playerPos = PlayerController.Instance.transform.position;
        Vector3 direction = (playerPos - transform.position).normalized;

        // Spawn position: center + slightly forward and up
        Vector3 spawnPos = transform.position + Vector3.up + direction;

        // Get projectile from pool
        GameObject projectile = ProjectilePool.Instance.Get(
            _controller.Data.projectilePrefab,
            spawnPos,
            Quaternion.LookRotation(direction)
        );

        if (projectile.TryGetComponent<ProjectileController>(out var projectileCtrl))
        {
            projectileCtrl.InitializeEnemyProjectile(
                _controller.Data.baseDamage,
                _controller.Data.projectilePrefab
            );
        }
    }

    /// <summary>
    /// Resets attack timer (called when enemy is pooled)
    /// </summary>
    public void ResetAttackTimer()
    {
        _attackTimer = 0f;
    }
}
