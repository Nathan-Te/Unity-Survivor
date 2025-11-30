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

        // 1. Titre et Rareté
        string rarityName = data.Rarity.ToString();
        Color rarityColor = RarityUtils.GetColor(data.Rarity);
        int levelBoost = RarityUtils.GetLevelBoost(data.Rarity);

        // On cherche si le joueur a déjà cette rune
        SpellManager sm = FindFirstObjectByType<SpellManager>();
        Rune existingRune = sm != null ? sm.FindActiveRune(GetSOFromData(data)) : null;

        string typeText = (existingRune != null) ? "AMÉLIORATION" : "NOUVEAU";

        titleText.text = $"{data.Name} <size=60%>({typeText})</size>";
        titleText.color = rarityColor;

        // 2. Description Dynamique (Avant -> Après)
        RuneSO so = GetSOFromData(data);
        if (so != null)
        {
            // On passe la rune existante (ou null) et la rareté
            descriptionText.text = so.GetDescription(existingRune, data.Rarity);

            // Petit ajout visuel pour le boost de niveau
            descriptionText.text += $"\n\n<color=yellow>Gain : +{levelBoost} Niveau(x)</color>";
        }

        // 3. Icone
        if (data.Icon != null)
        {
            iconImage.sprite = data.Icon;
            iconImage.enabled = true;
        }
        else iconImage.enabled = false;

        // Listeners
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnSelect);
    }

    private RuneSO GetSOFromData(UpgradeData data)
    {
        if (data.Type == UpgradeType.NewSpell) return data.TargetForm;
        if (data.Type == UpgradeType.Modifier) return data.TargetModifier;
        if (data.Type == UpgradeType.Effect) return data.TargetEffect;
        return null;
    }

    private void OnSelect()
    {
        _manager.SelectUpgrade(_data);
    }
}