using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Form")]
public class SpellForm : RuneSO
{
    public override RuneType Type => RuneType.Form;

    // ... (Tout tes champs existants : prefab, targetingMode, stats...) ...
    [Header("Visuals")]
    public GameObject prefab;

    [Header("Stratégie")]
    public TargetingMode targetingMode = TargetingMode.Nearest;
    public bool requiresLineOfSight = true;
    public SpellTag tags;

    [Header("Stats de Base")]
    public float baseCooldown = 1f;
    public int baseCount = 1;
    public int basePierce = 0;
    public float baseSpread = 0f;

    [Header("Croissance")]
    public float cooldownReductionPerLevel = 0.05f;
    public int countIncreaseEveryXLevels = 5;

    [Header("Mouvement")]
    public float baseSpeed = 20f;
    public float impactDelay = 0f;
    public float baseDuration = 5f;
    [Range(0f, 1f)] public float procCoefficient = 1.0f;

    public override string GetDescription(Rune currentRune, Rarity rarity)
    {
        int currentLvl = currentRune != null ? currentRune.Level : 1;
        int boost = RarityUtils.GetLevelBoost(rarity);
        int nextLvl = currentLvl + boost;

        // Calcul Actuel
        float curCd = GetCooldownAtLevel(currentLvl);
        int curCount = GetCountAtLevel(currentLvl);

        // Calcul Futur
        float nextCd = GetCooldownAtLevel(nextLvl);
        int nextCount = GetCountAtLevel(nextLvl);

        // Construction du texte
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine(description); // La description textuelle de base
        sb.AppendLine();
        sb.AppendLine(FormatStat("Cooldown", curCd, nextCd, "s"));

        if (nextCount != curCount || baseCount > 1)
            sb.AppendLine(FormatIntStat("Projectiles", curCount, nextCount));

        return sb.ToString();
    }

    // Helpers de calcul (similaires à SpellBuilder) pour éviter la duplication
    private float GetCooldownAtLevel(int lvl)
    {
        float reduction = cooldownReductionPerLevel * (lvl - 1);
        return Mathf.Max(0.1f, baseCooldown - reduction);
    }

    private int GetCountAtLevel(int lvl)
    {
        int extra = (lvl - 1) / Mathf.Max(1, countIncreaseEveryXLevels);
        return baseCount + extra;
    }
}