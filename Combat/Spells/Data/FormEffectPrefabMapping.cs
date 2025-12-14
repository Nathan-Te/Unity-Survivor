using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Maps (SpellForm, SpellEffect) combinations to their specific prefabs and VFX.
/// For example: (Bolt, Fire) -> FireBoltPrefab, (Bolt, Lightning) -> LightningBoltPrefab
/// </summary>
[CreateAssetMenu(menuName = "Spells/Form-Effect Prefab Mapping")]
public class FormEffectPrefabMapping : ScriptableObject
{
    [System.Serializable]
    public class PrefabEntry
    {
        public SpellForm form;
        public SpellEffect effect;
        public GameObject prefab;
    }

    [Header("Mappings")]
    [SerializeField] private List<PrefabEntry> mappings = new List<PrefabEntry>();

    public List<PrefabEntry> PrefabMappings => mappings;

    /// <summary>
    /// Returns the prefab for a given (form, effect) combination.
    /// Falls back to form.prefab if no specific mapping exists.
    /// </summary>
    public GameObject GetPrefab(SpellForm form, SpellEffect effect)
    {
        if (form == null || effect == null)
            return null;

        // Try to find specific mapping
        foreach (var entry in mappings)
        {
            if (entry.form == form && entry.effect == effect && entry.prefab != null)
            {
                return entry.prefab;
            }
        }

        // Fallback to form's default prefab
        return form.prefab;
    }

    /// <summary>
    /// Checks if a (form, effect) combination is compatible.
    /// A combination is compatible if it exists in the mapping with a valid prefab.
    /// </summary>
    public bool IsCompatible(SpellForm form, SpellEffect effect)
    {
        if (form == null || effect == null)
            return false;

        // Check if this combination exists in the mapping
        foreach (var entry in mappings)
        {
            if (entry.form == form && entry.effect == effect)
            {
                return entry.prefab != null;
            }
        }

        // Not in mapping = not compatible
        return false;
    }

    /// <summary>
    /// Returns all effects compatible with a given form
    /// </summary>
    public List<SpellEffect> GetCompatibleEffects(SpellForm form)
    {
        List<SpellEffect> compatible = new List<SpellEffect>();

        if (form == null)
            return compatible;

        foreach (var entry in mappings)
        {
            if (entry.form == form && entry.prefab != null)
            {
                if (!compatible.Contains(entry.effect))
                    compatible.Add(entry.effect);
            }
        }

        return compatible;
    }

    /// <summary>
    /// Returns all forms compatible with a given effect
    /// </summary>
    public List<SpellForm> GetCompatibleForms(SpellEffect effect)
    {
        List<SpellForm> compatible = new List<SpellForm>();

        if (effect == null)
            return compatible;

        foreach (var entry in mappings)
        {
            if (entry.effect == effect && entry.prefab != null)
            {
                if (!compatible.Contains(entry.form))
                    compatible.Add(entry.form);
            }
        }

        return compatible;
    }
}
