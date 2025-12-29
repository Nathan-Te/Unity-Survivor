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
        public GameObject impactVfxPrefab; // VFX spawned on hit/impact

        [Header("Smite Timing (Only for Smite spells)")]
        [Tooltip("Delay before explosion/damage occurs")]
        public float impactDelay = 0f;
        [Tooltip("Delay before spawning impact VFX (usually same as impactDelay)")]
        public float vfxSpawnDelay = 0f;
        [Tooltip("How long the smite prefab remains visible after explosion")]
        public float smiteLifetime = 2f;

        [Header("Audio Settings")]
        [Tooltip("Sound played when casting this spell")]
        public AudioClip castSound;
        [Tooltip("Volume for cast sound (0-1)")]
        [Range(0f, 1f)] public float castVolume = 1f;

        [Tooltip("Sound played on impact/hit")]
        public AudioClip impactSound;
        [Tooltip("Volume for impact sound (0-1)")]
        [Range(0f, 1f)] public float impactVolume = 1f;
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
    /// Returns the impact VFX prefab for a given (form, effect) combination.
    /// Returns null if no VFX is configured for this combination.
    /// </summary>
    public GameObject GetImpactVfx(SpellForm form, SpellEffect effect)
    {
        if (form == null || effect == null)
            return null;

        // Find the mapping entry
        foreach (var entry in mappings)
        {
            if (entry.form == form && entry.effect == effect)
            {
                return entry.impactVfxPrefab;
            }
        }

        return null;
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

    /// <summary>
    /// Returns Smite timing configuration for a (form, effect) combination
    /// </summary>
    public (float impactDelay, float vfxSpawnDelay, float lifetime) GetSmiteTiming(SpellForm form, SpellEffect effect)
    {
        if (form == null || effect == null)
            return (0f, 0f, 2f); // Default values

        foreach (var entry in mappings)
        {
            if (entry.form == form && entry.effect == effect)
            {
                return (entry.impactDelay, entry.vfxSpawnDelay, entry.smiteLifetime);
            }
        }

        // Return defaults if not found
        return (0f, 0f, 2f);
    }

    /// <summary>
    /// Returns cast sound and volume for a (form, effect) combination
    /// </summary>
    public (AudioClip clip, float volume) GetCastSound(SpellForm form, SpellEffect effect)
    {
        if (form == null || effect == null)
            return (null, 1f);

        foreach (var entry in mappings)
        {
            if (entry.form == form && entry.effect == effect)
            {
                return (entry.castSound, entry.castVolume);
            }
        }

        return (null, 1f);
    }

    /// <summary>
    /// Returns impact sound and volume for a (form, effect) combination
    /// </summary>
    public (AudioClip clip, float volume) GetImpactSound(SpellForm form, SpellEffect effect)
    {
        if (form == null || effect == null)
            return (null, 1f);

        foreach (var entry in mappings)
        {
            if (entry.form == form && entry.effect == effect)
            {
                return (entry.impactSound, entry.impactVolume);
            }
        }

        return (null, 1f);
    }
}
