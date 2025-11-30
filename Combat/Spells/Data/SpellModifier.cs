using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Modifier")]
public class SpellModifier : RuneSO
{
    public override RuneType Type => RuneType.Modifier;

    // ... (Champs existants) ...
    [Header("Restriction")]
    public SpellTag requiredTag;

    [Header("Multiplicateurs de Stats")]
    public float damageMult = 1f;
    public float cooldownMult = 1f;
    public float sizeMult = 1f;
    public float speedMult = 1f;
    public float durationMult = 1f;

    [Header("Croissance")]
    public float damageMultGrowth = 0.05f;

    [Header("Additions")]
    public int addCount = 0;
    public int addPierce = 0;
    public float addSpread = 0f;
    public bool enableHoming;

    public override string GetDescription(Rune currentRune, Rarity rarity)
    {
        int currentLvl = currentRune != null ? currentRune.Level : 1;
        int boost = RarityUtils.GetLevelBoost(rarity);
        int nextLvl = currentLvl + boost;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine(description);
        sb.AppendLine();

        // On affiche seulement les stats pertinentes (qui changent)
        if (damageMult != 1f || damageMultGrowth > 0)
        {
            float cur = damageMult + (damageMultGrowth * (currentLvl - 1));
            float next = damageMult + (damageMultGrowth * (nextLvl - 1));
            sb.AppendLine(FormatStat("Puissance", cur, next, "x"));
        }

        if (addCount > 0) sb.AppendLine($"+{addCount} Projectiles");
        if (addPierce > 0) sb.AppendLine($"+{addPierce} Pierce");
        if (enableHoming) sb.AppendLine("Guidage Activé");

        return sb.ToString();
    }
}