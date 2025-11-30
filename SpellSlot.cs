using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpellSlot
{
    public Rune formRune;
    public Rune effectRune;
    public Rune[] modifierRunes = new Rune[2];

    private SpellDefinition _cachedDefinition;
    public SpellDefinition Definition
    {
        get
        {
            if (_cachedDefinition == null) RecalculateStats();
            return _cachedDefinition;
        }
    }

    [HideInInspector] public float currentCooldown;

    public void RecalculateStats()
    {
        // --- SÉCURITÉ ANTI-CRASH (Fix du TotalPower à 0) ---
        // Si une rune a été créée via l'Inspecteur avant l'ajout du champ TotalPower,
        // Unity l'a mis à 0. On force la valeur par défaut (1.0) ici.

        if (formRune != null && formRune.TotalPower <= 0.1f)
            formRune.TotalPower = 1.0f;

        if (effectRune != null && effectRune.TotalPower <= 0.1f)
            effectRune.TotalPower = 1.0f;

        if (modifierRunes != null)
        {
            foreach (var mod in modifierRunes)
            {
                if (mod != null && mod.Data != null && mod.TotalPower <= 0.1f)
                {
                    mod.TotalPower = 1.0f;
                }
            }
        }
        // -----------------------------------------------------

        if (formRune != null && formRune.AsForm != null &&
            effectRune != null && effectRune.AsEffect != null)
        {
            _cachedDefinition = SpellBuilder.Build(this);
        }
        else
        {
            _cachedDefinition = null;
        }
    }

    public void ForceInit() => RecalculateStats();

    // ... (Le reste de la classe TryAddModifier / ReplaceModifier reste inchangé)
    public bool TryAddModifier(Rune newModRune)
    {
        for (int i = 0; i < modifierRunes.Length; i++)
        {
            if (modifierRunes[i] == null || modifierRunes[i].Data == null)
            {
                modifierRunes[i] = newModRune;
                RecalculateStats();
                return true;
            }
        }
        return false;
    }

    public void ReplaceModifier(int index, Rune newModRune)
    {
        if (index >= 0 && index < modifierRunes.Length)
        {
            modifierRunes[index] = newModRune;
            RecalculateStats();
        }
    }
}