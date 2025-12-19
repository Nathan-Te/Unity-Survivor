using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

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
            titleText.text = rune.Data.runeName;
            levelText.text = $"LvL {rune.Level}";

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
            _sb.Append(effect.element);
            _sb.Append("</color>\n");

            if (effect.applyBurn)
            {
                if (slot != null && slot.Definition != null)
                {
                    _sb.Append("• Burn: <color=yellow>");
                    _sb.Append(slot.Definition.BurnDamagePerTick.ToString("F1"));
                    _sb.Append(" dmg/tick</color> for <color=yellow>");
                    _sb.Append(slot.Definition.BurnDuration.ToString("F1"));
                    _sb.Append("s</color>\n");
                }
                else
                {
                    _sb.Append("• Burn: ");
                    _sb.Append(effect.burnDamagePerTick.ToString("F1"));
                    _sb.Append(" dmg/tick for ");
                    _sb.Append(effect.burnDuration.ToString("F1"));
                    _sb.Append("s\n");
                }
            }
            if (effect.applySlow) _sb.Append("• Slow\n");
            if (effect.baseChainCount > 0) _sb.Append("• Chain x").Append(effect.baseChainCount).Append("\n");
            if (effect.aoeRadius > 0) _sb.Append("• AoE ").Append(effect.aoeRadius).Append("m\n");
            if (effect.minionSpawnChance > 0) _sb.Append("• Summon ").Append((effect.minionSpawnChance * 100).ToString("F0")).Append("%\n");
        }
        else if (rune.AsModifier != null)
        {
            SpellModifier modifier = rune.AsModifier;
            if (modifier.enableHoming) _sb.Append("• Homing\n");
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
                _sb.Append("<color=yellow>Total Damage: ").Append(def.Damage.ToString("F0")).Append("</color>\n");
                _sb.Append("Cooldown: ").Append(def.Cooldown.ToString("F1")).Append("s\n");

                if (def.Count > 1) _sb.Append("Count: ").Append(def.Count).Append("\n");
                if (def.Pierce > 0) _sb.Append("Pierce: ").Append(def.Pierce).Append("\n");
                if (def.Spread > 0) _sb.Append("Spread: ").Append(def.Spread.ToString("F0")).Append("°\n");
                if (def.Range > 0) _sb.Append("Range: ").Append(def.Range.ToString("F0")).Append("m\n");

                // Display crit stats if any
                if (def.CritChance > 0)
                {
                    _sb.Append("<color=#FFD700>Crit Chance: ").Append((def.CritChance * 100f).ToString("F1")).Append("%</color>\n");
                }
                if (def.CritDamageMultiplier > 1f)
                {
                    _sb.Append("<color=#FFD700>Crit Damage: ").Append((def.CritDamageMultiplier * 100f).ToString("F0")).Append("%</color>\n");
                }
            }
            else
            {
                _sb.Append("Cooldown: ").Append(form.baseCooldown.ToString("F1")).Append("s\n");
                if (form.baseCount > 1) _sb.Append("Count: ").Append(form.baseCount).Append("\n");
                if (form.basePierce > 0) _sb.Append("Pierce: ").Append(form.basePierce).Append("\n");
                if (form.baseSpread > 0) _sb.Append("Spread: ").Append(form.baseSpread.ToString("F0")).Append("°\n");
            }
        }
        else if (rune.AsEffect != null)
        {
            SpellEffect effect = rune.AsEffect;

            if (slot != null && slot.Definition != null)
            {
                SpellDefinition def = slot.Definition;
                _sb.Append("<color=yellow>Total Damage: ").Append(def.Damage.ToString("F0")).Append("</color>\n");

                // Display crit stats if any
                if (def.CritChance > 0)
                {
                    _sb.Append("<color=#FFD700>Crit Chance: ").Append((def.CritChance * 100f).ToString("F1")).Append("%</color>\n");
                }
                if (def.CritDamageMultiplier > 1f)
                {
                    _sb.Append("<color=#FFD700>Crit Damage: ").Append((def.CritDamageMultiplier * 100f).ToString("F0")).Append("%</color>\n");
                }
            }
            else
            {
                _sb.Append("Damage: ").Append(effect.baseDamage.ToString("F0")).Append("\n");
            }

            if (effect.baseDamageMultiplier != 1.0f)
                _sb.Append("Damage Mult: ").Append((effect.baseDamageMultiplier * 100).ToString("F0")).Append("%\n");
            if (effect.baseKnockback > 0)
                _sb.Append("Knockback: ").Append(effect.baseKnockback.ToString("F1")).Append("\n");
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
        if (stats.DamageMult != 0) { FormatMultiplier(stats.DamageMult); _sb.Append(" Damage\n"); }
        if (stats.CooldownMult != 0) { FormatMultiplier(stats.CooldownMult); _sb.Append(" Cooldown\n"); }
        if (stats.SizeMult != 0) { FormatMultiplier(stats.SizeMult); _sb.Append(" Size\n"); }
        if (stats.SpeedMult != 0) { FormatMultiplier(stats.SpeedMult); _sb.Append(" Speed\n"); }
        if (stats.DurationMult != 0) { FormatMultiplier(stats.DurationMult); _sb.Append(" Duration\n"); }

        if (stats.FlatCount != 0) _sb.Append("<color=green>+").Append(stats.FlatCount).Append("</color> Projectiles\n");
        if (stats.FlatPierce != 0) _sb.Append("<color=green>+").Append(stats.FlatPierce).Append("</color> Pierce\n");
        if (stats.FlatSpread != 0) _sb.Append("<color=green>+").Append(stats.FlatSpread.ToString("F0")).Append("°</color> Spread\n");
        if (stats.FlatRange != 0) _sb.Append("<color=green>+").Append(stats.FlatRange.ToString("F0")).Append("m</color> Range\n");
        if (stats.FlatKnockback != 0) _sb.Append("<color=green>+").Append(stats.FlatKnockback.ToString("F1")).Append("</color> Knockback\n");
        if (stats.FlatChainCount != 0) _sb.Append("<color=green>+").Append(stats.FlatChainCount).Append("</color> Chain\n");

        if (stats.FlatBurnDamage != 0) _sb.Append("<color=orange>+").Append(stats.FlatBurnDamage.ToString("F1")).Append("</color> Burn Damage\n");
        if (stats.FlatBurnDuration != 0) _sb.Append("<color=orange>+").Append(stats.FlatBurnDuration.ToString("F1")).Append("s</color> Burn Duration\n");

        if (stats.FlatCritChance != 0)
        {
            float critPercent = stats.FlatCritChance * 100f;
            _sb.Append("<color=#FFD700>+").Append(critPercent.ToString("F1")).Append("%</color> Crit Chance\n");
        }
        if (stats.FlatCritDamage != 0)
        {
            float critDmgPercent = stats.FlatCritDamage * 100f;
            _sb.Append("<color=#FFD700>+").Append(critDmgPercent.ToString("F1")).Append("%</color> Crit Damage\n");
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
}
