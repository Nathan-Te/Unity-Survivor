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
        [Tooltip("Si vrai, cette combinaison est compatible et peut être proposée au joueur")]
        public bool isCompatible = true;
    }

    [Header("Mappings")]
    [SerializeField] private List<PrefabEntry> mappings = new List<PrefabEntry>();

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
    /// Checks if a (form, effect) combination is compatible and has a prefab
    /// </summary>
    public bool IsCompatible(SpellForm form, SpellEffect effect)
    {
        if (form == null || effect == null)
            return false;

        // Check explicit mapping
        foreach (var entry in mappings)
        {
            if (entry.form == form && entry.effect == effect)
            {
                return entry.isCompatible && entry.prefab != null;
            }
        }

        // If no explicit mapping, check if form has a default prefab
        // This allows backward compatibility
        return form.prefab != null;
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
            if (entry.form == form && entry.isCompatible && entry.prefab != null)
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
            if (entry.effect == effect && entry.isCompatible && entry.prefab != null)
            {
                if (!compatible.Contains(entry.form))
                    compatible.Add(entry.form);
            }
        }

        return compatible;
    }
}
