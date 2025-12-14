using UnityEngine;

/// <summary>
/// Validates compatibility between Spells components (Form, Effect, Modifier)
/// </summary>
public static class CompatibilityValidator
{
    /// <summary>
    /// Checks if a Form is compatible with an Effect using the prefab mapping registry
    /// </summary>
    public static bool IsCompatible(SpellForm form, SpellEffect effect)
    {
        if (form == null || effect == null)
            return false;

        // Use the SpellPrefabRegistry as the single source of truth
        if (SpellPrefabRegistry.Instance != null)
        {
            return SpellPrefabRegistry.Instance.IsCompatible(form, effect);
        }

        // Fallback if no registry exists (shouldn't happen in normal gameplay)
        Debug.LogWarning("[CompatibilityValidator] No SpellPrefabRegistry found! Cannot validate compatibility.");
        return false;
    }

    /// <summary>
    /// Checks if a Form is compatible with a Modifier based on required tags
    /// </summary>
    public static bool IsCompatible(SpellForm form, SpellModifier modifier)
    {
        if (form == null || modifier == null)
            return false;

        // If modifier has no tag requirement (None), it's compatible with everything
        if (modifier.requiredTag == SpellTag.None)
            return true;

        // Check if form has the required tag
        return (form.tags & modifier.requiredTag) != 0;
    }

    /// <summary>
    /// Checks if a Form, Effect, and Modifier combination is fully compatible
    /// </summary>
    public static bool IsCompatible(SpellForm form, SpellEffect effect, SpellModifier modifier)
    {
        return IsCompatible(form, effect) && IsCompatible(form, modifier);
    }
}
