using UnityEngine;
using UnityEngine.UI;
using TMPro; // N'oublie pas d'importer TextMeshPro via le Package Manager ou clic droit

public class UpgradeCard : MonoBehaviour
{
    [Header("UI Elements")]
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

        string lvlSuffix = data.Level > 1 ? $" (Lvl {data.Level})" : "";
        titleText.text = $"{data.Name} <size=70%>{data.Rarity}</size>";
        titleText.color = RarityUtils.GetColor(data.Rarity);

        RuneSO so = null;
        if (data.Type == UpgradeType.NewSpell) so = data.TargetForm;
        else if (data.Type == UpgradeType.Modifier) so = data.TargetModifier;
        else if (data.Type == UpgradeType.Effect) so = data.TargetEffect;

        if (so != null)
        {
            descriptionText.text = so.GetLevelUpDescription(1, data.Rarity);
        }

        if (data.Icon != null)
        {
            iconImage.sprite = data.Icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }

        // Nettoyage des anciens listeners
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnSelect);
    }

    private void OnSelect()
    {
        _manager.SelectUpgrade(_data);
    }
}