using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using SurvivorGame.Localization;

/// <summary>
/// Displays detailed information about a rune when hovering over it
/// </summary>
public class RuneTooltip : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private CanvasGroup canvasGroup;

    private static RuneTooltip _instance;

    // Cache to avoid recalculating on every hover
    private Rune _lastRune;
    private SpellSlot _lastSlot;
    private int _lastRuneLevel;
    private float _lastSlotDamage;
    private float _lastCritChance;
    private float _lastCritDamage;

    // Reusable StringBuilder to avoid string allocations
    private StringBuilder _sb = new StringBuilder(512);

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        // Keep GameObject active but invisible - avoids layout rebuild on Show()
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    public static RuneTooltip Instance => _instance;

    /// <summary>
    /// Shows the tooltip with information about a rune and optionally the full spell context
    /// </summary>
    public void Show(Rune rune, Vector3 position, SpellSlot slot = null)
    {
        if (rune == null || rune.Data == null)
        {
            Hide();
            return;
        }

        // Check if we need to rebuild the content (cache check)
        float currentSlotDamage = slot?.Definition?.Damage ?? 0f;
        float currentCritChance = slot?.Definition?.CritChance ?? 0f;
        float currentCritDamage = slot?.Definition?.CritDamageMultiplier ?? 0f;

        bool needsRebuild = _lastRune != rune ||
                           _lastSlot != slot ||
                           _lastRuneLevel != rune.Level ||
                           _lastSlotDamage != currentSlotDamage ||
                           _lastCritChance != currentCritChance ||
                           _lastCritDamage != currentCritDamage;

        if (needsRebuild)
        {
            // Update cache tracking
            _lastRune = rune;
            _lastSlot = slot;
            _lastRuneLevel = rune.Level;
            _lastSlotDamage = currentSlotDamage;
            _lastCritChance = currentCritChance;
            _lastCritDamage = currentCritDamage;

            // Rebuild content using StringBuilder (zero allocations on subsequent hovers)
            titleText.text = rune.Data.GetLocalizedName();

            // Display level with max level if defined
            int maxLevel = rune.Data.GetMaxLevel();
            if (maxLevel > 0)
            {
                levelText.text = SimpleLocalizationHelper.FormatLevelWithMax(rune.Level, maxLevel);

                // Color code if at max level
                if (rune.Data.IsMaxLevel(rune.Level))
                {
                    levelText.color = new Color(1f, 0.84f, 0f); // Gold color
                }
                else
                {
                    levelText.color = Color.white;
                }
            }
            else
            {
                levelText.text = $"{SimpleLocalizationHelper.GetTooltipLevel()} {rune.Level}";
                levelText.color = Color.white;
            }

            _sb.Clear();
            BuildDescription(rune, slot);
            BuildBaseStats(rune, slot);
            BuildAccumulatedStats(rune);
            contentText.text = _sb.ToString();
        }

        // Position the tooltip (always update position even if content is cached)
        transform.position = position;

        // Show (NO SetActive call - just alpha change)
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// Shows the tooltip for a StatUpgradeSO (now with rune instance for level and accumulated stats)
    /// </summary>
    public void ShowForStatUpgrade(Rune rune, Vector3 position)
    {
        if (rune == null || rune.Data == null || rune.AsStatUpgrade == null)
        {
            Hide();
            return;
        }

        StatUpgradeSO statSO = rune.AsStatUpgrade;

        // Build display for StatUpgrade rune
        titleText.text = statSO.GetLocalizedName();

        // Display level with max level if defined
        int maxLevel = statSO.GetMaxLevel();
        if (maxLevel > 0)
        {
            levelText.text = SimpleLocalizationHelper.FormatLevelWithMax(rune.Level, maxLevel);

            // Color code if at max level
            if (statSO.IsMaxLevel(rune.Level))
            {
                levelText.color = new Color(1f, 0.84f, 0f); // Gold color
            }
            else
            {
                levelText.color = Color.white;
            }
        }
        else
        {
            levelText.text = $"{SimpleLocalizationHelper.GetTooltipLevel()} {rune.Level}";
            levelText.color = Color.white;
        }

        _sb.Clear();
        _sb.Append("<b>").Append(SimpleLocalizationHelper.GetTooltipType()).Append("</b> ").Append(SimpleLocalizationHelper.GetStatUpgradeType()).Append("\n");
        _sb.Append("<b>").Append(SimpleLocalizationHelper.GetTooltipTarget()).Append("</b> <color=yellow>").Append(SimpleLocalizationHelper.GetStatName(statSO.targetStat)).Append("</color>\n\n");

        // Display accumulated stat value
        if (rune.AccumulatedStats.StatValue != 0)
        {
            _sb.Append("<b>Total Bonus:</b>\n");
            _sb.Append("<color=green>+").Append(FormatStatValue(statSO.targetStat, rune.AccumulatedStats.StatValue)).Append("</color> ");
            _sb.Append(SimpleLocalizationHelper.GetStatName(statSO.targetStat));
        }

        contentText.text = _sb.ToString();

        // Position the tooltip
        transform.position = position;

        // Show
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
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
    }

    private void BuildDescription(Rune rune, SpellSlot slot)
    {
        if (rune.AsEffect != null)
        {
            SpellEffect effect = rune.AsEffect;
            _sb.Append("<color=#");
            _sb.Append(ColorUtility.ToHtmlStringRGB(effect.tintColor));
            _sb.Append(">");
            _sb.Append(SimpleLocalizationHelper.GetElementName(effect.element));
            _sb.Append("</color>\n");

            if (effect.applyBurn)
            {
                if (slot != null && slot.Definition != null)
                {
                    _sb.Append("• ").Append(SimpleLocalizationHelper.FormatBurn(slot.Definition.BurnDamagePerTick, slot.Definition.BurnDuration)).Append("\n");
                }
                else
                {
                    _sb.Append("• ").Append(SimpleLocalizationHelper.FormatBurn(effect.burnDamagePerTick, effect.burnDuration)).Append("\n");
                }
            }
            if (effect.applySlow) _sb.Append("• ").Append(SimpleLocalizationHelper.GetSlow()).Append("\n");
            if (effect.baseChainCount > 0) _sb.Append("• ").Append(SimpleLocalizationHelper.FormatChain(effect.baseChainCount)).Append("\n");
            if (effect.aoeRadius > 0) _sb.Append("• ").Append(SimpleLocalizationHelper.FormatAoE(effect.aoeRadius)).Append("\n");
            if (effect.minionSpawnChance > 0) _sb.Append("• ").Append(SimpleLocalizationHelper.FormatSummon(effect.minionSpawnChance * 100)).Append("\n");
        }
        else if (rune.AsModifier != null)
        {
            SpellModifier modifier = rune.AsModifier;
            if (modifier.enableHoming) _sb.Append("• ").Append(SimpleLocalizationHelper.GetHoming()).Append("\n");
        }

        if (_sb.Length > 0) _sb.Append("\n");
    }

    private void BuildBaseStats(Rune rune, SpellSlot slot)
    {
        int startLength = _sb.Length;

        if (rune.AsForm != null)
        {
            SpellForm form = rune.AsForm;

            if (slot != null && slot.Definition != null)
            {
                SpellDefinition def = slot.Definition;
                _sb.Append("<color=yellow>").Append(SimpleLocalizationHelper.GetDamageLabel()).Append(": ").Append(def.Damage.ToString("F0")).Append("</color>\n");
                _sb.Append(SimpleLocalizationHelper.GetCooldownLabel()).Append(": ").Append(def.Cooldown.ToString("F1")).Append("s\n");

                if (def.Count > 1) _sb.Append(SimpleLocalizationHelper.GetCountLabel()).Append(": ").Append(def.Count).Append("\n");
                if (def.Pierce > 0) _sb.Append(SimpleLocalizationHelper.GetPierceLabel()).Append(": ").Append(def.Pierce).Append("\n");
                if (def.Spread > 0) _sb.Append(SimpleLocalizationHelper.GetSpreadLabel()).Append(": ").Append(def.Spread.ToString("F0")).Append("°\n");
                if (def.Range > 0) _sb.Append(SimpleLocalizationHelper.GetRangeLabel()).Append(": ").Append(def.Range.ToString("F0")).Append("m\n");

                // Display crit stats if any
                if (def.CritChance > 0)
                {
                    _sb.Append("<color=#FFD700>").Append(SimpleLocalizationHelper.GetCritChanceLabel()).Append(": ").Append((def.CritChance * 100f).ToString("F1")).Append("%</color>\n");
                }
                if (def.CritDamageMultiplier > 1f)
                {
                    _sb.Append("<color=#FFD700>").Append(SimpleLocalizationHelper.GetCritDamageLabel()).Append(": ").Append((def.CritDamageMultiplier * 100f).ToString("F0")).Append("%</color>\n");
                }
            }
            else
            {
                _sb.Append(SimpleLocalizationHelper.GetCooldownLabel()).Append(": ").Append(form.baseCooldown.ToString("F1")).Append("s\n");
                if (form.baseCount > 1) _sb.Append(SimpleLocalizationHelper.GetCountLabel()).Append(": ").Append(form.baseCount).Append("\n");
                if (form.basePierce > 0) _sb.Append(SimpleLocalizationHelper.GetPierceLabel()).Append(": ").Append(form.basePierce).Append("\n");
                if (form.baseSpread > 0) _sb.Append(SimpleLocalizationHelper.GetSpreadLabel()).Append(": ").Append(form.baseSpread.ToString("F0")).Append("°\n");
            }
        }
        else if (rune.AsEffect != null)
        {
            SpellEffect effect = rune.AsEffect;

            if (slot != null && slot.Definition != null)
            {
                SpellDefinition def = slot.Definition;
                _sb.Append("<color=yellow>").Append(SimpleLocalizationHelper.GetDamageLabel()).Append(": ").Append(def.Damage.ToString("F0")).Append("</color>\n");

                // Display crit stats if any
                if (def.CritChance > 0)
                {
                    _sb.Append("<color=#FFD700>").Append(SimpleLocalizationHelper.GetCritChanceLabel()).Append(": ").Append((def.CritChance * 100f).ToString("F1")).Append("%</color>\n");
                }
                if (def.CritDamageMultiplier > 1f)
                {
                    _sb.Append("<color=#FFD700>").Append(SimpleLocalizationHelper.GetCritDamageLabel()).Append(": ").Append((def.CritDamageMultiplier * 100f).ToString("F0")).Append("%</color>\n");
                }
            }
            else
            {
                _sb.Append(SimpleLocalizationHelper.GetDamageLabel()).Append(": ").Append(effect.baseDamage.ToString("F0")).Append("\n");
            }

            if (effect.baseDamageMultiplier != 1.0f)
                _sb.Append(SimpleLocalizationHelper.GetDamageLabel()).Append(" Mult: ").Append((effect.baseDamageMultiplier * 100).ToString("F0")).Append("%\n");
            if (effect.baseKnockback > 0)
                _sb.Append(SimpleLocalizationHelper.GetKnockbackLabel()).Append(": ").Append(effect.baseKnockback.ToString("F1")).Append("\n");
        }
        else if (rune.AsModifier != null)
        {
            FormatRuneStats(rune.AsModifier.baseStats);
        }

        if (_sb.Length > startLength) _sb.Append("\n");
    }

    private void BuildAccumulatedStats(Rune rune)
    {
        if (!rune.AccumulatedStats.Equals(RuneStats.Zero))
        {
            _sb.Append("<b>Bonuses:</b>\n");
            FormatRuneStats(rune.AccumulatedStats);
        }
    }

    private void FormatRuneStats(RuneStats stats)
    {
        // Format: "+20% Damage" (value and label on same line)
        if (stats.DamageMult != 0) { _sb.Append("<color=green>+").Append((stats.DamageMult * 100f).ToString("F0")).Append("%</color> ").Append(SimpleLocalizationHelper.GetDamageLabel()).Append("\n"); }
        if (stats.CooldownMult != 0)
        {
            string color = stats.CooldownMult < 0 ? "green" : "red";
            string sign = stats.CooldownMult > 0 ? "+" : "";
            _sb.Append("<color=").Append(color).Append(">").Append(sign).Append((stats.CooldownMult * 100f).ToString("F0")).Append("%</color> ").Append(SimpleLocalizationHelper.GetCooldownLabel()).Append("\n");
        }
        if (stats.SizeMult != 0) { _sb.Append("<color=green>+").Append((stats.SizeMult * 100f).ToString("F0")).Append("%</color> ").Append(SimpleLocalizationHelper.GetSizeLabel()).Append("\n"); }
        if (stats.SpeedMult != 0) { _sb.Append("<color=green>+").Append((stats.SpeedMult * 100f).ToString("F0")).Append("%</color> ").Append(SimpleLocalizationHelper.GetSpeedLabel()).Append("\n"); }
        if (stats.DurationMult != 0) { _sb.Append("<color=green>+").Append((stats.DurationMult * 100f).ToString("F0")).Append("%</color> ").Append(SimpleLocalizationHelper.GetDurationLabel()).Append("\n"); }

        if (stats.FlatCooldown != 0)
        {
            string color = stats.FlatCooldown < 0 ? "green" : "red";
            _sb.Append("<color=").Append(color).Append(">").Append(stats.FlatCooldown > 0 ? "+" : "").Append(stats.FlatCooldown.ToString("F2")).Append("s</color> ").Append(SimpleLocalizationHelper.GetCooldownLabel()).Append("\n");
        }
        if (stats.FlatCount != 0) _sb.Append("<color=green>+").Append(stats.FlatCount).Append("</color> ").Append(SimpleLocalizationHelper.GetCountLabel()).Append("\n");
        if (stats.FlatPierce != 0) _sb.Append("<color=green>+").Append(stats.FlatPierce).Append("</color> ").Append(SimpleLocalizationHelper.GetPierceLabel()).Append("\n");
        if (stats.FlatSpread != 0) _sb.Append("<color=green>+").Append(stats.FlatSpread.ToString("F0")).Append("°</color> ").Append(SimpleLocalizationHelper.GetSpreadLabel()).Append("\n");
        if (stats.FlatRange != 0) _sb.Append("<color=green>+").Append(stats.FlatRange.ToString("F0")).Append("m</color> ").Append(SimpleLocalizationHelper.GetRangeLabel()).Append("\n");
        if (stats.FlatKnockback != 0) _sb.Append("<color=green>+").Append(stats.FlatKnockback.ToString("F1")).Append("</color> ").Append(SimpleLocalizationHelper.GetKnockbackLabel()).Append("\n");
        if (stats.FlatChainCount != 0) _sb.Append("<color=green>+").Append(stats.FlatChainCount).Append("</color> Chain\n");
        if (stats.FlatMulticast != 0) _sb.Append("<color=green>+").Append(stats.FlatMulticast).Append("</color> ").Append(SimpleLocalizationHelper.GetMulticastLabel()).Append("\n");

        if (stats.FlatBurnDamage != 0) _sb.Append("<color=orange>+").Append(stats.FlatBurnDamage.ToString("F1")).Append("</color> Burn ").Append(SimpleLocalizationHelper.GetDamageLabel()).Append("\n");
        if (stats.FlatBurnDuration != 0) _sb.Append("<color=orange>+").Append(stats.FlatBurnDuration.ToString("F1")).Append("s</color> Burn ").Append(SimpleLocalizationHelper.GetDurationLabel()).Append("\n");

        if (stats.FlatCritChance != 0)
        {
            float critPercent = stats.FlatCritChance * 100f;
            _sb.Append("<color=#FFD700>+").Append(critPercent.ToString("F1")).Append("%</color> ").Append(SimpleLocalizationHelper.GetCritChanceLabel()).Append("\n");
        }
        if (stats.FlatCritDamage != 0)
        {
            float critDmgPercent = stats.FlatCritDamage * 100f;
            _sb.Append("<color=#FFD700>+").Append(critDmgPercent.ToString("F1")).Append("%</color> ").Append(SimpleLocalizationHelper.GetCritDamageLabel()).Append("\n");
        }
    }

    private void FormatMultiplier(float mult)
    {
        float percent = mult * 100f;
        if (percent > 0)
        {
            _sb.Append("<color=green>+").Append(percent.ToString("F0")).Append("%</color>");
        }
        else
        {
            _sb.Append("<color=red>").Append(percent.ToString("F0")).Append("%</color>");
        }
    }

    /// <summary>
    /// Formats a stat value based on the stat type (percentage vs flat value)
    /// </summary>
    private string FormatStatValue(StatType statType, float value)
    {
        switch (statType)
        {
            // Percentage-based stats (display as percentage with 1 decimal)
            case StatType.MoveSpeed:
            case StatType.GlobalDamage:
            case StatType.GlobalCooldown:
            case StatType.GlobalArea:
            case StatType.GlobalSpeed:
            case StatType.ExperienceGain:
            case StatType.Armor:
            case StatType.CritChance:
            case StatType.CritDamage:
                return $"{(value * 100f).ToString("F1")}%";

            // Flat value stats
            case StatType.MaxHealth:
            case StatType.HealthRegen:
            case StatType.MagnetArea:
                return value.ToString("F1");

            // Integer stats
            case StatType.GlobalCount:
                return ((int)value).ToString();

            default:
                return value.ToString("F1");
        }
    }

    /// <summary>
    /// Adds upgrade list information from a RuneSO (works for all rune types including StatUpgradeSO)
    /// </summary>
    private void AddUpgradeListInfo(RuneSO runeSO)
    {
        if (runeSO.CommonUpgrades != null && runeSO.CommonUpgrades.Count > 0)
        {
            _sb.Append("<color=white>Common:</color> ");
            AppendUpgradeExamples(runeSO.CommonUpgrades);
            _sb.Append("\n");
        }

        if (runeSO.RareUpgrades != null && runeSO.RareUpgrades.Count > 0)
        {
            _sb.Append("<color=#4169E1>Rare:</color> ");
            AppendUpgradeExamples(runeSO.RareUpgrades);
            _sb.Append("\n");
        }

        if (runeSO.EpicUpgrades != null && runeSO.EpicUpgrades.Count > 0)
        {
            _sb.Append("<color=#9370DB>Epic:</color> ");
            AppendUpgradeExamples(runeSO.EpicUpgrades);
            _sb.Append("\n");
        }

        if (runeSO.LegendaryUpgrades != null && runeSO.LegendaryUpgrades.Count > 0)
        {
            _sb.Append("<color=#FFD700>Legendary:</color> ");
            AppendUpgradeExamples(runeSO.LegendaryUpgrades);
            _sb.Append("\n");
        }
    }

    /// <summary>
    /// Appends a summary of upgrades from a list (shows first example or count)
    /// </summary>
    private void AppendUpgradeExamples(System.Collections.Generic.List<RuneDefinition> upgrades)
    {
        if (upgrades.Count == 1)
        {
            string desc = upgrades[0].Description != null ? upgrades[0].Description.GetText() : "";
            _sb.Append(desc);
        }
        else
        {
            string desc = upgrades[0].Description != null ? upgrades[0].Description.GetText() : "";
            _sb.Append(desc).Append(" (").Append(upgrades.Count).Append(" options)");
        }
    }
}
