using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SurvivorGame.Localization;

public class UpgradeCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button selectButton;

    private UpgradeData _data;
    private LevelUpUI _manager;

    public void Initialize(UpgradeData data, LevelUpUI manager)
    {
        _data = data;
        _manager = manager;

        string rarityName = SimpleLocalizationHelper.GetRarityName(data.Rarity);
        Color rarityColor = RarityUtils.GetColor(data.Rarity);
        string runeType = SimpleLocalizationHelper.GetRuneTypeName(data.Type);

        // Title with rarity and type
        titleText.text = $"{data.Name} <size=70%>{rarityName}</size>\n<size=60%><i>{runeType}</i></size>";
        titleText.color = rarityColor;

        // Generate description based on upgrade type
        if (data.Type == UpgradeType.StatBoost && data.TargetStat != null && data.UpgradeDefinition != null)
        {
            // Generate random localized description for stat upgrades
            descriptionText.text = StatUpgradeDescriptionGenerator.GenerateRandomDescription(
                data.TargetStat.targetStat,
                data.UpgradeDefinition.Stats.StatValue
            );
        }
        else if (data.UpgradeDefinition != null)
        {
            // Auto-generate description from stats for other rune types (using rich text for colors)
            descriptionText.text = RuneDescriptionGenerator.GenerateRichDescription(data.UpgradeDefinition);
        }
        else if (!string.IsNullOrEmpty(data.Description))
        {
            // Fallback to manual description if no upgrade definition
            descriptionText.text = data.Description;
        }
        else
        {
            descriptionText.text = "";
        }

        if (data.Icon != null)
        {
            iconImage.sprite = data.Icon;
            iconImage.enabled = true;
        }
        else iconImage.enabled = false;

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnSelect);

        // Add tooltip for StatUpgradeSO
        if (data.Type == UpgradeType.StatBoost && data.TargetStat != null)
        {
            // Remove any existing tooltip triggers to avoid duplicates
            var existingTrigger = iconImage.GetComponent<StatUpgradeTooltipTrigger>();
            if (existingTrigger == null)
            {
                existingTrigger = iconImage.gameObject.AddComponent<StatUpgradeTooltipTrigger>();
            }

            // Create a preview rune for the tooltip
            // Check if player already has this stat rune
            Rune previewRune = null;
            if (PlayerStats.Instance != null)
            {
                previewRune = PlayerStats.Instance.GetStatRune(data.TargetStat.targetStat);
            }

            // If no existing rune, create a temporary one for preview
            if (previewRune == null)
            {
                previewRune = new Rune(data.TargetStat);
                if (data.UpgradeDefinition != null)
                {
                    previewRune.InitializeWithStats(data.UpgradeDefinition);
                }
            }

            existingTrigger.SetStatRune(previewRune);
        }
    }

    private void OnSelect() => _manager.SelectUpgrade(_data);
}