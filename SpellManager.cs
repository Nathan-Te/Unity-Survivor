using System;
using System.Collections.Generic;
using UnityEngine;

public class SpellManager : MonoBehaviour
{
    [Header("Inventaire Actif")]
    [SerializeField] private List<SpellSlot> activeSlots = new List<SpellSlot>();

    private Transform _playerTransform;

    public event Action OnInventoryUpdated;

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

        OnInventoryUpdated?.Invoke();
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
        // On vérifie Definition car les runes peuvent être vides au départ
        if (slot.Definition == null) return;

        slot.currentCooldown -= Time.deltaTime;

        if (slot.currentCooldown <= 0f)
        {
            bool attacked = AttemptAttack(slot.Definition);

            if (attacked)
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

        bool isFullCircle = Mathf.Abs(spread - 360f) < 0.1f;
        float angleStep = (count > 1) ? (isFullCircle ? spread / count : spread / (count - 1)) : 0;
        float startAngle = count > 1 ? -spread / 2f : 0;
        if (isFullCircle) startAngle = 0f;

        for (int i = 0; i < count; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);
            Vector3 finalDir = rotation * dirToTarget;

            Vector3 spawnPos = _playerTransform.position + Vector3.up + finalDir * 0.5f;

            GameObject p = ProjectilePool.Instance.Get(def.Form.prefab, spawnPos, Quaternion.LookRotation(finalDir));

            if (p.TryGetComponent<ProjectileController>(out var ctrl))
            {
                ctrl.Initialize(def, finalDir, i, count);
            }
        }
    }

    // --- CORRECTIONS POUR LA NOUVELLE ARCHITECTURE RUNE ---

    public void AddNewSlot(SpellForm form)
    {
        SpellSlot newSlot = new SpellSlot();
        newSlot.formRune = new Rune(form, 1);

        // Effet par défaut
        var defaultEffect = Resources.Load<SpellEffect>("Spells/Effects/Physical");
        newSlot.effectRune = new Rune(defaultEffect, 1);

        newSlot.ForceInit();
        activeSlots.Add(newSlot);

        OnInventoryUpdated?.Invoke(); // Refresh UI
    }

    public bool TryAddModifierToSlot(SpellModifier mod, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= activeSlots.Count) return false;

        SpellSlot slot = activeSlots[slotIndex];

        // Vérif Tag
        if (mod.requiredTag != SpellTag.None && !slot.formRune.AsForm.tags.HasFlag(mod.requiredTag))
        {
            Debug.Log("Incompatible !");
            return false;
        }

        // Ajout ou Remplacement (Logique FIFO simple pour l'instant)
        Rune newRune = new Rune(mod, 1);
        if (!slot.TryAddModifier(newRune))
        {
            // Si plein, on remplace le premier (ou le dernier)
            slot.ReplaceModifier(0, newRune);
        }

        OnInventoryUpdated?.Invoke();
        return true;
    }

    public List<SpellSlot> GetSlots() => activeSlots;
}