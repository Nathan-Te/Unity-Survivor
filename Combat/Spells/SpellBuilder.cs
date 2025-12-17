using System.Collections.Generic;
using UnityEngine;

public static class SpellBuilder
{
    public static SpellDefinition Build(SpellSlot slot)
    {
        SpellDefinition def = new SpellDefinition();

        SpellForm form = slot.formRune.AsForm;
        SpellEffect effect = slot.effectRune.AsEffect;

        // On r�cup�re les bonus accumul�s
        RuneStats formStats = slot.formRune.AccumulatedStats;
        RuneStats effectStats = slot.effectRune.AccumulatedStats;

        // R�cup�ration des stats globales
        var stats = PlayerStats.Instance;
        float globalMight = stats != null ? stats.Might : 1f;
        float globalCD = stats != null ? stats.CooldownSpeed : 1f;
        float globalArea = stats != null ? stats.AreaSize : 1f;
        float globalProjSpeed = stats != null ? stats.ProjectileSpeed : 1f;
        int globalCount = stats != null ? stats.AdditionalAmount : 0;

        // 1. Config Base
        def.Form = form;
        def.Effect = effect;
        def.Mode = form.targetingMode;
        def.RequiresLoS = form.requiresLineOfSight;

        // Get the appropriate prefab for this (form, effect) combination
        if (SpellPrefabRegistry.Instance != null)
        {
            def.Prefab = SpellPrefabRegistry.Instance.GetPrefab(form, effect);
            def.ImpactVfxPrefab = SpellPrefabRegistry.Instance.GetImpactVfx(form, effect);

            // Get Smite timing configuration from mapping
            var timing = SpellPrefabRegistry.Instance.GetSmiteTiming(form, effect);
            def.SmiteImpactDelay = timing.impactDelay;
            def.SmiteVfxSpawnDelay = timing.vfxSpawnDelay;
            def.SmiteLifetime = timing.lifetime;
        }
        else
        {
            def.Prefab = form.prefab;
            def.ImpactVfxPrefab = null;
            def.SmiteImpactDelay = 0f;
            def.SmiteVfxSpawnDelay = 0f;
            def.SmiteLifetime = 2f;
        }

        // CRITICAL: Ensure we always have a valid prefab
        if (def.Prefab == null)
        {
            Debug.LogError($"[SpellBuilder] No prefab found for {form.runeName} + {effect.runeName}. Registry: {(SpellPrefabRegistry.Instance != null ? "EXISTS" : "NULL")}, form.prefab: {(form.prefab != null ? form.prefab.name : "NULL")}");
        }

        // 2. Calcul Stats Forme (Base SO + Bonus Forme)
        // Cooldown : Base * (1 + Somme des % reduction)
        // Note : CooldownMult est typiquement n�gatif (ex: -0.1). On clamp � 0.1s min.
        float cdMult = Mathf.Max(0.1f, 1f + formStats.CooldownMult);
        def.Cooldown = form.baseCooldown * cdMult;

        def.Count = form.baseCount + formStats.FlatCount;
        def.Pierce = form.basePierce + formStats.FlatPierce;
        def.Spread = form.baseSpread + formStats.FlatSpread;

        def.Speed = form.baseSpeed * (1f + formStats.SpeedMult);
        def.Duration = form.baseDuration * (1f + formStats.DurationMult);
        def.Range = form.baseRange + formStats.FlatRange;

        // 3. Calcul Stats Effet (Base SO + Bonus Effet)
        float baseDmg = effect.baseDamage;
        float dmgMult = effect.baseDamageMultiplier + effectStats.DamageMult;

        def.Knockback = effect.baseKnockback + effectStats.FlatKnockback;

        // Stats sp�ciales (peuvent �tre boost�es via RuneStats si on veut)
        def.ChainCount = effect.baseChainCount + effectStats.FlatChainCount;
        def.ChainRange = effect.chainRange;
        def.ChainDamageReduction = effect.chainDamageReduction;
        def.MinionChance = effect.minionSpawnChance;
        def.MinionPrefab = effect.minionPrefab;

        // Burn stats (base effect + accumulated bonuses from effect rune)
        def.BurnDamagePerTick = effect.burnDamagePerTick + effectStats.FlatBurnDamage;
        def.BurnDuration = effect.burnDuration + effectStats.FlatBurnDuration;

        // 4. Application des Modificateurs
        // On parcourt les runes de modification
        foreach (var modRune in slot.modifierRunes)
        {
            if (modRune == null || modRune.AsModifier == null) continue;
            SpellModifier modSO = modRune.AsModifier;

            // Check compatibilit�
            if (modSO.requiredTag != SpellTag.None && !form.tags.HasFlag(modSO.requiredTag)) continue;

            // Les stats du mod (Base + Upgrades) sont dans AccumulatedStats
            RuneStats modStats = modRune.AccumulatedStats;

            // Application Multiplicative pour les %
            dmgMult *= (1f + modStats.DamageMult);
            def.Cooldown *= Mathf.Max(0.1f, 1f + modStats.CooldownMult);
            def.Speed *= (1f + modStats.SpeedMult);
            def.Duration *= (1f + modStats.DurationMult);

            // Taille : Multiplicatif sur la base
            // Note : On n'a pas trait� la taille dans Form/Effect avant, on le fait ici
            // On part du principe que la taille de base est 1 (ou scale du prefab)
            // On accumule les bonus de taille de partout
            float totalSizeMult = (1f + formStats.SizeMult + effectStats.SizeMult + modStats.SizeMult);
            // Pour simplifier, on multiplie la taille courante (si plusieurs mods, �a se multiplie)
            // Mais l'addition des % est souvent plus stable : 
            // Ici, faisons simple : chaque mod multiplie la taille finale.
            // def.Size est calcul� � la fin.

            // Application Additive pour les Flats
            def.Count += modStats.FlatCount;
            def.Pierce += modStats.FlatPierce;
            def.Spread += modStats.FlatSpread;
            def.Range += modStats.FlatRange;
            def.Knockback += modStats.FlatKnockback;
            def.ChainCount += modStats.FlatChainCount;

            // Burn stats from modifiers
            def.BurnDamagePerTick += modStats.FlatBurnDamage;
            def.BurnDuration += modStats.FlatBurnDuration;

            if (modSO.enableHoming) def.IsHoming = true;
        }

        // 5. Finalisation
        def.Damage = baseDmg * dmgMult * form.procCoefficient;

        // Calcul final de la taille (somme des bonus de taille de toutes les sources)
        float finalSizeBonus = formStats.SizeMult + effectStats.SizeMult;
        foreach (var m in slot.modifierRunes) if (m?.Data != null) finalSizeBonus += m.AccumulatedStats.SizeMult;

        float prefabScale = (def.Prefab ? def.Prefab.transform.localScale.x : 1f);
        def.Size = prefabScale * (1f + finalSizeBonus);

        if (def.Range <= 0) def.Range = 20f;

        // APPLICATION DES GLOBAUX � LA FIN
        // Cooldown : Base / Vitesse (ex: 1s / 1.5 = 0.66s)
        def.Cooldown /= globalCD;

        def.Damage *= globalMight;
        def.Size *= globalArea;
        def.Speed *= globalProjSpeed;
        def.Count += globalCount;

        return def;
    }
}