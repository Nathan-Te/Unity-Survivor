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

        // 1. Base (Forme) & Comportement
        def.Form = form;
        def.Effect = effect;
        def.Mode = form.targetingMode;
        def.RequiresLoS = form.requiresLineOfSight;

        int formLvl = slot.formRune.Level;
        int effectLvl = slot.effectRune.Level;

        // --- CALCULS (SIMPLIFIÉS) ---
        // Cooldown : Dépend uniquement du niveau
        float reduction = form.cooldownReductionPerLevel * (formLvl - 1); // Plus de formMult
        float baseCooldown = Mathf.Max(0.1f, form.baseCooldown - reduction);

        // Count augmente par palier
        int extraCount = (formLvl - 1) / form.countIncreaseEveryXLevels; // Division entière
        def.Count = form.baseCount + extraCount;

        def.Pierce = form.basePierce;
        def.Duration = form.baseDuration;

        float finalSpread = form.baseSpread;

        def.Range = 20f; // Ou une valeur dans Form

        // --- CALCULS AVEC CROISSANCE & RARETÉ (EFFECT) ---
        float effectMult = slot.effectRune.PowerMultiplier;
        // Dégâts
        float baseDamage = effect.baseDamage + (effect.damageGrowth * (effectLvl - 1)); // Plus de effectMult
        float damageMult = effect.damageMultiplier + (effect.multiplierGrowth * (effectLvl - 1));

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
            float modRarity = modRune.PowerMultiplier;
            int modLvl = modRune.Level;

            // Vérification Tag
            if (mod.requiredTag != SpellTag.None && !form.tags.HasFlag(mod.requiredTag)) continue;

            // --- CALCULS AVEC CROISSANCE (MODIFIER) ---
            float growth = mod.damageMultGrowth * (modLvl - 1); // Plus de modRarity
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