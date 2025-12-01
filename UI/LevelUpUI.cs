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
    [SerializeField] private List<StatUpgradeSO> availableStats; // NOUVEAU

    private UpgradeData _pendingUpgrade;
    private int _targetSlotIndex;
    private bool _isBanMode = false;

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

    public void StartLevelUpSequence()
    {
        Time.timeScale = 0f;
        _pendingUpgrade = null;
        _isBanMode = false;
        ShowDraftPhase();
    }

    private void ShowDraftPhase()
    {
        draftPanel.SetActive(true);
        inventoryPanel.SetActive(false);
        if (modifierReplacePanel) modifierReplacePanel.SetActive(false);
        UpdateDraftButtons();

        foreach (Transform child in cardsContainer) Destroy(child.gameObject);

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
        instructionText.text = _isBanMode ? "BANNISSEMENT : Cliquez sur une carte" : "LEVEL UP ! Choisissez une récompense";
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

        // CAS 0 : Stat Passive (Immédiat)
        if (upgrade.Type == UpgradeType.StatBoost)
        {
            // On récupère la valeur depuis la définition piochée
            float val = upgrade.UpgradeDefinition.Stats.StatValue;
            PlayerStats.Instance.ApplyUpgrade(upgrade.TargetStat.targetStat, val);
            EndLevelUp();
        }
        // CAS 1 : Nouveau Sort (Forme)
        else if (upgrade.Type == UpgradeType.NewSpell)
        {
            if (sm.CanAddSpell())
            {
                sm.AddNewSpellWithUpgrade((SpellForm)upgrade.TargetRuneSO, upgrade.UpgradeDefinition);
                EndLevelUp();
            }
            else
            {
                ShowTargetingPhase();
                instructionText.text = "Inventaire Plein ! Remplacer quel sort ?";
            }
        }
        // CAS 2 : Upgrade Forme existante
        else if (upgrade.Type == UpgradeType.SpellUpgrade)
        {
            sm.UpgradeExistingForm((SpellForm)upgrade.TargetRuneSO, upgrade.UpgradeDefinition);
            EndLevelUp();
        }
        // CAS 3 : Modif ou Effet
        else
        {
            ShowTargetingPhase();
        }
    }

    private void ShowTargetingPhase()
    {
        draftPanel.SetActive(false);
        inventoryPanel.SetActive(true);
        instructionText.text = $"Appliquer {_pendingUpgrade.Name} sur ?";

        foreach (Transform child in inventoryContainer) Destroy(child.gameObject);

        SpellManager sm = FindFirstObjectByType<SpellManager>();
        List<SpellSlot> slots = sm.GetSlots();

        for (int i = 0; i < slots.Count; i++)
        {
            GameObject obj = Instantiate(slotPrefab, inventoryContainer);
            if (obj.TryGetComponent<SpellSlotUI>(out var ui))
            {
                ui.Initialize(slots[i], i, this);
            }
        }
    }

    public void OnSlotClicked(int slotIndex)
    {
        _targetSlotIndex = slotIndex;
        SpellManager sm = FindFirstObjectByType<SpellManager>();
        SpellSlot slot = sm.GetSlots()[slotIndex];

        if (_pendingUpgrade.Type == UpgradeType.NewSpell)
        {
            sm.ReplaceSpell((SpellForm)_pendingUpgrade.TargetRuneSO, slotIndex, _pendingUpgrade.UpgradeDefinition);
            EndLevelUp();
        }
        else if (_pendingUpgrade.Type == UpgradeType.Effect)
        {
            sm.ApplyEffectToSlot((SpellEffect)_pendingUpgrade.TargetRuneSO, slotIndex, _pendingUpgrade.UpgradeDefinition);
            EndLevelUp();
        }
        else if (_pendingUpgrade.Type == UpgradeType.Modifier)
        {
            bool success = sm.TryApplyModifierToSlot((SpellModifier)_pendingUpgrade.TargetRuneSO, slotIndex, -1, _pendingUpgrade.UpgradeDefinition);

            if (success)
            {
                EndLevelUp();
            }
            else
            {
                if (_pendingUpgrade.TargetModifier.requiredTag != SpellTag.None &&
                    !slot.formRune.AsForm.tags.HasFlag(_pendingUpgrade.TargetModifier.requiredTag))
                {
                    instructionText.text = "Incompatible avec cette forme !";
                    return;
                }
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
        SpellManager sm = FindFirstObjectByType<SpellManager>();

        while (picks.Count < count && attempts < 100)
        {
            attempts++;
            float r = Random.value;
            UpgradeData candidate = null;
            Rarity rarity = RarityUtils.GetRandomRarity();

            // Pondération : 20% Spell, 20% Effect, 30% Mod, 30% Stat
            if (r < 0.2f && availableSpells.Count > 0)
                candidate = new UpgradeData(availableSpells[Random.Range(0, availableSpells.Count)], rarity);
            else if (r < 0.4f && availableEffects.Count > 0)
                candidate = new UpgradeData(availableEffects[Random.Range(0, availableEffects.Count)], rarity);
            else if (r < 0.7f && availableModifiers.Count > 0)
                candidate = new UpgradeData(availableModifiers[Random.Range(0, availableModifiers.Count)], rarity);
            else if (availableStats != null && availableStats.Count > 0) // Stats
                candidate = new UpgradeData(availableStats[Random.Range(0, availableStats.Count)], rarity);

            if (candidate != null)
            {
                if (LevelManager.Instance.IsRuneBanned(candidate.Name)) continue;
                if (picks.Exists(x => x.TargetRuneSO == candidate.TargetRuneSO)) continue;

                // Check Upgrade Forme
                if (candidate.Type == UpgradeType.NewSpell && sm.HasForm((SpellForm)candidate.TargetRuneSO))
                {
                    candidate.SetAsUpgrade();
                }

                picks.Add(candidate);
            }
        }
        return picks;
    }
}