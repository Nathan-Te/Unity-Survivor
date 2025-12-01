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
        // On vérifie juste que les références existent
        if (formRune != null && formRune.Data != null &&
            effectRune != null && effectRune.Data != null)
        {
            _cachedDefinition = SpellBuilder.Build(this);
        }
        else
        {
            _cachedDefinition = null;
        }
    }

    public void ForceInit() => RecalculateStats();

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