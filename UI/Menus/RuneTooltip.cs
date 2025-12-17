using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays detailed information about a rune when hovering over it
/// </summary>
public class RuneTooltip : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI baseStatsText;
    [SerializeField] private TextMeshProUGUI accumulatedStatsText;
    [SerializeField] private CanvasGroup canvasGroup;

    private static RuneTooltip _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        Hide();
    }

    public static RuneTooltip Instance => _instance;

    /// <summary>
    /// Shows the tooltip with information about a rune
    /// </summary>
    public void Show(Rune rune, Vector3 position)
    {
        if (rune == null || rune.Data == null)
        {
            Hide();
            return;
        }

        // Set title and level
        titleText.text = rune.Data.runeName;
        levelText.text = $"Level {rune.Level}";

        // Build description based on rune type
        BuildDescription(rune);

        // Build base stats
        BuildBaseStats(rune);

        // Build accumulated stats
        BuildAccumulatedStats(rune);

        // Position the tooltip
        transform.position = position;

        // Show
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Hides the tooltip
    /// </summary>
    public void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
        gameObject.SetActive(false);
    }

    private void BuildDescription(Rune rune)
    {
        string desc = "";

        if (rune.AsForm != null)
        {
            SpellForm form = rune.AsForm;
            desc += $"<b>Type:</b> Spell Form\n";
            desc += $"<b>Targeting:</b> {form.targetingMode}\n";
            desc += $"<b>Tags:</b> {form.tags}\n";
            if (form.requiresLineOfSight)
                desc += "Requires Line of Sight\n";
        }
        else if (rune.AsEffect != null)
        {
            SpellEffect effect = rune.AsEffect;
            desc += $"<b>Type:</b> Spell Effect\n";
            desc += $"<b>Element:</b> {effect.element}\n";

            if (effect.applyBurn)
                desc += "Applies Burn\n";
            if (effect.applySlow)
                desc += "Applies Slow\n";
            if (effect.baseChainCount > 0)
                desc += $"Chains {effect.baseChainCount} times\n";
            if (effect.aoeRadius > 0)
                desc += $"AoE Radius: {effect.aoeRadius}m\n";
            if (effect.minionSpawnChance > 0)
                desc += $"Minion Spawn: {effect.minionSpawnChance * 100}%\n";
        }
        else if (rune.AsModifier != null)
        {
            SpellModifier modifier = rune.AsModifier;
            desc += $"<b>Type:</b> Modifier\n";

            if (modifier.requiredTag != SpellTag.None)
                desc += $"<b>Requires:</b> {modifier.requiredTag}\n";
            if (modifier.enableHoming)
                desc += "Enables Homing\n";
        }

        descriptionText.text = desc;
    }

    private void BuildBaseStats(Rune rune)
    {
        string stats = "<b>BASE STATS:</b>\n";

        if (rune.AsForm != null)
        {
            SpellForm form = rune.AsForm;
            stats += $"Cooldown: {form.baseCooldown:F2}s\n";
            stats += $"Count: {form.baseCount}\n";
            stats += $"Pierce: {form.basePierce}\n";
            stats += $"Speed: {form.baseSpeed:F1}\n";
            stats += $"Range: {form.baseRange:F1}m\n";
            stats += $"Duration: {form.baseDuration:F1}s\n";
            if (form.baseSpread > 0)
                stats += $"Spread: {form.baseSpread:F1}°\n";
            stats += $"Proc Coefficient: {form.procCoefficient * 100:F0}%\n";
        }
        else if (rune.AsEffect != null)
        {
            SpellEffect effect = rune.AsEffect;
            stats += $"Damage: {effect.baseDamage:F1}\n";
            stats += $"Damage Mult: {effect.baseDamageMultiplier * 100:F0}%\n";
            if (effect.baseKnockback > 0)
                stats += $"Knockback: {effect.baseKnockback:F1}\n";
            if (effect.baseChainCount > 0)
            {
                stats += $"Chain Count: {effect.baseChainCount}\n";
                stats += $"Chain Range: {effect.chainRange:F1}m\n";
                stats += $"Chain Damage: {effect.chainDamageReduction * 100:F0}%\n";
            }
        }
        else if (rune.AsModifier != null)
        {
            stats += FormatRuneStats(rune.AsModifier.baseStats);
        }

        baseStatsText.text = stats;
    }

    private void BuildAccumulatedStats(Rune rune)
    {
        string stats = "<b>ACCUMULATED BONUSES:</b>\n";

        if (rune.AccumulatedStats.Equals(RuneStats.Zero))
        {
            stats += "<i>No bonuses</i>";
        }
        else
        {
            stats += FormatRuneStats(rune.AccumulatedStats);
        }

        accumulatedStatsText.text = stats;
    }

    private string FormatRuneStats(RuneStats stats)
    {
        string result = "";

        // Multiplicateurs
        if (stats.DamageMult != 0)
            result += $"Damage: {FormatMultiplier(stats.DamageMult)}\n";
        if (stats.CooldownMult != 0)
            result += $"Cooldown: {FormatMultiplier(stats.CooldownMult)}\n";
        if (stats.SizeMult != 0)
            result += $"Size: {FormatMultiplier(stats.SizeMult)}\n";
        if (stats.SpeedMult != 0)
            result += $"Speed: {FormatMultiplier(stats.SpeedMult)}\n";
        if (stats.DurationMult != 0)
            result += $"Duration: {FormatMultiplier(stats.DurationMult)}\n";

        // Additions
        if (stats.FlatCount != 0)
            result += $"+{stats.FlatCount} Projectiles\n";
        if (stats.FlatPierce != 0)
            result += $"+{stats.FlatPierce} Pierce\n";
        if (stats.FlatSpread != 0)
            result += $"+{stats.FlatSpread:F1}° Spread\n";
        if (stats.FlatRange != 0)
            result += $"+{stats.FlatRange:F1}m Range\n";
        if (stats.FlatKnockback != 0)
            result += $"+{stats.FlatKnockback:F1} Knockback\n";
        if (stats.FlatChainCount != 0)
            result += $"+{stats.FlatChainCount} Chain Count\n";

        if (string.IsNullOrEmpty(result))
            result = "<i>No stats</i>\n";

        return result;
    }

    private string FormatMultiplier(float mult)
    {
        float percent = mult * 100f;
        if (percent > 0)
            return $"<color=green>+{percent:F0}%</color>";
        else
            return $"<color=red>{percent:F0}%</color>";
    }
}
