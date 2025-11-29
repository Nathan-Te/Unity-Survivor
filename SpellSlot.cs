using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpellSlot
{
    // Les composants ("Le Deck")
    public SpellForm form;
    public SpellEffect effect;
    public List<SpellModifier> modifiers = new List<SpellModifier>();

    // Le résultat mis en cache (pour ne pas recalculer à chaque frame)
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
        if (form != null && effect != null)
        {
            _cachedDefinition = SpellBuilder.Build(form, effect, modifiers);
        }
    }

    // Pour l'initialisation dans l'inspecteur
    public void ForceInit() => RecalculateStats();
}