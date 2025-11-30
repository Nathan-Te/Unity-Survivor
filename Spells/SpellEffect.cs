using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Effect")]
public class SpellEffect : RuneSO
{
    public override RuneType Type => RuneType.Effect;

    // ... (Tous tes champs existants) ...
    [Header("Identité")]
    public string effectName;
    public ElementType element;
    public Color tintColor = Color.white;

    [Header("Stats de Base")]
    public float baseDamage = 10f;
    public float damageMultiplier = 1.0f;
    public float knockbackForce = 0f;

    [Header("Croissance")]
    public float damageGrowth = 2f;
    public float multiplierGrowth = 0.1f;

    // ... (Status, Chain, Necro, Zone...) ...
    [Header("Status / Spécial")]
    public bool applyBurn;
    public bool applySlow;
    public bool chainLightning;
    public bool spawnMinionOnDeath;

    [Header("Foudre (Chain)")]
    public int baseChainCount = 0;
    public float chainRange = 8f;
    public float chainDamageReduction = 0.7f;

    [Header("Nécrotique")]
    [Range(0f, 1f)] public float minionSpawnChance = 0f;
    public GameObject minionPrefab;

    [Header("Zone")]
    public float aoeRadius = 0f;

    public override string GetDescription(Rune currentRune, Rarity rarity)
    {
        int currentLvl = currentRune != null ? currentRune.Level : 1;
        int boost = RarityUtils.GetLevelBoost(rarity);
        int nextLvl = currentLvl + boost;

        float curDmg = baseDamage + (damageGrowth * (currentLvl - 1));
        float nextDmg = baseDamage + (damageGrowth * (nextLvl - 1));

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // Affiche l'élément en couleur
        string hexColor = ColorUtility.ToHtmlStringRGB(tintColor);
        sb.AppendLine($"<color=#{hexColor}>{element}</color> : {description}");
        sb.AppendLine();

        sb.AppendLine(FormatStat("Dégâts de Base", curDmg, nextDmg));

        // Affichage conditionnel des propriétés spéciales
        if (applyBurn) sb.AppendLine("- Applique Brûlure");
        if (applySlow) sb.AppendLine("- Applique Ralentissement");
        if (baseChainCount > 0) sb.AppendLine($"\nChaîne : {baseChainCount} rebonds");

        return sb.ToString();
    }
}