using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles spell casting - targeting, firing, and cooldown management.
/// </summary>
[RequireComponent(typeof(SpellInventory))]
public class SpellCaster : MonoBehaviour
{
    private Transform _playerTransform;
    private SpellInventory _inventory;

    // Track active Orbit projectiles per slot (pour mise à jour dynamique)
    private Dictionary<int, List<ProjectileController>> _orbitProjectiles
        = new Dictionary<int, List<ProjectileController>>();

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
            ProcessSlot(slots[i], i);
        }
    }

    private void ProcessSlot(SpellSlot slot, int slotIndex)
    {
        if (slot.Definition == null) return;

        // Pour les Orbit, on ne lance qu'une fois, puis on met à jour dynamiquement
        if (slot.Definition.Form.tags.HasFlag(SpellTag.Orbit))
        {
            ProcessOrbitSlot(slot, slotIndex);
        }
        else
        {
            // Comportement normal pour les autres types de spells
            slot.currentCooldown -= Time.deltaTime;

            if (slot.currentCooldown <= 0f)
            {
                if (AttemptAttack(slot.Definition))
                {
                    slot.currentCooldown = slot.Definition.Cooldown;
                }
            }
        }
    }

    private void ProcessOrbitSlot(SpellSlot slot, int slotIndex)
    {
        // S'assurer que nous avons une liste pour ce slot
        if (!_orbitProjectiles.ContainsKey(slotIndex))
        {
            _orbitProjectiles[slotIndex] = new List<ProjectileController>();
        }

        var activeOrbits = _orbitProjectiles[slotIndex];

        // Nettoyer les projectiles détruits
        activeOrbits.RemoveAll(p => p == null || !p.gameObject.activeInHierarchy);

        // Vérifier si la définition a changé (upgrade, changement d'effect, etc.)
        bool needsRefresh = false;

        if (activeOrbits.Count > 0 && activeOrbits[0] != null)
        {
            // Comparer la définition actuelle avec celle des projectiles existants
            var currentDef = activeOrbits[0].Definition;

            // Si n'importe quelle propriété importante a changé, on relance tout
            if (currentDef.Count != slot.Definition.Count ||
                currentDef.Damage != slot.Definition.Damage ||
                currentDef.Speed != slot.Definition.Speed ||
                currentDef.Effect != slot.Definition.Effect ||
                currentDef.Prefab != slot.Definition.Prefab)
            {
                needsRefresh = true;
            }
        }

        // Si aucun orbit ou si la définition a changé, tout détruire et relancer
        if (activeOrbits.Count == 0 || needsRefresh)
        {
            // Détruire tous les orbits existants
            foreach (var orbit in activeOrbits)
            {
                if (orbit != null && orbit.gameObject != null)
                {
                    orbit.Despawn();
                }
            }
            activeOrbits.Clear();

            // Créer les nouveaux orbits avec la définition à jour
            int desiredCount = slot.Definition.Count;
            for (int i = 0; i < desiredCount; i++)
            {
                Vector3 spawnPos = _playerTransform.position + Vector3.up;
                Vector3 dummyDir = Vector3.forward;

                GameObject projectile = ProjectilePool.Instance.Get(
                    slot.Definition.Prefab,
                    spawnPos,
                    Quaternion.identity
                );

                if (projectile != null && projectile.TryGetComponent<ProjectileController>(out var ctrl))
                {
                    ctrl.Initialize(slot.Definition, dummyDir, slot.Definition.Prefab, i, desiredCount);
                    activeOrbits.Add(ctrl);
                }
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

    private void OnDisable()
    {
        // Nettoyer tous les orbits quand le caster est désactivé
        foreach (var orbits in _orbitProjectiles.Values)
        {
            foreach (var orbit in orbits)
            {
                if (orbit != null && orbit.gameObject != null)
                {
                    orbit.Despawn();
                }
            }
        }
        _orbitProjectiles.Clear();
    }
}
