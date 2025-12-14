using UnityEngine;

/// <summary>
/// Validates compatibility between Spells components (Form, Effect, Modifier)
/// </summary>
public static class CompatibilityValidator
{
    /// <summary>
    /// Checks if a Form is compatible with an Effect based on tags
    /// </summary>
    public static bool IsCompatible(SpellForm form, SpellEffect effect)
    {
        if (form == null || effect == null)
            return false;

        // If effect has no tag restrictions (None), it's compatible with everything
        if (effect.compatibleTags == SpellTag.None)
            return true;

        // Check if form has at least one tag that matches effect's requirements
        return (form.tags & effect.compatibleTags) != 0;
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
