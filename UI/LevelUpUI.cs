using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUpUI : MonoBehaviour
{
    [Header("Panneaux")]
    [SerializeField] private GameObject draftPanel;    // Contient les 3 cartes
    [SerializeField] private GameObject inventoryPanel;// Contient les slots du joueur pour choisir
    [SerializeField] private TextMeshProUGUI instructionText; // "Choisissez une amélioration" vs "Choisissez un slot"

    [Header("Conteneurs")]
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private Transform inventoryContainer; // Grid Layout pour les slots

    [Header("Prefabs")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject slotPrefab;

    [Header("Data")]
    [SerializeField] private List<SpellForm> availableSpells;
    [SerializeField] private List<SpellModifier> availableModifiers;

    // État temporaire
    private UpgradeData _pendingUpgrade;

    private void Start()
    {
        if (LevelManager.Instance != null) LevelManager.Instance.OnLevelUp.AddListener(StartLevelUpSequence);

        draftPanel.SetActive(false);
        inventoryPanel.SetActive(false);
        instructionText.gameObject.SetActive(false); // <--- AJOUT : Caché au départ
    }

    public void StartLevelUpSequence()
    {
        Time.timeScale = 0f;
        _pendingUpgrade = null;

        ShowDraftPhase();
    }

    // PHASE 1 : Afficher les 3 cartes
    private void ShowDraftPhase()
    {
        draftPanel.SetActive(true);
        inventoryPanel.SetActive(false);

        instructionText.gameObject.SetActive(true);
        instructionText.text = "LEVEL UP ! Choisissez une récompense";

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
    }

    // Appelé par la carte quand on clique dessus
    public void SelectUpgrade(UpgradeData upgrade)
    {
        _pendingUpgrade = upgrade;

        if (upgrade.Type == UpgradeType.NewSpell)
        {
            // Si c'est un nouveau sort, on l'ajoute direct (ou on demande de remplacer si inventaire plein)
            // Pour l'instant : Ajout direct
            FindFirstObjectByType<SpellManager>().AddNewSlot(upgrade.TargetForm);
            EndLevelUp();
        }
        else if (upgrade.Type == UpgradeType.Modifier) // Ou Effect
        {
            // Si c'est un modificateur, on doit choisir où le mettre
            ShowTargetingPhase();
        }
    }

    // PHASE 2 : Afficher l'inventaire pour choisir la cible
    private void ShowTargetingPhase()
    {
        draftPanel.SetActive(false);
        inventoryPanel.SetActive(true);

        instructionText.gameObject.SetActive(true);
        instructionText.text = $"Où appliquer {_pendingUpgrade.Name} ?";

        // On affiche les slots actuels du joueur
        foreach (Transform child in inventoryContainer) Destroy(child.gameObject);

        SpellManager sm = FindFirstObjectByType<SpellManager>();
        List<SpellSlot> slots = sm.GetSlots();

        for (int i = 0; i < slots.Count; i++)
        {
            GameObject obj = Instantiate(slotPrefab, inventoryContainer);
            if (obj.TryGetComponent<SpellSlotUI>(out var ui))
            {
                // On passe 'this' pour dire que c'est cliquable et que ça rappelera OnSlotClicked
                ui.Initialize(slots[i], i, this);
            }
        }

        // Ajouter un bouton "Retour" ou "Annuler" serait bien ici
    }

    // Appelé par SpellSlotUI quand on clique sur un slot
    public void OnSlotClicked(int slotIndex)
    {
        SpellManager sm = FindFirstObjectByType<SpellManager>();

        if (_pendingUpgrade.Type == UpgradeType.Modifier)
        {
            bool success = sm.TryAddModifierToSlot(_pendingUpgrade.TargetModifier, slotIndex);

            if (success)
            {
                EndLevelUp();
            }
            else
            {
                // Feedback visuel : "Impossible !"
                instructionText.text = "Incompatible avec ce sort !";
                // Animation de shake ou son d'erreur
            }
        }
    }

    private void EndLevelUp()
    {
        draftPanel.SetActive(false);
        inventoryPanel.SetActive(false);
        instructionText.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    // ... (GenerateOptions reste inchangé) ...
    private List<UpgradeData> GenerateOptions(int count)
    {
        List<UpgradeData> picks = new List<UpgradeData>();
        for (int i = 0; i < count; i++)
        {
            bool pickSpell = Random.value > 0.5f;
            if (pickSpell && availableSpells.Count > 0)
                picks.Add(new UpgradeData(availableSpells[Random.Range(0, availableSpells.Count)]));
            else if (availableModifiers.Count > 0)
                picks.Add(new UpgradeData(availableModifiers[Random.Range(0, availableModifiers.Count)]));
        }
        return picks;
    }
}