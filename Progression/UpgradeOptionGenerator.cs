using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates upgrade options with rarity weighting and filters.
/// </summary>
public class UpgradeOptionGenerator : MonoBehaviour
{
    private List<SpellForm> availableSpells;
    private List<SpellEffect> availableEffects;
    private List<SpellModifier> availableModifiers;
    private List<StatUpgradeSO> availableStats;

    public void Initialize(
        List<SpellForm> spells,
        List<SpellEffect> effects,
        List<SpellModifier> modifiers,
        List<StatUpgradeSO> stats)
    {
        availableSpells = spells;
        availableEffects = effects;
        availableModifiers = modifiers;
        availableStats = stats;
    }

    /// <summary>
    /// Generates a list of upgrade options with filtering and rarity weighting
    /// </summary>
    public List<UpgradeData> GenerateOptions(int count, Rarity minRarity, RewardFilter filter)
    {
        List<UpgradeData> picks = new List<UpgradeData>();
        int attempts = 0;

        while (picks.Count < count && attempts < 200)
        {
            attempts++;

            // 1. Weighted rarity with minimum
            Rarity rarity = RarityUtils.GetRandomRarityAtLeast(minRarity);

            // 2. Type filtering based on RewardFilter
            UpgradeType selectedType = SelectUpgradeType(filter);
            RuneSO selectedSO = SelectRuneByType(selectedType);

            if (selectedSO != null)
            {
                // Check if rune is banned
                if (LevelManager.Instance.IsRuneBanned(selectedSO.runeName))
                    continue;

                // Check for duplicates in current picks
                if (picks.Exists(x => x.TargetRuneSO == selectedSO))
                    continue;

                // CRITICAL: Check compatibility with player's current spell inventory
                if (!IsCompatibleWithPlayerInventory(selectedSO, selectedType))
                    continue;

                UpgradeData data = new UpgradeData(selectedSO, rarity);
                picks.Add(data);
            }
        }

        return picks;
    }

    private UpgradeType SelectUpgradeType(RewardFilter filter)
    {
        if (filter == RewardFilter.Form)
            return UpgradeType.NewSpell;
        if (filter == RewardFilter.Effect)
            return UpgradeType.Effect;
        if (filter == RewardFilter.Modifier)
            return UpgradeType.Modifier;
        if (filter == RewardFilter.Stat)
            return UpgradeType.StatBoost;

        // Random selection for "Any" filter
        float rand = Random.value;

        if (rand < 0.2f)
            return UpgradeType.NewSpell;
        else if (rand < 0.5f)
            return UpgradeType.Effect;
        else if (rand < 0.7f)
            return UpgradeType.Modifier;
        else
            return UpgradeType.StatBoost;
    }

    private RuneSO SelectRuneByType(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.NewSpell:
                if (availableSpells != null && availableSpells.Count > 0)
                    return availableSpells[Random.Range(0, availableSpells.Count)];
                break;

            case UpgradeType.Effect:
                if (availableEffects != null && availableEffects.Count > 0)
                    return availableEffects[Random.Range(0, availableEffects.Count)];
                break;

            case UpgradeType.Modifier:
                if (availableModifiers != null && availableModifiers.Count > 0)
                    return availableModifiers[Random.Range(0, availableModifiers.Count)];
                break;

            case UpgradeType.StatBoost:
                if (availableStats != null && availableStats.Count > 0)
                    return availableStats[Random.Range(0, availableStats.Count)];
                break;
        }

        return null;
    }

    /// <summary>
    /// Checks if the selected rune is compatible with at least one spell in the player's inventory.
    /// This prevents offering Effects/Modifiers that can't be used with any current spell.
    /// </summary>
    private bool IsCompatibleWithPlayerInventory(RuneSO selectedSO, UpgradeType type)
    {
        // Stat boosts and new spells are always compatible
        if (type == UpgradeType.StatBoost || type == UpgradeType.NewSpell)
            return true;

        // Get player's current spells
        SpellManager spellManager = FindFirstObjectByType<SpellManager>();
        if (spellManager == null)
            return true; // If no spell manager, don't filter (for safety)

        var slots = spellManager.GetSlots();
        if (slots == null || slots.Count == 0)
            return true; // If no spells yet, allow everything (player will get forms first)

        // For Effects: Check if compatible with at least one form in player's inventory
        if (type == UpgradeType.Effect)
        {
            SpellEffect effect = selectedSO as SpellEffect;
            if (effect == null)
                return false;

            foreach (var slot in slots)
            {
                if (slot.formRune != null && slot.formRune.AsForm != null)
                {
                    // Check tag-based compatibility
                    if (CompatibilityValidator.IsCompatible(slot.formRune.AsForm, effect))
                    {
                        // Also check prefab mapping compatibility if registry exists
                        if (SpellPrefabRegistry.Instance != null)
                        {
                            if (SpellPrefabRegistry.Instance.IsCompatible(slot.formRune.AsForm, effect))
                                return true;
                        }
                        else
                        {
                            // No registry, rely on tag-based compatibility only
                            return true;
                        }
                    }
                }
            }

            return false; // No compatible form found
        }

        // For Modifiers: Check if compatible with at least one form in player's inventory
        if (type == UpgradeType.Modifier)
        {
            SpellModifier modifier = selectedSO as SpellModifier;
            if (modifier == null)
                return false;

            foreach (var slot in slots)
            {
                if (slot.formRune != null && slot.formRune.AsForm != null)
                {
                    if (CompatibilityValidator.IsCompatible(slot.formRune.AsForm, modifier))
                        return true;
                }
            }

            return false; // No compatible form found
        }

        return true; // Unknown type, allow by default
    }
}
