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
        }
    }

    /// <summary>
    /// Applies area of effect damage around a center point
    /// </summary>
    public void ApplyAreaDamage(Vector3 center, SpellDefinition def)
    {
        float radius = def.Effect.aoeRadius > 0 ? def.Effect.aoeRadius : 3f;
        var enemies = EnemyManager.Instance.GetEnemiesInRange(center, radius);

        foreach (var enemy in enemies)
        {
            ApplyDamage(enemy, def);
        }

        // TODO: Instantiate VFX Explosion
    }

    /// <summary>
    /// Applies damage and status effects to a single enemy
    /// </summary>
    public void ApplyDamage(EnemyController enemy, SpellDefinition def)
    {
        // Apply base damage
        enemy.TakeDamage(def.Damage);

        // Apply status effects
        if (def.Effect != null)
        {
            if (def.Effect.applyBurn)
            {
                enemy.ApplyBurn(def.Damage * 0.2f, 3f);
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
}
