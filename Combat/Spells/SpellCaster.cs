using UnityEngine;

/// <summary>
/// Handles spell casting - targeting, firing, and cooldown management.
/// </summary>
[RequireComponent(typeof(SpellInventory))]
public class SpellCaster : MonoBehaviour
{
    private Transform _playerTransform;
    private SpellInventory _inventory;

    private void Awake()
    {
        _inventory = GetComponent<SpellInventory>();
    }

    private void Start()
    {
        if (PlayerController.Instance != null)
            _playerTransform = PlayerController.Instance.transform;
    }

    private void Update()
    {
        if (_playerTransform == null) return;

        var slots = _inventory.GetSlots();
        for (int i = 0; i < slots.Count; i++)
        {
            ProcessSlot(slots[i]);
        }
    }

    private void ProcessSlot(SpellSlot slot)
    {
        if (slot.Definition == null) return;

        slot.currentCooldown -= Time.deltaTime;

        if (slot.currentCooldown <= 0f)
        {
            if (AttemptAttack(slot.Definition))
            {
                slot.currentCooldown = slot.Definition.Cooldown;
            }
        }
    }

    private bool AttemptAttack(SpellDefinition def)
    {
        Vector3 targetPos = Vector3.zero;
        bool hasTarget = false;

        if (PlayerController.Instance.IsManualAiming)
        {
            targetPos = PlayerController.Instance.MouseWorldPosition;
            hasTarget = true;
        }
        else
        {
            // Auto-aim: scan from chest height to avoid ground
            Vector3 scanOrigin = _playerTransform.position + Vector3.up;

            Transform target = EnemyManager.Instance.GetTarget(
                scanOrigin,
                def.Range,
                def.Mode,
                def.Effect.aoeRadius,
                def.RequiresLoS
            );

            if (target != null)
            {
                targetPos = target.position;
                hasTarget = true;
            }
        }

        if (hasTarget)
        {
            Fire(targetPos, def);
            return true;
        }

        return false;
    }

    private void Fire(Vector3 targetPos, SpellDefinition def)
    {
        Vector3 dirToTarget = (targetPos - _playerTransform.position).normalized;
        dirToTarget.y = 0;

        int count = def.Count;
        float spread = def.Spread;

        bool isFullCircle = Mathf.Abs(spread - 360f) < 0.1f;
        float angleStep = (count > 1) ? (isFullCircle ? spread / count : spread / (count - 1)) : 0;
        float startAngle = count > 1 ? -spread / 2f : 0;
        if (isFullCircle) startAngle = 0f;

        for (int i = 0; i < count; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);
            Vector3 finalDir = rotation * dirToTarget;

            Vector3 spawnPos = CalculateSpawnPosition(targetPos, finalDir, def, i, count);

            // Spawn projectile
            GameObject projectile = ProjectilePool.Instance.Get(
                def.Prefab,
                spawnPos,
                Quaternion.LookRotation(finalDir)
            );

            if (projectile != null && projectile.TryGetComponent<ProjectileController>(out var ctrl))
            {
                ctrl.Initialize(def, finalDir, def.Prefab, i, count);
            }
        }
    }

    private Vector3 CalculateSpawnPosition(Vector3 targetPos, Vector3 direction, SpellDefinition def, int index, int totalCount)
    {
        if (def.Form.tags.HasFlag(SpellTag.Smite))
        {
            Vector3 currentTarget = targetPos;

            // Multi-target logic for Smite
            if (index > 0 && !PlayerController.Instance.IsManualAiming)
            {
                // Try to find another enemy for multi-cast
                Vector3 scanOrigin = _playerTransform.position + Vector3.up;

                Transform extraTarget = EnemyManager.Instance.GetTarget(
                    scanOrigin,
                    def.Range,
                    TargetingMode.Random,
                    def.Effect.aoeRadius,
                    def.RequiresLoS
                );

                if (extraTarget != null)
                {
                    currentTarget = extraTarget.position;
                }
                else
                {
                    // If no other enemy found, add random offset
                    Vector2 rnd = Random.insideUnitCircle * 3.0f;
                    currentTarget += new Vector3(rnd.x, 0, rnd.y);
                }
            }
            else if (totalCount > 1)
            {
                // Add slight random spread for multiple casts
                Vector2 rnd = Random.insideUnitCircle * 2.0f;
                currentTarget += new Vector3(rnd.x, 0, rnd.y);
            }

            return currentTarget;
        }
        else
        {
            // BOLT / NOVA / ORBIT: spawn from player
            return _playerTransform.position + Vector3.up + direction * 0.5f;
        }
    }
}
