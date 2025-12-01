using System.Collections.Generic;
using UnityEngine;

public static class SpellBuilder
{
    public static SpellDefinition Build(SpellSlot slot)
    {
        SpellDefinition def = new SpellDefinition();

        SpellForm form = slot.formRune.AsForm;
        SpellEffect effect = slot.effectRune.AsEffect;

        RuneStats formStats = slot.formRune.AccumulatedStats;
        RuneStats effectStats = slot.effectRune.AccumulatedStats;

        // 1. Base (Forme)
        def.Form = form;
        def.Effect = effect;
        def.Mode = form.targetingMode;
        def.RequiresLoS = form.requiresLineOfSight;

        // 2. Calculs Initiaux (Base du SO + Bonus accumulés de la Forme)
        // Cooldown : Base * (1 + BonusForme)
        // Note: CooldownMult est généralement négatif (ex: -0.1)
        float cdMult = Mathf.Max(0.1f, 1f + formStats.CooldownMult);
        def.Cooldown = form.baseCooldown * cdMult;

        def.Count = form.baseCount + formStats.FlatCount;
        def.Pierce = form.basePierce + formStats.FlatPierce;
        def.Spread = form.baseSpread + formStats.FlatSpread;

        def.Speed = form.baseSpeed * (1f + formStats.SpeedMult);
        def.Duration = form.baseDuration * (1f + formStats.DurationMult);
        def.Range = form.baseRange + formStats.FlatRange; // Assure-toi que baseRange existe dans SpellForm ! (sinon mets 20f)

        // 3. Calculs Effet (Base du SO + Bonus accumulés de l'Effet)
        def.Damage = effect.baseDamage * (1f + effectStats.DamageMult);
        def.Knockback = effect.baseKnockback + effectStats.FlatKnockback; // <--- Ajouté

        def.ChainCount = effect.baseChainCount + effectStats.FlatChainCount;
        def.ChainRange = effect.chainRange;
        def.ChainDamageReduction = effect.chainDamageReduction;
        def.MinionChance = effect.minionSpawnChance;
        def.MinionPrefab = effect.minionPrefab;

        // 4. Application des Modificateurs
        foreach (var modRune in slot.modifierRunes)
        {
            if (modRune == null || modRune.AsModifier == null) continue;
            SpellModifier modSO = modRune.AsModifier;

            if (modSO.requiredTag != SpellTag.None && !form.tags.HasFlag(modSO.requiredTag)) continue;

            // Les stats du mod sont déjà dans AccumulatedStats
            RuneStats modStats = modRune.AccumulatedStats;

            // Application multiplicative des %
            def.Damage *= (1f + modStats.DamageMult);
            def.Cooldown *= Mathf.Max(0.1f, 1f + modStats.CooldownMult);
            def.Speed *= (1f + modStats.SpeedMult);
            def.Size *= (1f + modStats.SizeMult);
            def.Duration *= (1f + modStats.DurationMult);

            // Application additive des Flats
            def.Count += modStats.FlatCount;
            def.Pierce += modStats.FlatPierce;
            def.Spread += modStats.FlatSpread;
            def.ChainCount += modStats.FlatChainCount;
            def.Knockback += modStats.FlatKnockback;

            if (modSO.enableHoming) def.IsHoming = true;
        }

        // Final adjustments
        def.Damage *= form.procCoefficient;
        float prefabScale = (form.prefab ? form.prefab.transform.localScale.x : 1f);
        def.Size = prefabScale * def.Size; // Attention: def.Size est initialisé à 0 par défaut (float), il faut le mettre à 1

        // CORRECTION TAILLE : Si SizeMult est 0 (défaut), on veut 1.
        // Dans RuneStats, SizeMult est un "Bonus" (+0.5).
        // Donc taille finale = Base * (1 + SommeBonusSize).
        // On recalcule propre :
        float totalSizeBonus = formStats.SizeMult + effectStats.SizeMult;
        foreach (var m in slot.modifierRunes) if (m?.AsModifier != null) totalSizeBonus += m.AccumulatedStats.SizeMult;

        def.Size = prefabScale * (1f + totalSizeBonus);

        if (def.Range <= 0) def.Range = 20f; // Sécurité

        return def;
    }
}