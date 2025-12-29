using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SurvivorGame.Localization;

/// <summary>
/// Main coordinator for the level-up system.
/// Delegates to specialized controllers for draft, inventory, and option generation.
/// </summary>
public class LevelUpUI : MonoBehaviour
{
    [Header("Panneaux")]
    [SerializeField] private GameObject draftPanel;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject modifierReplacePanel;
    [SerializeField] private TextMeshProUGUI instructionText;

    [Header("Boutons Draft")]
    [SerializeField] private Button rerollButton;
    [SerializeField] private TextMeshProUGUI rerollCostText;
    [SerializeField] private Button banButton;
    [SerializeField] private TextMeshProUGUI banStockText;

    [Header("Boutons Inventory")]
    [SerializeField] private Button backButton;

    [Header("Conteneurs")]
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private Transform inventoryContainer;
    [SerializeField] private Transform replaceContainer;

    [Header("Prefabs")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject replaceButtonPrefab;

    [Header("Data")]
    [SerializeField] private List<SpellForm> availableSpells;
    [SerializeField] private List<SpellEffect> availableEffects;
    [SerializeField] private List<SpellModifier> availableModifiers;
    [SerializeField] private List<StatUpgradeSO> availableStats;

    // Sub-controllers
    private LevelUpDraftController _draftController;
    private LevelUpInventoryController _inventoryController;
    private UpgradeOptionGenerator _optionGenerator;

    // State for POI filters
    private Rarity _currentMinRarity = Rarity.Common;
    private RewardFilter _currentFilter = RewardFilter.Any;

    private void Awake()
    {
        // Get or add sub-components
        _draftController = GetComponent<LevelUpDraftController>();
        if (_draftController == null)
            _draftController = gameObject.AddComponent<LevelUpDraftController>();

        _inventoryController = GetComponent<LevelUpInventoryController>();
        if (_inventoryController == null)
            _inventoryController = gameObject.AddComponent<LevelUpInventoryController>();

        _optionGenerator = GetComponent<UpgradeOptionGenerator>();
        if (_optionGenerator == null)
            _optionGenerator = gameObject.AddComponent<UpgradeOptionGenerator>();
    }

    private void Start()
    {
        // Initialize sub-controllers with references
        _draftController.Initialize(
            this,
            _optionGenerator,
            draftPanel,
            instructionText,
            rerollButton,
            rerollCostText,
            banButton,
            banStockText,
            cardsContainer,
            cardPrefab
        );

        _inventoryController.Initialize(
            this,
            inventoryPanel,
            modifierReplacePanel,
            instructionText,
            inventoryContainer,
            replaceContainer,
            slotPrefab,
            replaceButtonPrefab,
            backButton
        );

        _optionGenerator.Initialize(
            availableSpells,
            availableEffects,
            availableModifiers,
            availableStats
        );

        // Initialize StatUpgradeRegistry for HUD stat icon display
        if (StatUpgradeRegistry.Instance != null)
        {
            StatUpgradeRegistry.Instance.Initialize(availableStats);
        }

        // Register level up listener
        if (LevelManager.Instance != null)
            LevelManager.Instance.OnLevelUp.AddListener(StartLevelUpSequence);

        // Hide all panels initially
        if (draftPanel) draftPanel.SetActive(false);
        if (inventoryPanel) inventoryPanel.SetActive(false);
        if (modifierReplacePanel) modifierReplacePanel.SetActive(false);
        if (instructionText) instructionText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Standard level up sequence (triggered by LevelManager)
    /// </summary>
    public void StartLevelUpSequence()
    {
        // Use GameStateController instead of Time.timeScale
        if (GameStateController.Instance != null)
            GameStateController.Instance.SetState(GameStateController.GameState.LevelingUp);

        // Reset to default filters
        _currentMinRarity = Rarity.Common;
        _currentFilter = RewardFilter.Any;

        ShowDraftPhase();
    }

    /// <summary>
    /// Special reward draft (called by RuneAltar or RewardChest)
    /// </summary>
    public void ShowRewardDraft(Rarity minRarity, RewardFilter filter)
    {
        // Use GameStateController instead of Time.timeScale
        if (GameStateController.Instance != null)
            GameStateController.Instance.SetState(GameStateController.GameState.LevelingUp);

        // Apply filters from POI
        _currentMinRarity = minRarity;
        _currentFilter = filter;

        ShowDraftPhase();
    }

    /// <summary>
    /// Shows the draft phase with upgrade cards (generates new options)
    /// </summary>
    public void ShowDraftPhase()
    {
        _inventoryController.Hide();
        // Force regeneration for new level-ups and rerolls
        _draftController.ShowDraftPhase(_currentMinRarity, _currentFilter, forceRegenerate: true);
    }

    /// <summary>
    /// Called when user selects an upgrade card
    /// </summary>
    public void SelectUpgrade(UpgradeData upgrade)
    {
        // Handle ban mode
        if (_draftController.IsBanMode)
        {
            LevelManager.Instance.BanRune(upgrade.TargetRuneSO.runeName);
            EndLevelUp();
            return;
        }

        // Handle stat boost (instant application)
        if (upgrade.Type == UpgradeType.StatBoost)
        {
            float val = upgrade.UpgradeDefinition.Stats.StatValue;
            PlayerStats.Instance.ApplyUpgrade(upgrade.TargetStat.targetStat, val, upgrade.TargetStat, upgrade.UpgradeDefinition);
            EndLevelUp();
            return;
        }

        // Handle spell forms
        if (upgrade.Type == UpgradeType.NewSpell)
        {
            SpellManager sm = FindFirstObjectByType<SpellManager>();
            bool hasDuplicate = sm.HasForm((SpellForm)upgrade.TargetRuneSO);
            bool hasSpace = sm.CanAddSpell();

            // Auto-add if new spell and space available
            if (!hasDuplicate && hasSpace)
            {
                sm.AddNewSpellWithUpgrade((SpellForm)upgrade.TargetRuneSO, upgrade.UpgradeDefinition);
                EndLevelUp();
                return;
            }

            // Otherwise show targeting phase
            _draftController.Hide();
            _inventoryController.ShowTargetingPhase(upgrade);

            if (hasDuplicate)
                _inventoryController.UpdateInstructionText(SimpleLocalizationHelper.GetDuplicateSpell());
            else
                _inventoryController.UpdateInstructionText(SimpleLocalizationHelper.GetInventoryFull());

            return;
        }

        // Modifiers and Effects always require targeting
        _draftController.Hide();
        _inventoryController.ShowTargetingPhase(upgrade);

        if (upgrade.Type == UpgradeType.Effect)
            _inventoryController.UpdateInstructionText(SimpleLocalizationHelper.GetApplyEffect());
        else if (upgrade.Type == UpgradeType.Modifier)
            _inventoryController.UpdateInstructionText(SimpleLocalizationHelper.GetApplyModifier());
    }

    /// <summary>
    /// Called when user clicks on a spell slot (delegated from SpellSlotUI)
    /// </summary>
    public void OnSlotClicked(int slotIndex, UpgradeData pendingUpgrade)
    {
        _inventoryController.OnSlotClicked(slotIndex, pendingUpgrade);
    }

    /// <summary>
    /// Returns from targeting phase back to draft phase (without regenerating options)
    /// </summary>
    public void ReturnToDraftPhase()
    {
        _inventoryController.Hide();
        // Don't force regeneration - reuse the cached options
        _draftController.ShowDraftPhase(_currentMinRarity, _currentFilter, forceRegenerate: false);
    }

    /// <summary>
    /// Ends the level up sequence and resumes game
    /// </summary>
    public void EndLevelUp()
    {
        _draftController.Hide();
        _inventoryController.Hide();

        // Use GameStateController to resume
        if (GameStateController.Instance != null)
            GameStateController.Instance.Resume();

        if (LevelManager.Instance != null)
            LevelManager.Instance.TriggerNextLevelUp();
    }
}
