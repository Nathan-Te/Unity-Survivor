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
        float baseDamage = effect.baseDamage;
        float baseCooldown = form.baseCooldown;
        float baseSpeed = form.baseSpeed;
        float baseSize = 1f;
        float baseRange = 20f;

        def.Count = form.baseCount;
        def.Pierce = form.basePierce; // <--- CORRIGÉ (Au lieu de 0)
        def.Duration = form.baseDuration;

        // 2. Application des Multiplicateurs (Mods)
        float damageMult = effect.damageMultiplier;
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
        def.Cooldown = Mathf.Max(0.1f, baseCooldown * cooldownMult);
        def.Speed = baseSpeed * speedMult;
        def.Size = form.prefab.transform.localScale.x * sizeMult * baseSize;
        def.Range = baseRange;

        return def;
    }
}