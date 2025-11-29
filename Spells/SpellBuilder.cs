using System.Collections.Generic;
using UnityEngine;

public static class SpellBuilder
{
    // On prend le Slot en entrée pour accéder aux Niveaux des runes
    public static SpellDefinition Build(SpellSlot slot)
    {
        SpellDefinition def = new SpellDefinition();

        SpellForm form = slot.formRune.AsForm;
        SpellEffect effect = slot.effectRune.AsEffect;
        int formLvl = slot.formRune.Level;
        int effectLvl = slot.effectRune.Level;

        // 1. Base (Forme) & Comportement
        def.Form = form;
        def.Effect = effect;
        def.Mode = form.targetingMode;
        def.RequiresLoS = form.requiresLineOfSight;

        // --- CALCULS AVEC CROISSANCE (FORM) ---
        // Cooldown réduit par niveau
        float baseCooldown = Mathf.Max(0.1f, form.baseCooldown - (form.cooldownReductionPerLevel * (formLvl - 1)));

        // Count augmente par palier
        int extraCount = (formLvl - 1) / form.countIncreaseEveryXLevels; // Division entière
        def.Count = form.baseCount + extraCount;

        def.Pierce = form.basePierce;
        def.Duration = form.baseDuration;
        def.Range = 20f; // Ou une valeur dans Form

        // --- CALCULS AVEC CROISSANCE (EFFECT) ---
        // Dégâts = Base + (Growth * Lvl)
        float baseDamage = effect.baseDamage + (effect.damageGrowth * (effectLvl - 1));
        // Multiplicateur = Base + (Growth * Lvl)
        float effectMult = effect.damageMultiplier + (effect.multiplierGrowth * (effectLvl - 1));

        // Transfert des stats spéciales
        def.ChainCount = effect.baseChainCount;
        def.ChainRange = effect.chainRange;
        def.ChainDamageReduction = effect.chainDamageReduction;
        def.MinionChance = effect.minionSpawnChance;
        def.MinionPrefab = effect.minionPrefab;

        // 2. Modificateurs
        float damageMultTotal = effectMult;
        float cooldownMult = 1f;
        float sizeMult = 1f;
        float speedMult = 1f;

        foreach (var modRune in slot.modifierRunes)
        {
            if (modRune == null || modRune.AsModifier == null) continue;

            SpellModifier mod = modRune.AsModifier;
            int modLvl = modRune.Level;

            // Vérification Tag
            if (mod.requiredTag != SpellTag.None && !form.tags.HasFlag(mod.requiredTag)) continue;

            // --- CALCULS AVEC CROISSANCE (MODIFIER) ---
            // Exemple : Dégâts augmente avec le niveau du mod
            float modDmg = mod.damageMult + (mod.damageMultGrowth * (modLvl - 1));

            damageMultTotal *= modDmg;
            cooldownMult *= mod.cooldownMult;
            sizeMult *= mod.sizeMult;
            speedMult *= mod.speedMult;

            def.Count += mod.addCount;
            def.Pierce += mod.addPierce;
            if (mod.enableHoming) def.IsHoming = true;
        }

        // 3. Finalisation
        def.Damage = baseDamage * damageMultTotal * form.procCoefficient;
        def.Cooldown = Mathf.Max(0.1f, baseCooldown * cooldownMult);
        def.Speed = form.baseSpeed * speedMult;
        def.Size = (form.prefab ? form.prefab.transform.localScale.x : 1f) * sizeMult;

        return def;
    }
}