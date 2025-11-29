using System.Collections.Generic;
using UnityEngine;

public static class SpellBuilder
{
    public static SpellDefinition Build(SpellSlot slot)
    {
        SpellDefinition def = new SpellDefinition();

        SpellForm form = slot.formRune.AsForm;
        SpellEffect effect = slot.effectRune.AsEffect;

        // ON UTILISE TOTALPOWER AU LIEU DE LEVEL
        float formPower = slot.formRune.TotalPower;
        float effectPower = slot.effectRune.TotalPower;

        // 1. Base (Forme)
        def.Form = form;
        def.Effect = effect;
        def.Mode = form.targetingMode;
        def.RequiresLoS = form.requiresLineOfSight;

        // --- CALCULS FORM (Basés sur formPower) ---
        // Cooldown : Réduit selon la puissance accumulée
        // (Power - 1) car au niveau 1 (Power 1), le bonus est de 0.
        float reduction = form.cooldownReductionPerLevel * (formPower - 1);
        float baseCooldown = Mathf.Max(0.1f, form.baseCooldown - reduction);

        // Count augmente par palier de puissance
        int extraCount = Mathf.FloorToInt((formPower - 1) / form.countIncreaseEveryXLevels);
        def.Count = form.baseCount + extraCount;

        def.Pierce = form.basePierce;
        def.Duration = form.baseDuration;

        // Spread (Initial)
        float finalSpread = form.baseSpread;
        def.Range = 20f;

        // --- CALCULS EFFECT (Basés sur effectPower) ---
        // Dégâts
        float baseDamage = effect.baseDamage + (effect.damageGrowth * (effectPower - 1));
        // Multiplicateur
        float finalDmgMult = effect.damageMultiplier + (effect.multiplierGrowth * (effectPower - 1));

        // Transfert stats
        def.ChainCount = effect.baseChainCount;
        def.ChainRange = effect.chainRange;
        def.ChainDamageReduction = effect.chainDamageReduction;
        def.MinionChance = effect.minionSpawnChance;
        def.MinionPrefab = effect.minionPrefab;

        // 2. Modificateurs
        float damageMultTotal = finalDmgMult;
        float cooldownMult = 1f;
        float sizeMult = 1f;
        float speedMult = 1f;

        foreach (var modRune in slot.modifierRunes)
        {
            if (modRune == null || modRune.AsModifier == null) continue;

            SpellModifier mod = modRune.AsModifier;
            float modPower = modRune.TotalPower;

            if (mod.requiredTag != SpellTag.None && !form.tags.HasFlag(mod.requiredTag)) continue;

            // --- CALCULS MODIFIER (Basés sur modPower) ---
            float growth = mod.damageMultGrowth * (modPower - 1);
            float modDmg = mod.damageMult + growth;

            damageMultTotal *= modDmg;
            cooldownMult *= mod.cooldownMult;
            sizeMult *= mod.sizeMult;
            speedMult *= mod.speedMult;

            def.Count += mod.addCount;
            def.Pierce += mod.addPierce;
            finalSpread += mod.addSpread;
            if (mod.enableHoming) def.IsHoming = true;
        }

        // 3. Finalisation
        def.Damage = baseDamage * damageMultTotal * form.procCoefficient;
        def.Cooldown = Mathf.Max(0.1f, baseCooldown * cooldownMult);
        def.Speed = form.baseSpeed * speedMult;
        def.Size = (form.prefab ? form.prefab.transform.localScale.x : 1f) * sizeMult;
        def.Spread = finalSpread;

        return def;
    }
}