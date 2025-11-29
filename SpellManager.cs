using System.Collections.Generic;
using UnityEngine;

public class SpellManager : MonoBehaviour
{
    [Header("Inventaire Actif")]
    [SerializeField] private List<SpellSlot> activeSlots = new List<SpellSlot>();

    private Transform _playerTransform;

    private void Start()
    {
        if (PlayerController.Instance != null)
        {
            _playerTransform = PlayerController.Instance.transform;
        }

        foreach (var slot in activeSlots)
        {
            slot.ForceInit();
            slot.currentCooldown = 0.5f;
        }
    }

    private void Update()
    {
        if (_playerTransform == null) return;

        for (int i = 0; i < activeSlots.Count; i++)
        {
            ProcessSlot(activeSlots[i]);
        }
    }

    private void ProcessSlot(SpellSlot slot)
    {
        if (slot.Definition == null) return;

        slot.currentCooldown -= Time.deltaTime;

        if (slot.currentCooldown <= 0f)
        {
            // CORRECTION : On passe bien une SpellDefinition
            bool attacked = AttemptAttack(slot.Definition);

            if (attacked)
            {
                // CORRECTION : Majuscule à Cooldown
                slot.currentCooldown = slot.Definition.Cooldown;
            }
        }
    }

    // CORRECTION : Le paramètre est SpellDefinition, plus SpellData
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
        float spread = def.Form.baseSpread;

        float angleStep = count > 1 ? spread / (count - 1) : 0;
        float startAngle = count > 1 ? -spread / 2f : 0;

        for (int i = 0; i < count; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);
            Vector3 finalDir = rotation * dirToTarget;

            Vector3 spawnPos = _playerTransform.position + Vector3.up + finalDir * 0.5f;

            GameObject p = ProjectilePool.Instance.Get(def.Form.prefab, spawnPos, Quaternion.LookRotation(finalDir));

            if (p.TryGetComponent<ProjectileController>(out var ctrl))
            {
                ctrl.Initialize(def, finalDir);
            }
        }
    }

    // --- CORRECTION TEMPORAIRE : On commente AddSpell ---
    // Cette méthode ne peut plus prendre SpellData. Il faudra créer une méthode
    // qui prend (Form, Effect, Mods) plus tard.
    /*
    public void AddSpell(SpellData newSpell)
    {
        activeSlots.Add(new SpellSlot(newSpell));
    }
    */
}