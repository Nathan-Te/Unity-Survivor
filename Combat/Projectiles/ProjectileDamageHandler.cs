using UnityEngine;

/// <summary>
/// Handles damage application, AOE effects, and status effects for projectiles.
/// </summary>
[RequireComponent(typeof(ProjectileController))]
public class ProjectileDamageHandler : MonoBehaviour
{
    private ProjectileController _controller;

    private void Awake()
    {
        _controller = GetComponent<ProjectileController>();
    }

    /// <summary>
    /// Applies hit effect - either single target or AOE damage
    /// </summary>
    public void ApplyHit(EnemyController target, SpellDefinition def)
    {
        if (def.Effect.aoeRadius > 0)
        {
            ApplyAreaDamage(transform.position, def);
        }
        else
        {
            ApplyDamage(target, def);

            // Spawn impact VFX at target position
            SpawnImpactVfx(target.transform.position, def);
        }
    }

    /// <summary>
    /// Applies area of effect damage around a center point
    /// </summary>
    /// <param name="spawnVfx">Whether to spawn impact VFX (false for Smite which handles VFX separately)</param>
    public void ApplyAreaDamage(Vector3 center, SpellDefinition def, bool spawnVfx = true)
    {
        float radius = def.Effect.aoeRadius > 0 ? def.Effect.aoeRadius : 3f;
        var enemies = EnemyManager.Instance.GetEnemiesInRange(center, radius);

        // Each enemy can crit independently
        foreach (var enemy in enemies)
        {
            ApplyDamage(enemy, def);
        }

        // Spawn AOE impact VFX at center (skip for Smite - VFX handled by SmiteMotion timing)
        if (spawnVfx)
        {
            SpawnImpactVfx(center, def);
        }
    }

    /// <summary>
    /// Applies damage and status effects to a single enemy
    /// </summary>
    public void ApplyDamage(EnemyController enemy, SpellDefinition def)
    {
        // Calculate if this hit is a critical
        bool isCritical = Random.value < def.CritChance;

        // Calculate final damage
        float finalDamage = def.Damage;
        if (isCritical)
        {
            finalDamage *= def.CritDamageMultiplier;
        }

        // Apply damage with appropriate damage type
        DamageType damageType = isCritical ? DamageType.Critical : DamageType.Normal;
        enemy.TakeDamage(finalDamage, damageType);

        // Apply status effects
        if (def.Effect != null)
        {
            if (def.Effect.applyBurn)
            {
                // Use calculated burn stats from SpellDefinition (includes bonuses)
                enemy.ApplyBurn(def.BurnDamagePerTick, def.BurnDuration);
            }

            if (def.Effect.applySlow)
            {
                enemy.ApplySlow(0.5f, 2f);
            }
        }

        // Apply knockback
        if (def.Knockback > 0 && enemy.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 pushDir = (enemy.transform.position - transform.position).normalized;
            rb.AddForce(pushDir * def.Knockback, ForceMode.Impulse);
        }

        // Minion spawn logic
        bool isFatal = (enemy.currentHp - def.Damage) <= 0;
        if (isFatal && def.MinionChance > 0 && def.MinionPrefab != null)
        {
            if (Random.value <= def.MinionChance)
            {
                Instantiate(def.MinionPrefab, enemy.transform.position, Quaternion.identity);
            }
        }
    }

    /// <summary>
    /// Spawns impact VFX at the specified position using VFXPool
    /// </summary>
    private void SpawnImpactVfx(Vector3 position, SpellDefinition def)
    {
        if (def.ImpactVfxPrefab != null && VFXPool.Instance != null)
        {
            VFXPool.Instance.Spawn(def.ImpactVfxPrefab, position, Quaternion.identity, 2f);
        }
    }
}
