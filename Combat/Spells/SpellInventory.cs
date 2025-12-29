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
    /// Event fired when an incompatible action is attempted. UI should display this message to the user.
    /// </summary>
    public static event Action<string> OnIncompatibilityWarning;

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
            // CRITICAL: Ensure runes preserve their AccumulatedStats from Inspector configuration
            // This fixes the issue where FlatCount, FlatMulticast, and other stats set in the
            // Inspector were being reset to Zero on game start
            EnsureRuneStatsPreserved(slot);

            slot.ForceInit();
            slot.currentCooldown = 0.5f;
        }
        OnInventoryUpdated?.Invoke();
    }

    /// <summary>
    /// Ensures that runes configured in the Inspector preserve their AccumulatedStats.
    /// Without this, all AccumulatedStats would be reset to Zero on game start.
    /// </summary>
    private void EnsureRuneStatsPreserved(SpellSlot slot)
    {
        // Form rune
        if (slot.formRune != null && slot.formRune.Data != null)
        {
            // If AccumulatedStats is non-zero, preserve it (Inspector-configured)
            // Otherwise, initialize to Zero (will be set by upgrade cards during gameplay)
            if (!IsRuneStatsZero(slot.formRune.AccumulatedStats))
            {
                // Already has stats from Inspector - keep them
            }
            else
            {
                // No stats configured - initialize to Zero (or Modifier baseStats)
                if (slot.formRune.Data is SpellModifier mod)
                {
                    slot.formRune.AccumulatedStats = mod.baseStats;
                }
                else
                {
                    slot.formRune.AccumulatedStats = RuneStats.Zero;
                }
            }
        }

        // Effect rune
        if (slot.effectRune != null && slot.effectRune.Data != null)
        {
            if (!IsRuneStatsZero(slot.effectRune.AccumulatedStats))
            {
                // Already has stats from Inspector - keep them
            }
            else
            {
                if (slot.effectRune.Data is SpellModifier mod)
                {
                    slot.effectRune.AccumulatedStats = mod.baseStats;
                }
                else
                {
                    slot.effectRune.AccumulatedStats = RuneStats.Zero;
                }
            }
        }

        // Modifier runes
        if (slot.modifierRunes != null)
        {
            for (int i = 0; i < slot.modifierRunes.Length; i++)
            {
                if (slot.modifierRunes[i] != null && slot.modifierRunes[i].Data != null)
                {
                    if (!IsRuneStatsZero(slot.modifierRunes[i].AccumulatedStats))
                    {
                        // Already has stats from Inspector - keep them
                    }
                    else
                    {
                        if (slot.modifierRunes[i].Data is SpellModifier mod)
                        {
                            slot.modifierRunes[i].AccumulatedStats = mod.baseStats;
                        }
                        else
                        {
                            slot.modifierRunes[i].AccumulatedStats = RuneStats.Zero;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Checks if a RuneStats struct is effectively zero (all fields are default/zero).
    /// Used to detect if AccumulatedStats was configured in the Inspector.
    /// </summary>
    private bool IsRuneStatsZero(RuneStats stats)
    {
        return stats.DamageMult == 0f &&
               stats.CooldownMult == 0f &&
               stats.SizeMult == 0f &&
               stats.SpeedMult == 0f &&
               stats.DurationMult == 0f &&
               stats.FlatCooldown == 0f &&
               stats.FlatCount == 0 &&
               stats.FlatPierce == 0 &&
               stats.FlatSpread == 0f &&
               stats.FlatRange == 0f &&
               stats.FlatKnockback == 0f &&
               stats.FlatChainCount == 0 &&
               stats.FlatMulticast == 0 &&
               stats.FlatBurnDamage == 0f &&
               stats.FlatBurnDuration == 0f &&
               stats.FlatCritChance == 0f &&
               stats.FlatCritDamage == 0f &&
               stats.StatValue == 0f;
    }

    /// <summary>
    /// Recalculates stats for all active spell slots (used when global player stats change)
    /// </summary>
    public void RecalculateAllSlots()
    {
        foreach (var slot in activeSlots)
        {
            slot.RecalculateStats();
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

        // Get a compatible effect from the mapping
        SpellEffect effectToUse = GetCompatibleEffect(form);
        if (effectToUse == null)
        {
            OnIncompatibilityWarning?.Invoke($"Impossible d'ajouter '{form.GetLocalizedName()}' : aucun effet compatible trouvé !");
            return;
        }

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

        // Clean up active projectiles from the old spell (especially important for Orbits)
        if (slot.formRune != null && slot.formRune.AsForm != null && ProjectilePool.Instance != null)
        {
            ProjectilePool.Instance.DespawnProjectilesWithForm(slot.formRune.AsForm);
        }

        slot.formRune = new Rune(newForm);
        slot.formRune.InitializeWithStats(upgradeDef);

        // Get a compatible effect from the mapping
        SpellEffect effectToUse = GetCompatibleEffect(newForm);
        if (effectToUse == null)
        {
            OnIncompatibilityWarning?.Invoke($"Impossible de remplacer par '{newForm.GetLocalizedName()}' : aucun effet compatible trouvé !");
            return;
        }

        slot.effectRune = new Rune(effectToUse);

        slot.modifierRunes = new Rune[2]; // Reset modifiers

        slot.RecalculateStats();
        slot.currentCooldown = 0.5f;
        OnInventoryUpdated?.Invoke();
    }

    /// <summary>
    /// Applies effect to a slot (adds new or upgrades existing)
    /// </summary>
    /// <returns>True if the effect was successfully applied, false otherwise</returns>
    public bool ApplyEffectToSlot(SpellEffect effectSO, int slotIndex, RuneDefinition upgradeDef)
    {
        if (slotIndex < 0 || slotIndex >= activeSlots.Count) return false;

        SpellSlot slot = activeSlots[slotIndex];

        // CRITICAL: Check compatibility before allowing effect assignment
        if (slot.formRune != null && slot.formRune.AsForm != null)
        {
            if (!CompatibilityValidator.IsCompatible(slot.formRune.AsForm, effectSO))
            {
                // Notify the UI to display a warning to the player
                OnIncompatibilityWarning?.Invoke($"L'effet '{effectSO.GetLocalizedName()}' n'est pas compatible avec '{slot.formRune.AsForm.GetLocalizedName()}' !");
                return false;
            }
        }

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
        return true;
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

    /// <summary>
    /// Gets the first compatible effect for a given form from the mapping.
    /// Returns the default effect asset if compatible, otherwise the first compatible effect found.
    /// </summary>
    private SpellEffect GetCompatibleEffect(SpellForm form)
    {
        if (form == null)
            return null;

        if (SpellPrefabRegistry.Instance == null || SpellPrefabRegistry.Instance.PrefabMapping == null)
        {
            Debug.LogWarning("[SpellInventory] No SpellPrefabRegistry or mapping found!");
            return defaultEffectAsset; // Fallback to default
        }

        // Try to use the default effect if it's compatible
        if (defaultEffectAsset != null && CompatibilityValidator.IsCompatible(form, defaultEffectAsset))
        {
            return defaultEffectAsset;
        }

        // Otherwise, get the first compatible effect from the mapping
        var compatibleEffects = SpellPrefabRegistry.Instance.PrefabMapping.GetCompatibleEffects(form);
        if (compatibleEffects != null && compatibleEffects.Count > 0)
        {
            return compatibleEffects[0];
        }

        Debug.LogError($"[SpellInventory] No compatible effect found for form {form.GetLocalizedName()} in mapping!");
        return null;
    }
}
