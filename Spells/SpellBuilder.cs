using System.Collections.Generic;
using UnityEngine;

public static class SpellBuilder
{
    public static SpellDefinition Build(SpellForm form, SpellEffect effect, List<SpellModifier> modifiers)
    {
        SpellDefinition def = new SpellDefinition();

        // 1. Base (Forme)
        def.Form = form;
        def.Effect = effect;

        def.Mode = form.targetingMode;
        def.RequiresLoS = form.requiresLineOfSight;

        // Valeurs de base
        float baseDamage = effect.baseDamage; // Les dégâts viennent de l'effet (Feu, Physique...)
        float baseCooldown = form.baseCooldown;
        float baseSpeed = form.baseSpeed;
        float baseSize = 1f;
        float baseRange = 20f; // Valeur par défaut ou définie dans la Forme si tu l'ajoutes

        def.Count = form.baseCount;
        def.Pierce = 0; // Par défaut
        def.Duration = form.baseDuration;

        // 2. Application des Multiplicateurs (Mods)
        float damageMult = effect.damageMultiplier; // Commence avec le mult de l'élément (ex: Physique x1.2)
        float cooldownMult = 1f;
        float sizeMult = 1f;
        float speedMult = 1f;

        if (modifiers != null)
        {
            foreach (var mod in modifiers)
            {
                // VÉRIFICATION COMPATIBILITÉ (Tags)
                if (mod.requiredTag != SpellTag.None && !form.tags.HasFlag(mod.requiredTag))
                {
                    // Incompatible : On ignore ce mod
                    continue;
                }

                // Application des stats
                damageMult *= mod.damageMult;
                cooldownMult *= mod.cooldownMult;
                sizeMult *= mod.sizeMult;
                speedMult *= mod.speedMult;

                // Additions
                def.Count += mod.addCount;
                def.Pierce += mod.addPierce;
                if (mod.enableHoming) def.IsHoming = true;
            }
        }

        // 3. Calcul Final
        def.Damage = baseDamage * damageMult;
        def.Cooldown = Mathf.Max(0.1f, baseCooldown * cooldownMult); // Sécurité anti-0
        def.Speed = baseSpeed * speedMult;
        def.Size = form.prefab.transform.localScale.x * sizeMult * baseSize; // On prend l'échelle du prefab en compte
        def.Range = baseRange; // Pourrait être modifiée par la vitesse ou durée

        return def;
    }
}