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

        // C'est ici que ça change : On affiche la description manuelle de la RuneDefinition tirée
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
    }

    private void OnSelect() => _manager.SelectUpgrade(_data);
}