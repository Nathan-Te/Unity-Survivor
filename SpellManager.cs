using System.Collections.Generic;
using UnityEngine;

public class SpellManager : MonoBehaviour
{
    [Header("Inventaire Actif")]
    // On remplace le simple 'startingSpell' par une liste
    [SerializeField] private List<SpellSlot> activeSlots = new List<SpellSlot>();

    private Transform _playerTransform;

    private void Start()
    {
        if (PlayerController.Instance != null)
        {
            _playerTransform = PlayerController.Instance.transform;
        }

        // Initialisation des cooldowns (pour éviter que tout parte exactement à la frame 0)
        // Optionnel : on pourrait décaler légèrement les tirs pour éviter un lag spike
        foreach (var slot in activeSlots)
        {
            slot.currentCooldown = 0.5f; // Petit délai au démarrage
        }
    }

    private void Update()
    {
        if (_playerTransform == null) return;

        // On gère chaque slot indépendamment
        for (int i = 0; i < activeSlots.Count; i++)
        {
            ProcessSlot(activeSlots[i]);
        }
    }

    private void ProcessSlot(SpellSlot slot)
    {
        if (slot.spellData == null) return;

        slot.currentCooldown -= Time.deltaTime;

        if (slot.currentCooldown <= 0f)
        {
            // On passe le 'spellData' spécifique du slot à la tentative d'attaque
            bool attacked = AttemptAttack(slot.spellData);

            if (attacked)
            {
                slot.currentCooldown = slot.spellData.cooldown;
            }
        }
    }

    private bool AttemptAttack(SpellData spell)
    {
        Vector3 targetPos = Vector3.zero;
        bool hasTarget = false;

        // MODE MANUEL (Priorité absolue)
        if (PlayerController.Instance.IsManualAiming)
        {
            targetPos = PlayerController.Instance.MouseWorldPosition;
            hasTarget = true;
        }
        // MODE AUTO (Selon la stratégie du sort)
        else
        {
            Transform target = EnemyManager.Instance.GetTarget(
                _playerTransform.position,
                spell.range,
                spell.targetingMode,
                spell.explosionRadius,
                spell.requiresLineOfSight
            );

            if (target != null)
            {
                targetPos = target.position;
                hasTarget = true;
            }
        }

        if (hasTarget)
        {
            Fire(targetPos, spell);
            return true;
        }
        return false;
    }

    private void Fire(Vector3 targetPos, SpellData spell)
    {
        Vector3 dir = (targetPos - _playerTransform.position).normalized;
        dir.y = 0;

        // Petite variation de spawn pour éviter que les projectiles se chevauchent trop
        Vector3 spawnPos = _playerTransform.position + Vector3.up + dir * 0.5f;

        GameObject p = ProjectilePool.Instance.Get(spell.projectilePrefab, spawnPos, Quaternion.LookRotation(dir));

        if (p.TryGetComponent<ProjectileController>(out var ctrl))
        {
            // On initialise avec les données DU SLOT (pas un global)
            ctrl.Initialize(spell, dir);
        }
    }

    // Méthode publique pour le futur système de Level Up
    public void AddSpell(SpellData newSpell)
    {
        activeSlots.Add(new SpellSlot(newSpell));
    }
}