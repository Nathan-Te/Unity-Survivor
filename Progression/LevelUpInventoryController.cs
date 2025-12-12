using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the inventory/targeting phase - selecting which spell slot to apply upgrade to.
/// </summary>
public class LevelUpInventoryController : MonoBehaviour
{
    private GameObject inventoryPanel;
    private GameObject modifierReplacePanel;
    private TextMeshProUGUI instructionText;
    private Transform inventoryContainer;
    private Transform replaceContainer;
    private GameObject slotPrefab;
    private GameObject replaceButtonPrefab;

    private LevelUpUI _mainUI;
    private UpgradeData _pendingUpgrade;
    private int _targetSlotIndex;

    public void Initialize(
        LevelUpUI mainUI,
        GameObject inventoryPanelRef,
        GameObject modifierReplacePanelRef,
        TextMeshProUGUI instructionTextRef,
        Transform inventoryContainerRef,
        Transform replaceContainerRef,
        GameObject slotPrefabRef,
        GameObject replaceButtonPrefabRef)
    {
        _mainUI = mainUI;
        inventoryPanel = inventoryPanelRef;
        modifierReplacePanel = modifierReplacePanelRef;
        instructionText = instructionTextRef;
        inventoryContainer = inventoryContainerRef;
        replaceContainer = replaceContainerRef;
        slotPrefab = slotPrefabRef;
        replaceButtonPrefab = replaceButtonPrefabRef;
    }

    /// <summary>
    /// Shows the targeting phase - choose which slot to apply upgrade to
    /// </summary>
    public void ShowTargetingPhase(UpgradeData pendingUpgrade)
    {
        _pendingUpgrade = pendingUpgrade;

        inventoryPanel.SetActive(true);

        if (!instructionText.text.Contains("!"))
            instructionText.text = $"Appliquer {_pendingUpgrade.Name} sur ?";

        // Clear old slots
        foreach (Transform child in inventoryContainer)
        {
            Destroy(child.gameObject);
        }

        SpellManager sm = FindFirstObjectByType<SpellManager>();
        List<SpellSlot> slots = sm.GetSlots();

        // Show all slots (occupied + empty)
        for (int i = 0; i < sm.MaxSlots; i++)
        {
            GameObject obj = Instantiate(slotPrefab, inventoryContainer);
            if (obj.TryGetComponent<SpellSlotUI>(out var ui))
            {
                if (i < slots.Count)
                    ui.Initialize(slots[i], i, _mainUI, _pendingUpgrade);
                else
                    ui.InitializeEmpty(i, _mainUI, _pendingUpgrade);
            }
        }
    }

    /// <summary>
    /// Hides inventory and replace panels
    /// </summary>
    public void Hide()
    {
        inventoryPanel.SetActive(false);
        if (modifierReplacePanel)
            modifierReplacePanel.SetActive(false);
    }

    /// <summary>
    /// Called when user clicks on a spell slot
    /// </summary>
    public void OnSlotClicked(int slotIndex, UpgradeData pendingUpgrade)
    {
        _targetSlotIndex = slotIndex;
        _pendingUpgrade = pendingUpgrade;

        SpellManager sm = FindFirstObjectByType<SpellManager>();
        List<SpellSlot> slots = sm.GetSlots();

        bool isOccupied = slotIndex < slots.Count;
        SpellSlot slot = isOccupied ? slots[slotIndex] : null;

        // CASE 1: FORM (SPELL)
        if (_pendingUpgrade.Type == UpgradeType.NewSpell)
        {
            HandleFormUpgrade(sm, slotIndex, isOccupied, slot);
        }
        // CASE 2: EFFECT
        else if (_pendingUpgrade.Type == UpgradeType.Effect && isOccupied)
        {
            sm.ApplyEffectToSlot((SpellEffect)_pendingUpgrade.TargetRuneSO, slotIndex, _pendingUpgrade.UpgradeDefinition);
            _mainUI.EndLevelUp();
        }
        // CASE 3: MODIFIER
        else if (_pendingUpgrade.Type == UpgradeType.Modifier && isOccupied)
        {
            HandleModifierUpgrade(sm, slotIndex, slot);
        }
    }

