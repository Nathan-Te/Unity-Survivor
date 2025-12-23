using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

        string rarityName = data.Rarity.ToString();
        Color rarityColor = RarityUtils.GetColor(data.Rarity);

        titleText.text = $"{data.Name} <size=70%>{rarityName}</size>";
        titleText.color = rarityColor;

        // C'est ici que �a change : On affiche la description manuelle de la RuneDefinition tir�e
        descriptionText.text = data.Description;
        // Plus besoin de GetLevelUpDescription() !

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