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

                // Check if rune is at max level
                if (IsRuneAtMaxLevel(selectedSO, selectedType))
                    continue;

                // Check if stat type limit is reached
                if (selectedType == UpgradeType.StatBoost && IsStatTypeLimitReached(selectedSO as StatUpgradeSO))
                    continue;

                // Check build lock system (only allow owned runes if build is full)
                if (IsBuildLocked() && !PlayerOwnsRune(selectedSO, selectedType))
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
                    // Check compatibility using the mapping (single source of truth)
                    if (CompatibilityValidator.IsCompatible(slot.formRune.AsForm, effect))
                        return true;
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

    /// <summary>
    /// Checks if the player already has this rune and if it's at max level
    /// </summary>
    private bool IsRuneAtMaxLevel(RuneSO selectedSO, UpgradeType type)
    {
        // No max level defined means unlimited
        if (selectedSO.GetMaxLevel() <= 0)
            return false;

        // Stat boosts don't have a concept of "player's rune" (they're applied globally)
        if (type == UpgradeType.StatBoost)
            return false;

        // Get player's current spells
        SpellManager spellManager = FindFirstObjectByType<SpellManager>();
        if (spellManager == null)
            return false;

        var slots = spellManager.GetSlots();
        if (slots == null || slots.Count == 0)
            return false;

        // For Forms: Check if ALL instances of this form are at max level AND no room for new instances
        if (type == UpgradeType.NewSpell)
        {
            SpellForm form = selectedSO as SpellForm;
            if (form == null)
                return false;

            bool hasForm = false;
            bool allMaxed = true;

            foreach (var slot in slots)
            {
                if (slot.formRune != null && slot.formRune.Data == form)
                {
                    hasForm = true;
                    // If ANY instance is NOT at max level, we can still offer this rune
                    if (!selectedSO.IsMaxLevel(slot.formRune.Level))
                    {
                        allMaxed = false;
                    }
                }
            }

            // If player has empty slots, they can always get this form (even if existing instances are maxed)
            bool hasEmptySlots = slots.Count < spellManager.MaxSlots;
            if (hasEmptySlots)
                return false; // Not blocked - player can add a new instance

            // Only block if player has this form AND all instances are maxed AND no empty slots
            return hasForm && allMaxed;
        }

        // For Effects: Check if ALL slots have this effect at max level (no room to apply/upgrade)
        if (type == UpgradeType.Effect)
        {
            SpellEffect effect = selectedSO as SpellEffect;
            if (effect == null)
                return false;

            int slotsWithEffectAtMax = 0;
            int totalSlots = slots.Count;

            foreach (var slot in slots)
            {
                // Check if this slot has the effect at max level
                if (slot.effectRune != null && slot.effectRune.Data == effect)
                {
                    if (selectedSO.IsMaxLevel(slot.effectRune.Level))
                    {
                        slotsWithEffectAtMax++;
                    }
                    // If this slot has the effect but NOT at max, we can upgrade it
                    else
                    {
                        return false; // Not blocked - can upgrade this slot
                    }
                }
                // If slot doesn't have this effect, we could potentially apply it there
                // (compatibility will be checked separately)
            }

            // Only block if ALL slots have this effect at max level
            return slotsWithEffectAtMax == totalSlots && totalSlots > 0;
        }

        // For Modifiers: Check if there's any room to add or upgrade this modifier
        if (type == UpgradeType.Modifier)
        {
            SpellModifier modifier = selectedSO as SpellModifier;
            if (modifier == null)
                return false;

            int modifierSlotsAtMax = 0;
            int totalModifierSlots = 0;
            int emptyModifierSlots = 0;

            foreach (var slot in slots)
            {
                if (slot.modifierRunes != null)
                {
                    totalModifierSlots += slot.modifierRunes.Length;

                    foreach (var modRune in slot.modifierRunes)
                    {
                        if (modRune == null || modRune.Data == null)
                        {
                            emptyModifierSlots++;
                        }
                        else if (modRune.Data == modifier)
                        {
                            if (selectedSO.IsMaxLevel(modRune.Level))
                            {
                                modifierSlotsAtMax++;
                            }
                            else
                            {
                                // Found an instance that's NOT at max - can upgrade it
                                return false;
                            }
                        }
                    }
                }
            }

            // If there are empty modifier slots, we can add this modifier there
            if (emptyModifierSlots > 0)
                return false;

            // Only block if ALL modifier slots have this modifier at max level
            return modifierSlotsAtMax == totalModifierSlots && totalModifierSlots > 0;
        }

        return false;
    }

    /// <summary>
    /// Checks if the player has reached the maximum number of different stat types
    /// </summary>
    private bool IsStatTypeLimitReached(StatUpgradeSO statSO)
    {
        if (statSO == null)
            return false;

        // Check if there's a limit configured
        if (RuneMaxLevelConfig.Instance == null || RuneMaxLevelConfig.Instance.maxStatTypes <= 0)
            return false; // No limit

        // If player already has this stat type, allow it (they can upgrade it)
        if (PlayerStats.Instance != null && PlayerStats.Instance.HasStatType(statSO.targetStat))
            return false;

        // Check if player has reached the limit
        if (PlayerStats.Instance != null)
        {
            int currentStatTypeCount = PlayerStats.Instance.GetAcquiredStatTypeCount();
            return currentStatTypeCount >= RuneMaxLevelConfig.Instance.maxStatTypes;
        }

        return false;
    }

    /// <summary>
    /// Checks if the player's build is "locked" (fully built) and should only receive upgrades for owned runes
    /// </summary>
    private bool IsBuildLocked()
    {
        // Check if build lock is enabled in config
        if (RuneMaxLevelConfig.Instance == null || !RuneMaxLevelConfig.Instance.lockBuildWhenFull)
            return false;

        SpellManager spellManager = FindFirstObjectByType<SpellManager>();
        if (spellManager == null)
            return false;

        var slots = spellManager.GetSlots();
        if (slots == null || slots.Count == 0)
            return false;

        // Check 1: All spell slots are filled
        bool allSlotsFilled = slots.Count >= spellManager.MaxSlots;
        if (!allSlotsFilled)
            return false;

        // Check 2: All modifier slots are filled (2 per spell)
        foreach (var slot in slots)
        {
            if (slot.modifierRunes == null || slot.modifierRunes.Length < 2)
                return false;

            for (int i = 0; i < slot.modifierRunes.Length; i++)
            {
                if (slot.modifierRunes[i] == null || slot.modifierRunes[i].Data == null)
                    return false;
            }
        }

        // Check 3: Stat type limit is reached (if configured)
        if (RuneMaxLevelConfig.Instance.maxStatTypes > 0 && PlayerStats.Instance != null)
        {
            int currentStatTypes = PlayerStats.Instance.GetAcquiredStatTypeCount();
            if (currentStatTypes < RuneMaxLevelConfig.Instance.maxStatTypes)
                return false; // Still room for more stat types
        }

        // All conditions met - build is locked
        return true;
    }

    /// <summary>
    /// Checks if the player already owns this rune (for build lock purposes)
    /// </summary>
    private bool PlayerOwnsRune(RuneSO selectedSO, UpgradeType type)
    {
        // Stats: Check if player has this stat type
        if (type == UpgradeType.StatBoost)
        {
            StatUpgradeSO statSO = selectedSO as StatUpgradeSO;
            if (statSO != null && PlayerStats.Instance != null)
            {
                return PlayerStats.Instance.HasStatType(statSO.targetStat);
            }
            return false;
        }

        // Forms/Effects/Modifiers: Check spell inventory
        SpellManager spellManager = FindFirstObjectByType<SpellManager>();
        if (spellManager == null)
            return false;

        var slots = spellManager.GetSlots();
        if (slots == null || slots.Count == 0)
            return false;

        // Forms
        if (type == UpgradeType.NewSpell)
        {
            SpellForm form = selectedSO as SpellForm;
            if (form == null)
                return false;

            foreach (var slot in slots)
            {
                if (slot.formRune != null && slot.formRune.Data == form)
                    return true;
            }
            return false;
        }

        // Effects
        if (type == UpgradeType.Effect)
        {
            SpellEffect effect = selectedSO as SpellEffect;
            if (effect == null)
                return false;

            foreach (var slot in slots)
            {
                if (slot.effectRune != null && slot.effectRune.Data == effect)
                    return true;
            }
            return false;
        }

        // Modifiers
        if (type == UpgradeType.Modifier)
        {
            SpellModifier modifier = selectedSO as SpellModifier;
            if (modifier == null)
                return false;

            foreach (var slot in slots)
            {
                if (slot.modifierRunes != null)
                {
                    foreach (var modRune in slot.modifierRunes)
                    {
                        if (modRune != null && modRune.Data == modifier)
                            return true;
                    }
                }
            }
            return false;
        }

        return false;
    }
}
