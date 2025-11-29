using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpellSlot
{
    // On remplace les types directs par la classe Rune
    public Rune formRune;
    public Rune effectRune;

    // Liste fixe de 2 modificateurs (null si vide)
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
        // On vérifie que les runes existent et contiennent les bonnes données
        if (formRune != null && formRune.AsForm != null &&
            effectRune != null && effectRune.AsEffect != null)
        {
            _cachedDefinition = SpellBuilder.Build(this); // On passe 'this' (le slot entier) au Builder
        }
        else
        {
            _cachedDefinition = null;
        }
    }

    public void ForceInit() => RecalculateStats();

    // --- GESTION DES MODIFICATEURS ---

    public bool TryAddModifier(Rune newModRune)
    {
        // 1. Chercher un slot vide
        for (int i = 0; i < modifierRunes.Length; i++)
        {
            if (modifierRunes[i] == null)
            {
                modifierRunes[i] = newModRune;
                RecalculateStats();
                return true;
            }
        }
        return false; // Pas de place
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