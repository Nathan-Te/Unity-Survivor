using UnityEngine;

/// <summary>
/// Handles chain lightning reactions for projectiles.
/// </summary>
[RequireComponent(typeof(ProjectileController))]
public class ProjectileChainReaction : MonoBehaviour
{
    private ProjectileController _controller;

    private void Awake()
    {
        _controller = GetComponent<ProjectileController>();
    }

    /// <summary>
    /// Creates a chain reaction that spawns a new projectile towards another enemy
    /// </summary>
    public void HandleChainReaction(EnemyController currentTarget, SpellDefinition def)
    {
        if (def.ChainCount <= 0) return;

        // Find the nearest enemy within chain range
        var candidates = EnemyManager.Instance.GetEnemiesInRange(transform.position, def.ChainRange);
        EnemyController bestCandidate = null;
        float closestDistSqr = float.MaxValue;

        foreach (var candidate in candidates)
        {
            // Skip the current target and dead enemies
            if (candidate == currentTarget || candidate.currentHp <= 0)
                continue;

            float distSqr = (candidate.transform.position - currentTarget.transform.position).sqrMagnitude;
            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                bestCandidate = candidate;
            }
        }

        if (bestCandidate == null) return;

        // Create a new spell definition for the chained projectile
        SpellDefinition chainDef = CreateChainDefinition(def);

        // Spawn the chain projectile
        Vector3 spawnPos = currentTarget.transform.position + Vector3.up;
        Vector3 direction = (bestCandidate.transform.position - spawnPos).normalized;

        GameObject chainProjectile = ProjectilePool.Instance.Get(
            def.Prefab,
            spawnPos,
            Quaternion.LookRotation(direction)
        );

        if (chainProjectile != null && chainProjectile.TryGetComponent<ProjectileController>(out var chainController))
        {
            chainController.Initialize(chainDef, direction, def.Prefab);
        }
    }

    /// <summary>
    /// Creates a new spell definition for chained projectiles with reduced stats
    /// </summary>
    private SpellDefinition CreateChainDefinition(SpellDefinition original)
    {
        SpellDefinition chainDef = new SpellDefinition();

        // Copy form and effect
        chainDef.Form = original.Form;
        chainDef.Effect = original.Effect;
        chainDef.Prefab = original.Prefab; // CRITICAL: Copy the prefab reference!

        // Copy basic stats with slight reduction
        chainDef.Speed = original.Speed;
        chainDef.Size = original.Size * 0.8f; // Slightly smaller
        chainDef.Range = original.ChainRange * 1.2f;

        // Apply damage reduction
        chainDef.Damage = original.Damage * original.ChainDamageReduction;

        // Reduce chain count
        chainDef.ChainCount = original.ChainCount - 1;
        chainDef.ChainRange = original.ChainRange;
        chainDef.ChainDamageReduction = original.ChainDamageReduction;

        return chainDef;
    }
}
