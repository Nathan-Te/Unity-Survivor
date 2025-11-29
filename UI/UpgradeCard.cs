using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

        // Calcul du bonus visuel
        float powerBoost = RarityUtils.GetPowerBoost(data.Rarity);
        string rarityName = data.Rarity.ToString();
        Color rarityColor = RarityUtils.GetColor(data.Rarity);

        // Titre : "Fireball (Legendary)"
        titleText.text = $"{data.Name} <size=70%>{rarityName}</size>";
        titleText.color = rarityColor;

        // Description
        // On essaie d'afficher ce que ce boost donnerait sur une rune de base
        // (C'est approximatif car on ne sait pas sur quel spell le joueur va le mettre, 
        // mais ça donne une idée de la puissance).
        RuneSO so = null;
        if (data.Type == UpgradeType.NewSpell) so = data.TargetForm;
        else if (data.Type == UpgradeType.Modifier) so = data.TargetModifier;
        else if (data.Type == UpgradeType.Effect) so = data.TargetEffect;

        int boost = RarityUtils.GetLevelBoost(data.Rarity);
        int targetDisplayLevel = 1 + boost;

        if (so != null)
        {
            // J'ai retiré GetLevelUpDescription de RuneSO pour simplifier
            // On affiche la description statique + info bonus
            descriptionText.text = so.GetLevelUpDescription(targetDisplayLevel);
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

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnSelect);
    }

    private void OnSelect()
    {
        _manager.SelectUpgrade(_data);
    }
}