    private void HandleFormUpgrade(SpellManager sm, int slotIndex, bool isOccupied, SpellSlot slot)
    {
        // A. Click empty -> Create new
        if (!isOccupied)
        {
            sm.AddNewSpellWithUpgrade((SpellForm)_pendingUpgrade.TargetRuneSO, _pendingUpgrade.UpgradeDefinition);
            _mainUI.EndLevelUp();
        }
        // B. Click occupied (same form) -> UPGRADE
        else if (slot.formRune.Data == _pendingUpgrade.TargetRuneSO)
        {
            sm.UpgradeSpellAtSlot(slotIndex, _pendingUpgrade.UpgradeDefinition);
            _mainUI.EndLevelUp();
        }
        // C. Click occupied (different form) -> REPLACE
        else
        {
            sm.ReplaceSpell((SpellForm)_pendingUpgrade.TargetRuneSO, slotIndex, _pendingUpgrade.UpgradeDefinition);
            _mainUI.EndLevelUp();
        }
    }

    private void HandleModifierUpgrade(SpellManager sm, int slotIndex, SpellSlot slot)
    {
        bool success = sm.TryApplyModifierToSlot(
            (SpellModifier)_pendingUpgrade.TargetRuneSO,
            slotIndex,
            -1,
            _pendingUpgrade.UpgradeDefinition
        );

        if (success)
        {
            _mainUI.EndLevelUp();
        }
        else
        {
            // Check incompatibility
            if (_pendingUpgrade.TargetModifier.requiredTag != SpellTag.None &&
                !slot.formRune.AsForm.tags.HasFlag(_pendingUpgrade.TargetModifier.requiredTag))
            {
                instructionText.text = "Incompatible avec cette forme !";
                return;
            }

            // Check if modifier slots are actually full
            if (slot.modifierRunes != null)
            {
                bool hasFreeSlot = false;
                foreach (var modRune in slot.modifierRunes)
                {
                    if (modRune == null || modRune.Data == null)
                    {
                        hasFreeSlot = true;
                        break;
                    }
                }

                if (!hasFreeSlot)
                {
                    // Slot full -> Show replace menu
                    ShowReplaceMenu(slot);
                }
                else
                {
                    // There's a free slot but TryApplyModifierToSlot failed
                    instructionText.text = "Erreur lors de l'ajout du modificateur";
                    Debug.LogError($"TryApplyModifierToSlot failed but slot has free space. Slot modifiers: {slot.modifierRunes.Length}");
                }
            }
        }
    }

    private void ShowReplaceMenu(SpellSlot slot)
    {
        inventoryPanel.SetActive(false);
        if (modifierReplacePanel)
            modifierReplacePanel.SetActive(true);

        instructionText.text = "Remplacer quel Modificateur ?";

        // Clear old buttons
        foreach (Transform child in replaceContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < slot.modifierRunes.Length; i++)
        {
            if (slot.modifierRunes[i] == null || slot.modifierRunes[i].Data == null)
                continue;

            GameObject btnObj = Instantiate(replaceButtonPrefab, replaceContainer);
            var txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            var btn = btnObj.GetComponent<Button>();

            txt.text = slot.modifierRunes[i].Data.runeName;

            int indexToReplace = i;
            btn.onClick.AddListener(() =>
            {
                FindFirstObjectByType<SpellManager>().TryApplyModifierToSlot(
                    (SpellModifier)_pendingUpgrade.TargetRuneSO,
                    _targetSlotIndex,
                    indexToReplace,
                    _pendingUpgrade.UpgradeDefinition
                );
                _mainUI.EndLevelUp();
            });
        }
    }

    public void UpdateInstructionText(string text)
    {
        instructionText.text = text;
    }
}
