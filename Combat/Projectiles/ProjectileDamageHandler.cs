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

            // Play impact sound
            if (AudioManager.Instance != null && def.Form != null && def.Effect != null)
            {
                AudioManager.Instance.PlaySpellImpactSound(def.Form, def.Effect, target.transform.position);
            }
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

        // Also damage any Destructible POIs in range (Chests, etc.)
        Collider[] hits = Physics.OverlapSphere(center, radius, LayerMask.GetMask("Destructible"));
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(def.Damage);
            }
        }

        // Spawn AOE impact VFX at center (skip for Smite - VFX handled by SmiteMotion timing)
        if (spawnVfx)
        {
            SpawnImpactVfx(center, def);

            // Play area explosion sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayAreaExplosionSound(center);
            }
        }
    }

    /// <summary>
    /// Applies damage and status effects to a single enemy
    /// </summary>
    public void ApplyDamage(EnemyController enemy, SpellDefinition def)
    {
        // Calculate overcrit: CritChance can exceed 100%
        // Example: 150% CritChance = 1 guaranteed crit + 50% chance for a second crit
        float critChance = def.CritChance;
        int guaranteedCrits = Mathf.FloorToInt(critChance);
        float remainingCritChance = critChance - guaranteedCrits;

        // Roll for additional crit if there's a remainder
        int totalCrits = guaranteedCrits;
        if (remainingCritChance > 0 && Random.value < remainingCritChance)
        {
            totalCrits++;
        }

        // Calculate final damage
        float finalDamage = def.Damage;
        bool isCritical = totalCrits > 0;

        if (isCritical)
        {
            // Each crit multiplies damage successively
            // Example: 10 dmg, 2x crit, 2 crits = 10 * 2 * 2 = 40 dmg
            for (int i = 0; i < totalCrits; i++)
            {
                finalDamage *= def.CritDamageMultiplier;
            }
        }

        // Apply damage with appropriate damage type
        DamageType damageType = isCritical ? DamageType.Critical : DamageType.Normal;
        enemy.TakeDamage(finalDamage, damageType);

        // Play hit sound (enemy damage sound is handled by EnemyController)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyHitSound(enemy.transform.position, isCritical);
        }

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
