using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUpUI : MonoBehaviour
{
    [Header("Panneaux")]
    [SerializeField] private GameObject draftPanel;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject modifierReplacePanel; // NOUVEAU : Popup "Quel mod remplacer ?"
    [SerializeField] private TextMeshProUGUI instructionText;

    [Header("Boutons Draft")]
    [SerializeField] private Button rerollButton;
    [SerializeField] private TextMeshProUGUI rerollCostText;
    [SerializeField] private Button banButton; // Toggle mode Ban
    [SerializeField] private TextMeshProUGUI banStockText;

    [Header("Conteneurs")]
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private Transform inventoryContainer;
    [SerializeField] private Transform replaceContainer; // Pour les boutons de remplacement

    [Header("Prefabs")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject replaceButtonPrefab; // Bouton simple avec texte/icone

    [Header("Data")]
    [SerializeField] private List<SpellForm> availableSpells;
    [SerializeField] private List<SpellEffect> availableEffects; // NOUVEAU
    [SerializeField] private List<SpellModifier> availableModifiers;

    private UpgradeData _pendingUpgrade;
    private int _targetSlotIndex;
    private bool _isBanMode = false;

    private void Start()
    {
        if (LevelManager.Instance != null) LevelManager.Instance.OnLevelUp.AddListener(StartLevelUpSequence);

        draftPanel.SetActive(false);
        inventoryPanel.SetActive(false);
        if (modifierReplacePanel) modifierReplacePanel.SetActive(false);

        // Setup Boutons
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

        // Nettoyage
        foreach (Transform child in cardsContainer) Destroy(child.gameObject);

        // Génération
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
        // Mise à jour visuelle des stocks
        if (rerollCostText) rerollCostText.text = $"Reroll ({LevelManager.Instance.availableRerolls})";
        rerollButton.interactable = LevelManager.Instance.availableRerolls > 0;

        if (banStockText) banStockText.text = $"Ban ({LevelManager.Instance.availableBans})";
        banButton.interactable = LevelManager.Instance.availableBans > 0;

        // Visuel mode Ban activé (changement de couleur par ex)
        banButton.image.color = _isBanMode ? Color.red : Color.white;
    }

    public void OnRerollClicked()
    {
        if (LevelManager.Instance.ConsumeReroll())
        {
            ShowDraftPhase(); // Régénère
        }
    }

    public void OnBanModeClicked()
    {
        _isBanMode = !_isBanMode;
        UpdateDraftButtons();
        instructionText.text = _isBanMode ? "CLIQUEZ SUR UNE CARTE POUR LA BANNIR" : "LEVEL UP ! Choisissez une récompense";
    }

    // Appelé par la carte
    // Appelé par la carte quand on clique dessus
    public void SelectUpgrade(UpgradeData upgrade)
    {
        // MODE BAN (Inchangé)
        if (_isBanMode)
        {
            LevelManager.Instance.BanRune(upgrade.Name);
            EndLevelUp();
            return;
        }

        // MODE NORMAL
        _pendingUpgrade = upgrade;
        SpellManager sm = FindFirstObjectByType<SpellManager>();

        if (upgrade.Type == UpgradeType.NewSpell)
        {
            if (sm.CanAddSpell())
            {
                // Cas 1 : Il y a de la place -> Ajout direct
                sm.AddSpell(upgrade.TargetForm);
                EndLevelUp();
            }
            else
            {
                // Cas 2 : Plein -> On demande de remplacer un sort existant
                ShowTargetingPhase();
                instructionText.text = "Inventaire Plein ! Choisissez un sort à remplacer";
            }
        }
        else if (upgrade.Type == UpgradeType.Modifier || upgrade.Type == UpgradeType.Effect)
        {
            // Cas 3 : Amélioration (Mod ou Effet) -> On choisit la cible
            ShowTargetingPhase();
        }
    }

    private void ShowTargetingPhase()
    {
        draftPanel.SetActive(false);
        inventoryPanel.SetActive(true);
        instructionText.text = $"Où appliquer {_pendingUpgrade.Name} ?";

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

    // Appelé par SpellSlotUI quand on clique sur un slot
    public void OnSlotClicked(int slotIndex)
    {
        _targetSlotIndex = slotIndex;
        SpellManager sm = FindFirstObjectByType<SpellManager>();
        SpellSlot slot = sm.GetSlots()[slotIndex];

        // CAS A : REMPLACEMENT DE SORT (Inventaire Plein)
        if (_pendingUpgrade.Type == UpgradeType.NewSpell)
        {
            sm.ReplaceSpell(_pendingUpgrade.TargetForm, slotIndex);
            EndLevelUp();
        }
        // CAS B : CHANGEMENT D'EFFET (Élément)
        else if (_pendingUpgrade.Type == UpgradeType.Effect)
        {
            sm.ReplaceEffect(_pendingUpgrade.TargetEffect, slotIndex);
            EndLevelUp();
        }
        // CAS C : AJOUT DE MODIFICATEUR (Inchangé)
        else if (_pendingUpgrade.Type == UpgradeType.Modifier)
        {
            // On essaie d'ajouter (sans forcer le remplacement)
            bool success = sm.TryApplyModifierToSlot(_pendingUpgrade.TargetModifier, slotIndex, -1);

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

                // Si compatible mais plein -> Menu Remplacement
                ShowReplaceMenu(slot);
            }
        }
    }

    private void ShowReplaceMenu(SpellSlot slot)
    {
        inventoryPanel.SetActive(false);
        if (modifierReplacePanel != null) modifierReplacePanel.SetActive(true);
        if (instructionText != null) instructionText.text = "Quel Modificateur remplacer ?";

        foreach (Transform child in replaceContainer) Destroy(child.gameObject);

        for (int i = 0; i < slot.modifierRunes.Length; i++)
        {
            // CORRECTION : On saute les slots vides ou mal initialisés
            if (slot.modifierRunes[i] == null || slot.modifierRunes[i].Data == null) continue;

            GameObject btnObj = Instantiate(replaceButtonPrefab, replaceContainer);
            var btn = btnObj.GetComponent<Button>();
            var txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();

            // Maintenant c'est safe car on a vérifié Data != null
            txt.text = slot.modifierRunes[i].Data.runeName;

            int indexToReplace = i;
            btn.onClick.AddListener(() => {
                FindFirstObjectByType<SpellManager>().TryApplyModifierToSlot(_pendingUpgrade.TargetModifier, _targetSlotIndex, indexToReplace);
                EndLevelUp();
            });
        }

        // Optionnel : Ajouter un bouton "Annuler" pour sortir de ce menu
    }

    private void EndLevelUp()
    {
        draftPanel.SetActive(false);
        inventoryPanel.SetActive(false);
        if (modifierReplacePanel) modifierReplacePanel.SetActive(false);
        instructionText.gameObject.SetActive(false);

        Time.timeScale = 1f;

        // On vérifie s'il reste des niveaux en attente
        if (LevelManager.Instance != null)
        {
            // Petite pause d'une frame ou appel direct ? Direct pour l'instant.
            LevelManager.Instance.TriggerNextLevelUp();
        }
    }

    private List<UpgradeData> GenerateOptions(int count)
    {
        List<UpgradeData> picks = new List<UpgradeData>();
        int attempts = 0;

        while (picks.Count < count && attempts < 100)
        {
            attempts++;

            // Tirage aléatoire pondéré (exemple simple)
            float r = Random.value;
            UpgradeData candidate = null;

            if (r < 0.2f && availableSpells.Count > 0) // 20% Spell
                candidate = new UpgradeData(availableSpells[Random.Range(0, availableSpells.Count)]);
            else if (r < 0.5f && availableEffects.Count > 0) // 30% Effect
                candidate = new UpgradeData(availableEffects[Random.Range(0, availableEffects.Count)]);
            else if (availableModifiers.Count > 0) // 50% Modifier
                candidate = new UpgradeData(availableModifiers[Random.Range(0, availableModifiers.Count)]);

            if (candidate != null)
            {
                // Vérification Ban
                if (LevelManager.Instance.IsRuneBanned(candidate.Name)) continue;

                // Vérif Doublon dans le tirage
                if (picks.Exists(x => x.Name == candidate.Name)) continue;

                picks.Add(candidate);
            }
        }
        return picks;
    }
}