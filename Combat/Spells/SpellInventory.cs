using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages spell inventory - adding, upgrading, and replacing spells in active slots.
/// </summary>
public class SpellInventory : MonoBehaviour
{
    private int maxSpellSlots = 4;
    private List<SpellSlot> activeSlots = new List<SpellSlot>();
    private SpellEffect defaultEffectAsset;

    public int MaxSlots => maxSpellSlots;
    public List<SpellSlot> GetSlots() => activeSlots;

    public event Action OnInventoryUpdated;

    /// <summary>
    /// Initializes the inventory with references from SpellManager
    /// </summary>
    public void Initialize(int maxSlots, List<SpellSlot> slots, SpellEffect defaultEffect)
    {
        maxSpellSlots = maxSlots;
        activeSlots = slots;
        defaultEffectAsset = defaultEffect;
    }

    /// <summary>
    /// Initializes all active slots
    /// </summary>
    public void InitializeSlots()
    {
        foreach (var slot in activeSlots)
        {
            slot.ForceInit();
            slot.currentCooldown = 0.5f;
        }
        OnInventoryUpdated?.Invoke();
    }

    public bool CanAddSpell() => activeSlots.Count < maxSpellSlots;

    public bool HasForm(SpellForm form)
    {
        foreach (var slot in activeSlots)
        {
            if (slot.formRune != null && slot.formRune.Data == form)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Adds a new spell to inventory with initial upgrade stats
    /// </summary>
    public void AddNewSpellWithUpgrade(SpellForm form, RuneDefinition upgradeDef)
    {
        if (!CanAddSpell()) return;

        SpellSlot newSlot = new SpellSlot();

        newSlot.formRune = new Rune(form);
        newSlot.formRune.InitializeWithStats(upgradeDef);

        SpellEffect effectToUse = defaultEffectAsset != null
            ? defaultEffectAsset
            : ScriptableObject.CreateInstance<SpellEffect>();
        newSlot.effectRune = new Rune(effectToUse);

        newSlot.ForceInit();
        activeSlots.Add(newSlot);
        OnInventoryUpdated?.Invoke();
    }

    /// <summary>
    /// Upgrades an existing spell form
    /// </summary>
    public void UpgradeExistingForm(SpellForm form, RuneDefinition upgradeDef)
    {
        foreach (var slot in activeSlots)
        {
            if (slot.formRune != null && slot.formRune.Data == form)
            {
                slot.formRune.ApplyUpgrade(upgradeDef);
                slot.RecalculateStats();
                OnInventoryUpdated?.Invoke();
                return;
            }
        }
    }

    /// <summary>
    /// Replaces a spell at a specific slot index
    /// </summary>
    public void ReplaceSpell(SpellForm newForm, int slotIndex, RuneDefinition upgradeDef)
    {
        if (slotIndex < 0 || slotIndex >= activeSlots.Count) return;

        SpellSlot slot = activeSlots[slotIndex];

        slot.formRune = new Rune(newForm);
        slot.formRune.InitializeWithStats(upgradeDef);

        SpellEffect effectToUse = defaultEffectAsset != null
            ? defaultEffectAsset
            : ScriptableObject.CreateInstance<SpellEffect>();
        slot.effectRune = new Rune(effectToUse);

        slot.modifierRunes = new Rune[2]; // Reset modifiers

        slot.RecalculateStats();
        slot.currentCooldown = 0.5f;
        OnInventoryUpdated?.Invoke();
    }

    /// <summary>
    /// Applies effect to a slot (adds new or upgrades existing)
    /// </summary>
    public void ApplyEffectToSlot(SpellEffect effectSO, int slotIndex, RuneDefinition upgradeDef)
    {
        if (slotIndex < 0 || slotIndex >= activeSlots.Count) return;

        SpellSlot slot = activeSlots[slotIndex];

        // If same effect, upgrade it
        if (slot.effectRune.Data == effectSO)
        {
            slot.effectRune.ApplyUpgrade(upgradeDef);
        }
        else
        {
            // If new effect, replace and apply upgrade
            slot.effectRune = new Rune(effectSO);
            slot.effectRune.InitializeWithStats(upgradeDef);
        }

        slot.RecalculateStats();
        OnInventoryUpdated?.Invoke();
    }

    /// <summary>
    /// Tries to apply a modifier to a spell slot
    /// </summary>
    public bool TryApplyModifierToSlot(SpellModifier mod, int slotIndex, int replaceIndex, RuneDefinition upgradeDef)
    {
        if (slotIndex < 0 || slotIndex >= activeSlots.Count) return false;

        SpellSlot slot = activeSlots[slotIndex];

        // Initialize modifier array if null (can happen with Inspector-configured slots)
        if (slot.modifierRunes == null)
        {
            slot.modifierRunes = new Rune[2];
        }

        // Check if modifier requires specific spell tag
        if (mod.requiredTag != SpellTag.None && !slot.formRune.AsForm.tags.HasFlag(mod.requiredTag))
            return false;

        // A. Upgrade existing modifier
        for (int i = 0; i < slot.modifierRunes.Length; i++)
        {
            if (slot.modifierRunes[i] != null && slot.modifierRunes[i].Data == mod)
            {
                slot.modifierRunes[i].ApplyUpgrade(upgradeDef);
                slot.RecalculateStats();
                OnInventoryUpdated?.Invoke();
                return true;
            }
        }

        // B. Add new modifier to empty slot
        for (int i = 0; i < slot.modifierRunes.Length; i++)
        {
            if (slot.modifierRunes[i] == null || slot.modifierRunes[i].Data == null)
            {
                slot.modifierRunes[i] = new Rune(mod);
                slot.modifierRunes[i].InitializeWithStats(upgradeDef);
                slot.RecalculateStats();
                OnInventoryUpdated?.Invoke();
                return true;
            }
        }

        // C. Force replace modifier at specific index
        if (replaceIndex != -1 && replaceIndex < slot.modifierRunes.Length)
        {
            slot.modifierRunes[replaceIndex] = new Rune(mod);
            slot.modifierRunes[replaceIndex].InitializeWithStats(upgradeDef);
            slot.RecalculateStats();
            OnInventoryUpdated?.Invoke();
            return true;
        }

        return false; // Inventory full
    }

    /// <summary>
    /// Upgrades a spell at a specific slot
    /// </summary>
    public void UpgradeSpellAtSlot(int slotIndex, RuneDefinition upgradeDef)
    {
        if (slotIndex < 0 || slotIndex >= activeSlots.Count) return;

        SpellSlot slot = activeSlots[slotIndex];

        if (slot.formRune != null)
        {
            slot.formRune.ApplyUpgrade(upgradeDef);
            slot.RecalculateStats();
            OnInventoryUpdated?.Invoke();
        }
    }
}
