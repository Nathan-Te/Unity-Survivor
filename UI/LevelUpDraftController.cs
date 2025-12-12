using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the draft phase of level up - showing upgrade cards, rerolls, and ban mode.
/// </summary>
public class LevelUpDraftController : MonoBehaviour
{
    private GameObject draftPanel;
    private TextMeshProUGUI instructionText;
    private Button rerollButton;
    private TextMeshProUGUI rerollCostText;
    private Button banButton;
    private TextMeshProUGUI banStockText;
    private Transform cardsContainer;
    private GameObject cardPrefab;

    private bool _isBanMode = false;
    private LevelUpUI _mainUI;
    private UpgradeOptionGenerator _optionGenerator;

    public bool IsBanMode => _isBanMode;

    public void Initialize(
        LevelUpUI mainUI,
        UpgradeOptionGenerator optionGenerator,
        GameObject draftPanelRef,
        TextMeshProUGUI instructionTextRef,
        Button rerollButtonRef,
        TextMeshProUGUI rerollCostTextRef,
        Button banButtonRef,
        TextMeshProUGUI banStockTextRef,
        Transform cardsContainerRef,
        GameObject cardPrefabRef)
    {
        _mainUI = mainUI;
        _optionGenerator = optionGenerator;

        draftPanel = draftPanelRef;
        instructionText = instructionTextRef;
        rerollButton = rerollButtonRef;
        rerollCostText = rerollCostTextRef;
        banButton = banButtonRef;
        banStockText = banStockTextRef;
        cardsContainer = cardsContainerRef;
        cardPrefab = cardPrefabRef;

        rerollButton.onClick.AddListener(OnRerollClicked);
        banButton.onClick.AddListener(OnBanModeClicked);
    }

    /// <summary>
    /// Shows the draft phase with upgrade cards
    /// </summary>
    public void ShowDraftPhase(Rarity minRarity, RewardFilter filter)
    {
        _isBanMode = false;
        draftPanel.SetActive(true);
        UpdateDraftButtons();

        // Clear old cards
        foreach (Transform child in cardsContainer)
        {
            Destroy(child.gameObject);
        }

        // Generate and display new options
        List<UpgradeData> options = _optionGenerator.GenerateOptions(3, minRarity, filter);

        foreach (var option in options)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardsContainer);
            if (cardObj.TryGetComponent<UpgradeCard>(out var card))
            {
                card.Initialize(option, _mainUI);
            }
        }

        // Update instruction text
        instructionText.gameObject.SetActive(true);

        if (filter != RewardFilter.Any || minRarity > Rarity.Common)
            instructionText.text = "RÉCOMPENSE SPÉCIALE !";
        else
            instructionText.text = "LEVEL UP ! Choisissez une récompense";
    }

    /// <summary>
    /// Hides the draft panel
    /// </summary>
    public void Hide()
    {
        draftPanel.SetActive(false);
        instructionText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Updates reroll and ban button states
    /// </summary>
    private void UpdateDraftButtons()
    {
        if (rerollCostText)
            rerollCostText.text = $"Reroll ({LevelManager.Instance.availableRerolls})";

        rerollButton.interactable = LevelManager.Instance.availableRerolls > 0;

        if (banStockText)
            banStockText.text = $"Ban ({LevelManager.Instance.availableBans})";

        banButton.interactable = LevelManager.Instance.availableBans > 0;

        banButton.image.color = _isBanMode ? Color.red : Color.white;
    }

    private void OnRerollClicked()
    {
        if (LevelManager.Instance.ConsumeReroll())
        {
            _mainUI.ShowDraftPhase();
        }
    }

    private void OnBanModeClicked()
    {
        _isBanMode = !_isBanMode;
        UpdateDraftButtons();

        instructionText.text = _isBanMode
            ? "BANNISSEMENT : Cliquez sur une carte"
            : "CHOISISSEZ UNE RÉCOMPENSE";
    }
}
