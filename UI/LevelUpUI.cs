using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private UpgradeData _pendingUpgrade;
    private int _targetSlotIndex;
    private bool _isBanMode = false;

    // --- ÉTAT POI (FILTRES) ---
    private Rarity _currentMinRarity = Rarity.Common;
    private RewardFilter _currentFilter = RewardFilter.Any;

    private void Start()
    {
        if (LevelManager.Instance != null) LevelManager.Instance.OnLevelUp.AddListener(StartLevelUpSequence);

        draftPanel.SetActive(false);
        inventoryPanel.SetActive(false);
        if (modifierReplacePanel) modifierReplacePanel.SetActive(false);
        instructionText.gameObject.SetActive(false);

        rerollButton.onClick.AddListener(OnRerollClicked);
        banButton.onClick.AddListener(OnBanModeClicked);
    }

    // 1. LEVEL UP STANDARD
    public void StartLevelUpSequence()
    {
        Time.timeScale = 0f;
        _pendingUpgrade = null;
        _isBanMode = false;

        // Reset des filtres par défaut
        _currentMinRarity = Rarity.Common;
        _currentFilter = RewardFilter.Any;

        ShowDraftPhase();
    }

    // 2. APPELÉ PAR L'AUTEL (REWARD CHEST)
    // Cette méthode avait disparu, la voici restaurée !
    public void ShowRewardDraft(Rarity minRarity, RewardFilter filter)
    {
        Time.timeScale = 0f;
        _pendingUpgrade = null;
        _isBanMode = false;

        // Application des filtres demandés par l'autel
        _currentMinRarity = minRarity;
        _currentFilter = filter;

        ShowDraftPhase();
    }

    private void ShowDraftPhase()
    {
        draftPanel.SetActive(true);
        inventoryPanel.SetActive(false);
        if (modifierReplacePanel) modifierReplacePanel.SetActive(false);
        UpdateDraftButtons();

        foreach (Transform child in cardsContainer) Destroy(child.gameObject);

        // On utilise les filtres actuels
        List<UpgradeData> options = GenerateOptions(3);

        foreach (var option in options)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardsContainer);
            if (cardObj.TryGetComponent<UpgradeCard>(out var card))
            {
                card.Initialize(option, this);
            }
        }

        instructionText.gameObject.SetActive(true);

        // Titre contextuel
        if (_currentFilter != RewardFilter.Any || _currentMinRarity > Rarity.Common)
            instructionText.text = "RÉCOMPENSE SPÉCIALE !";
        else
            instructionText.text = "LEVEL UP ! Choisissez une récompense";
    }

    private void UpdateDraftButtons()
    {
        if (rerollCostText) rerollCostText.text = $"Reroll ({LevelManager.Instance.availableRerolls})";
        rerollButton.interactable = LevelManager.Instance.availableRerolls > 0;

        if (banStockText) banStockText.text = $"Ban ({LevelManager.Instance.availableBans})";
        banButton.interactable = LevelManager.Instance.availableBans > 0;

        banButton.image.color = _isBanMode ? Color.red : Color.white;
    }

    public void OnRerollClicked()
    {
        if (LevelManager.Instance.ConsumeReroll()) ShowDraftPhase();
    }

    public void OnBanModeClicked()
    {
        _isBanMode = !_isBanMode;
        UpdateDraftButtons();
        instructionText.text = _isBanMode ? "BANNISSEMENT : Cliquez sur une carte" : "CHOISISSEZ UNE RÉCOMPENSE";
    }

    public void SelectUpgrade(UpgradeData upgrade)
    {
        if (_isBanMode)
        {
            LevelManager.Instance.BanRune(upgrade.TargetRuneSO.runeName);
            EndLevelUp();
            return;
        }

        _pendingUpgrade = upgrade;
        SpellManager sm = FindFirstObjectByType<SpellManager>();

        if (upgrade.Type == UpgradeType.StatBoost)
        {
            float val = upgrade.UpgradeDefinition.Stats.StatValue;
            PlayerStats.Instance.ApplyUpgrade(upgrade.TargetStat.targetStat, val);
            EndLevelUp();
        }
        else if (upgrade.Type == UpgradeType.NewSpell)
        {
            bool hasDuplicate = sm.HasForm((SpellForm)upgrade.TargetRuneSO);
            bool hasSpace = sm.CanAddSpell();

            // Cas A : Nouveau et Place dispo -> Auto Add
            if (!hasDuplicate && hasSpace)
            {
                sm.AddNewSpellWithUpgrade((SpellForm)upgrade.TargetRuneSO, upgrade.UpgradeDefinition);
                EndLevelUp();
            }
            // Cas B : Doublon ou Plein -> Choix
            else
            {
                ShowTargetingPhase();
                if (hasDuplicate) instructionText.text = "DOUBLON : Améliorer ou Dupliquer ?";
                else instructionText.text = "INVENTAIRE PLEIN : Remplacer quel sort ?";
            }
        }
        else
        {
            // Modifiers et Effets demandent toujours une cible
            ShowTargetingPhase();
        }
    }

    private void ShowTargetingPhase()
    {
        draftPanel.SetActive(false);
        inventoryPanel.SetActive(true);

        if (!instructionText.text.Contains("!"))
            instructionText.text = $"Appliquer {_pendingUpgrade.Name} sur ?";

        foreach (Transform child in inventoryContainer) Destroy(child.gameObject);

        SpellManager sm = FindFirstObjectByType<SpellManager>();
        List<SpellSlot> slots = sm.GetSlots();

        // Affiche tous les slots (occupés + vides)
        for (int i = 0; i < sm.MaxSlots; i++)
        {
            GameObject obj = Instantiate(slotPrefab, inventoryContainer);
            if (obj.TryGetComponent<SpellSlotUI>(out var ui))
            {
                if (i < slots.Count) ui.Initialize(slots[i], i, this);
                else ui.InitializeEmpty(i, this);
            }
        }
    }

    public void OnSlotClicked(int slotIndex)
    {
        _targetSlotIndex = slotIndex;
        SpellManager sm = FindFirstObjectByType<SpellManager>();
        List<SpellSlot> slots = sm.GetSlots();

        bool isOccupied = slotIndex < slots.Count;
        SpellSlot slot = isOccupied ? slots[slotIndex] : null;

        // CAS 1 : FORME (SORT)
        if (_pendingUpgrade.Type == UpgradeType.NewSpell)
        {
            // A. Clic Vide -> Création
            if (!isOccupied)
            {
                sm.AddNewSpellWithUpgrade((SpellForm)_pendingUpgrade.TargetRuneSO, _pendingUpgrade.UpgradeDefinition);
                EndLevelUp();
            }
            // B. Clic Occupé (Même forme) -> UPGRADE
            else if (slot.formRune.Data == _pendingUpgrade.TargetRuneSO)
            {
                sm.UpgradeSpellAtSlot(slotIndex, _pendingUpgrade.UpgradeDefinition);
                EndLevelUp();
            }
            // C. Clic Occupé (Autre forme) -> REMPLACEMENT
            else
            {
                sm.ReplaceSpell((SpellForm)_pendingUpgrade.TargetRuneSO, slotIndex, _pendingUpgrade.UpgradeDefinition);
                EndLevelUp();
            }
        }
        // CAS 2 : EFFET
        else if (_pendingUpgrade.Type == UpgradeType.Effect && isOccupied)
        {
            sm.ApplyEffectToSlot((SpellEffect)_pendingUpgrade.TargetRuneSO, slotIndex, _pendingUpgrade.UpgradeDefinition);
            EndLevelUp();
        }
        // CAS 3 : MODIFICATEUR
        else if (_pendingUpgrade.Type == UpgradeType.Modifier && isOccupied)
        {
            bool success = sm.TryApplyModifierToSlot((SpellModifier)_pendingUpgrade.TargetRuneSO, slotIndex, -1, _pendingUpgrade.UpgradeDefinition);

            if (success)
            {
                EndLevelUp();
            }
            else
            {
                // Vérif Incompatibilité
                if (_pendingUpgrade.TargetModifier.requiredTag != SpellTag.None &&
                    !slot.formRune.AsForm.tags.HasFlag(_pendingUpgrade.TargetModifier.requiredTag))
                {
                    instructionText.text = "Incompatible avec cette forme !";
                    return;
                }
                // Slot plein -> Menu Remplacement
                ShowReplaceMenu(slot);
            }
        }
    }

    private void ShowReplaceMenu(SpellSlot slot)
    {
        inventoryPanel.SetActive(false);
        if (modifierReplacePanel) modifierReplacePanel.SetActive(true);
        instructionText.text = "Remplacer quel Modificateur ?";

        foreach (Transform child in replaceContainer) Destroy(child.gameObject);

        for (int i = 0; i < slot.modifierRunes.Length; i++)
        {
            if (slot.modifierRunes[i] == null || slot.modifierRunes[i].Data == null) continue;

            GameObject btnObj = Instantiate(replaceButtonPrefab, replaceContainer);
            var txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            var btn = btnObj.GetComponent<Button>();

            txt.text = slot.modifierRunes[i].Data.runeName;

            int indexToReplace = i;
            btn.onClick.AddListener(() => {
                FindFirstObjectByType<SpellManager>().TryApplyModifierToSlot(
                    (SpellModifier)_pendingUpgrade.TargetRuneSO,
                    _targetSlotIndex,
                    indexToReplace,
                    _pendingUpgrade.UpgradeDefinition
                );
                EndLevelUp();
            });
        }
    }

    private void EndLevelUp()
    {
        draftPanel.SetActive(false);
        inventoryPanel.SetActive(false);
        if (modifierReplacePanel) modifierReplacePanel.SetActive(false);
        instructionText.gameObject.SetActive(false);

        Time.timeScale = 1f;
        if (LevelManager.Instance != null) LevelManager.Instance.TriggerNextLevelUp();
    }

    private List<UpgradeData> GenerateOptions(int count)
    {
        List<UpgradeData> picks = new List<UpgradeData>();
        int attempts = 0;

        while (picks.Count < count && attempts < 200)
        {
            attempts++;

            // --- LOGIQUE DE FILTRAGE RESTAURÉE ---

            // 1. Rareté Pondérée avec Min
            Rarity rarity = RarityUtils.GetRandomRarityAtLeast(_currentMinRarity);

            // 2. Filtre de Type
            bool pickSpell = (_currentFilter == RewardFilter.Any) ? (Random.value < 0.2f) : (_currentFilter == RewardFilter.Form);
            bool pickEffect = (_currentFilter == RewardFilter.Any) ? (Random.value < 0.5f && !pickSpell) : (_currentFilter == RewardFilter.Effect);
            bool pickMod = (_currentFilter == RewardFilter.Any) ? (Random.value < 0.7f && !pickSpell && !pickEffect) : (_currentFilter == RewardFilter.Modifier);
            bool pickStat = (_currentFilter == RewardFilter.Any) ? (!pickSpell && !pickEffect && !pickMod) : (_currentFilter == RewardFilter.Stat);

            RuneSO selectedSO = null;

            if (pickSpell && availableSpells.Count > 0)
                selectedSO = availableSpells[Random.Range(0, availableSpells.Count)];

            else if (pickEffect && availableEffects.Count > 0)
                selectedSO = availableEffects[Random.Range(0, availableEffects.Count)];

            else if (pickMod && availableModifiers.Count > 0)
                selectedSO = availableModifiers[Random.Range(0, availableModifiers.Count)];

            else if (pickStat && availableStats != null && availableStats.Count > 0)
                selectedSO = availableStats[Random.Range(0, availableStats.Count)];

            if (selectedSO != null)
            {
                if (LevelManager.Instance.IsRuneBanned(selectedSO.runeName)) continue;
                if (picks.Exists(x => x.TargetRuneSO == selectedSO)) continue;

                UpgradeData data = new UpgradeData(selectedSO, rarity);

                // NOTE: On ne force plus SetAsUpgrade() ici, 
                // on laisse SelectUpgrade() gérer le choix Doublon/Nouveau.

                picks.Add(data);
            }
        }
        return picks;
    }
}