using System;
using System.Collections.Generic;
using UnityEngine;

public class SpellManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private int maxSpellSlots = 4; // LIMITE MAX

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

    public bool CanAddSpell() => activeSlots.Count < maxSpellSlots;

    public void AddSpell(SpellForm form, Rarity rarity = Rarity.Common)
    {
        SpellSlot newSlot = new SpellSlot();

        // Le niveau initial dépend de la rareté de la carte "Nouveau Sort"
        // Commune = Lvl 1, Légendaire = Lvl 5 direct
        int startLevel = RarityUtils.GetLevelBoost(rarity);

        newSlot.formRune = new Rune(form, startLevel);

        var defaultEffect = Resources.Load<SpellEffect>("Spells/Effects/Physical");
        // L'effet par défaut commence niveau 1 (ou match le niveau du sort ?)
        // Disons niveau 1 pour l'instant.
        newSlot.effectRune = new Rune(defaultEffect, 1);

        newSlot.ForceInit();
        activeSlots.Add(newSlot);
        OnInventoryUpdated?.Invoke();
    }

    // Remplacement d'effet (Upgrade)
    public void ReplaceEffect(SpellEffect newEffect, int slotIndex, Rarity rarity)
    {
        if (slotIndex < 0 || slotIndex >= activeSlots.Count) return;
        SpellSlot slot = activeSlots[slotIndex];

        // Si on remplace, on peut décider de garder le niveau d'avant ou reset
        // Ici : On reset, mais on applique le boost de la nouvelle carte
        int startLevel = RarityUtils.GetLevelBoost(rarity);

        slot.effectRune = new Rune(newEffect, startLevel);
        slot.RecalculateStats();
        OnInventoryUpdated?.Invoke();
    }

    // --- NOUVEAU : Remplacer un Sort complet (Quand inventaire plein) ---
    public void ReplaceSpell(SpellForm newForm, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= activeSlots.Count) return;

        SpellSlot slot = activeSlots[slotIndex];

        // 1. Nouvelle Forme
        slot.formRune = new Rune(newForm, 1);

        // 2. Reset de l'Effet (On remet Physique par défaut pour éviter les incohérences)
        // Ou tu peux décider de garder l'effet actuel : slot.effectRune = slot.effectRune;
        // Ici, je reset pour faire propre comme un "Nouveau Sort"
        var defaultEffect = Resources.Load<SpellEffect>("Spells/Effects/Physical");
        if (defaultEffect == null) defaultEffect = ScriptableObject.CreateInstance<SpellEffect>();
        slot.effectRune = new Rune(defaultEffect, 1);

        // 3. Reset des Modificateurs (Pour éviter les incompatibilités avec la nouvelle forme)
        slot.modifierRunes = new Rune[2];

        slot.RecalculateStats();
        slot.currentCooldown = 0.5f; // Petit délai de sécurité

        OnInventoryUpdated?.Invoke();
    }

    public bool TryApplyModifierToSlot(SpellModifier mod, int slotIndex, int replaceIndex = -1, Rarity rarity = Rarity.Common)
    {
        if (slotIndex < 0 || slotIndex >= activeSlots.Count) return false;
        SpellSlot slot = activeSlots[slotIndex];

        // 1. Vérif Compatibilité
        if (mod.requiredTag != SpellTag.None && !slot.formRune.AsForm.tags.HasFlag(mod.requiredTag)) return false;

        int boostAmount = RarityUtils.GetLevelBoost(rarity);

        // 2. Upgrade Existant
        for (int i = 0; i < slot.modifierRunes.Length; i++)
        {
            if (slot.modifierRunes[i] != null && slot.modifierRunes[i].Data == mod)
            {
                // ON AJOUTE LE BOOST AU LIEU DE JUSTE +1
                slot.modifierRunes[i].IncreaseLevel(boostAmount);
                slot.RecalculateStats();
                OnInventoryUpdated?.Invoke();
                return true;
            }
        }

        // 3. Ajout Nouveau
        for (int i = 0; i < slot.modifierRunes.Length; i++)
        {
            if (slot.modifierRunes[i] == null || slot.modifierRunes[i].Data == null)
            {
                slot.modifierRunes[i] = new Rune(mod, boostAmount); // Commence niveau 1, 2, 3 ou 5
                slot.RecalculateStats();
                OnInventoryUpdated?.Invoke();
                return true;
            }
        }

        // 4. Remplacement
        if (replaceIndex != -1)
        {
            slot.modifierRunes[replaceIndex] = new Rune(mod, boostAmount);
            slot.RecalculateStats();
            OnInventoryUpdated?.Invoke();
            return true;
        }

        return false;
    }

    public List<SpellSlot> GetSlots() => activeSlots;
}