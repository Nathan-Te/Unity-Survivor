using System;
using System.Collections.Generic;
using UnityEngine;

public class SpellManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private int maxSpellSlots = 4;

    [Header("Inventaire Actif")]
    [SerializeField] private List<SpellSlot> activeSlots = new List<SpellSlot>();

    private Transform _playerTransform;
    public event Action OnInventoryUpdated;

    private void Start()
    {
        if (PlayerController.Instance != null) _playerTransform = PlayerController.Instance.transform;
        foreach (var slot in activeSlots) { slot.ForceInit(); slot.currentCooldown = 0.5f; }
        OnInventoryUpdated?.Invoke();
    }

    private void Update()
    {
        if (_playerTransform == null) return;
        for (int i = 0; i < activeSlots.Count; i++) ProcessSlot(activeSlots[i]);
    }

    private void ProcessSlot(SpellSlot slot)
    {
        if (slot.Definition == null) return;
        slot.currentCooldown -= Time.deltaTime;
        if (slot.currentCooldown <= 0f)
        {
            if (AttemptAttack(slot.Definition)) slot.currentCooldown = slot.Definition.Cooldown;
        }
    }

    public void UpgradeRune(Rune rune, RuneDefinition def)
    {
        rune.ApplyUpgrade(def);
        // Recalculate stats du slot parent...
    }

    public void AddNewSlot(SpellForm form)
    {
        SpellSlot newSlot = new SpellSlot();
        newSlot.formRune = new Rune(form); // Constructeur simplifié (1 arg)

        var defaultEffect = Resources.Load<SpellEffect>("Spells/Effects/Physical");
        if (defaultEffect == null) defaultEffect = ScriptableObject.CreateInstance<SpellEffect>();

        newSlot.effectRune = new Rune(defaultEffect); // Constructeur simplifié

        newSlot.ForceInit();
        activeSlots.Add(newSlot);
        OnInventoryUpdated?.Invoke();
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
            Transform target = EnemyManager.Instance.GetTarget(scanOrigin, def.Range, def.Mode, def.Effect.aoeRadius, def.RequiresLoS);
            if (target != null) { targetPos = target.position; hasTarget = true; }
        }

        if (hasTarget) { Fire(targetPos, def); return true; }
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

            Vector3 spawnPos = _playerTransform.position + Vector3.up + finalDir * 0.5f;
            GameObject p = ProjectilePool.Instance.Get(def.Form.prefab, spawnPos, Quaternion.LookRotation(finalDir));
            if (p.TryGetComponent<ProjectileController>(out var ctrl)) ctrl.Initialize(def, finalDir, i, count);
        }
    }

    // --- GESTION INVENTAIRE ---

    public bool HasForm(SpellForm form)
    {
        foreach (var slot in activeSlots)
        {
            if (slot.formRune != null && slot.formRune.Data == form) return true;
        }
        return false;
    }

    // Méthode pour appliquer l'upgrade de forme
    public void UpgradeForm(SpellForm form, RuneDefinition def)
    {
        foreach (var slot in activeSlots)
        {
            if (slot.formRune != null && slot.formRune.Data == form)
            {
                slot.formRune.ApplyUpgrade(def);
                slot.RecalculateStats();
                OnInventoryUpdated?.Invoke();
                return;
            }
        }
    }

    public bool CanAddSpell() => activeSlots.Count < maxSpellSlots;

    // Ajout d'un nouveau sort (Le premier niveau prend en compte le boost de rareté)
    public void AddSpell(SpellForm form, Rarity rarity)
    {
        if (!CanAddSpell()) return;

        SpellSlot newSlot = new SpellSlot();

        // La rune commence avec la puissance de la rareté (ex: Leg = 3.0)
        float initialPower = RarityUtils.GetPowerBoost(rarity);
        newSlot.formRune = new Rune(form);

        var defaultEffect = Resources.Load<SpellEffect>("Spells/Effects/Physical");
        if (defaultEffect == null) defaultEffect = ScriptableObject.CreateInstance<SpellEffect>();
        newSlot.effectRune = new Rune(defaultEffect);

        newSlot.ForceInit();
        activeSlots.Add(newSlot);
        OnInventoryUpdated?.Invoke();
    }

    public void ReplaceSpell(SpellForm newForm, int slotIndex, Rarity rarity)
    {
        if (slotIndex < 0 || slotIndex >= activeSlots.Count) return;
        SpellSlot slot = activeSlots[slotIndex];

        float initialPower = RarityUtils.GetPowerBoost(rarity);
        slot.formRune = new Rune(newForm);

        // Reset Effet et Mods
        var defaultEffect = Resources.Load<SpellEffect>("Spells/Effects/Physical");
        if (defaultEffect == null) defaultEffect = ScriptableObject.CreateInstance<SpellEffect>();
        slot.effectRune = new Rune(defaultEffect);
        slot.modifierRunes = new Rune[2];

        slot.RecalculateStats();
        slot.currentCooldown = 0.5f;
        OnInventoryUpdated?.Invoke();
    }

    public void ReplaceEffect(SpellEffect newEffect, int slotIndex, Rarity rarity)
    {
        if (slotIndex < 0 || slotIndex >= activeSlots.Count) return;

        // Si l'effet est déjà le même, on upgrade
        if (activeSlots[slotIndex].effectRune.Data == newEffect)
        {
            activeSlots[slotIndex].effectRune.Upgrade(rarity);
        }
        else
        {
            // Sinon on remplace (Nouveau départ avec boost rareté)
            float power = RarityUtils.GetPowerBoost(rarity);
            activeSlots[slotIndex].effectRune = new Rune(newEffect);
        }

        activeSlots[slotIndex].RecalculateStats();
        OnInventoryUpdated?.Invoke();
    }

    public bool TryApplyModifierToSlot(SpellModifier mod, int slotIndex, int replaceIndex, Rarity rarity)
    {
        if (slotIndex < 0 || slotIndex >= activeSlots.Count) return false;
        SpellSlot slot = activeSlots[slotIndex];

        if (mod.requiredTag != SpellTag.None && !slot.formRune.AsForm.tags.HasFlag(mod.requiredTag)) return false;

        // 1. Upgrade Existant (Si même mod présent)
        for (int i = 0; i < slot.modifierRunes.Length; i++)
        {
            if (slot.modifierRunes[i] != null && slot.modifierRunes[i].Data == mod)
            {
                slot.modifierRunes[i].Upgrade(rarity); // +1 Niveau, +Power selon rareté
                slot.RecalculateStats();
                OnInventoryUpdated?.Invoke();
                return true;
            }
        }

        // 2. Nouveau Mod (Slot vide)
        for (int i = 0; i < slot.modifierRunes.Length; i++)
        {
            if (slot.modifierRunes[i] == null || slot.modifierRunes[i].Data == null)
            {
                float power = RarityUtils.GetPowerBoost(rarity);
                slot.modifierRunes[i] = new Rune(mod);
                slot.RecalculateStats();
                OnInventoryUpdated?.Invoke();
                return true;
            }
        }

        // 3. Remplacement Forcé (Si replaceIndex est fourni par l'UI)
        if (replaceIndex != -1 && replaceIndex < slot.modifierRunes.Length)
        {
            float power = RarityUtils.GetPowerBoost(rarity);
            slot.modifierRunes[replaceIndex] = new Rune(mod);
            slot.RecalculateStats();
            OnInventoryUpdated?.Invoke();
            return true;
        }

        return false; // Inventaire plein, il faut demander le remplacement
    }

    // Cherche si le joueur possède déjà cette Rune (SO) active dans un slot
    // Retourne la Rune dynamique si trouvée, sinon null.
    public Rune FindActiveRune(RuneSO data)
    {
        foreach (var slot in activeSlots)
        {
            // Vérif Forme
            if (slot.formRune != null && slot.formRune.Data == data)
                return slot.formRune;

            // Vérif Effet
            if (slot.effectRune != null && slot.effectRune.Data == data)
                return slot.effectRune;

            // Vérif Modifiers
            foreach (var modRune in slot.modifierRunes)
            {
                if (modRune != null && modRune.Data == data)
                    return modRune;
            }
        }
        return null; // Pas trouvée (Nouveau)
    }

    public List<SpellSlot> GetSlots() => activeSlots;
}