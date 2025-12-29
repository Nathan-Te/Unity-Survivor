using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SurvivorGame.Localization;

public class UpgradeCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image borderImage;
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

        // Apply rarity color to border
        if (borderImage != null)
        {
            borderImage.color = rarityColor;
        }

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
    }

    private void OnSelect() => _manager.SelectUpgrade(_data);
}