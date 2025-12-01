using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpellSlotUI : MonoBehaviour
{
    [Header("Composants UI")]
    [SerializeField] private Image formIcon;
    [SerializeField] private TextMeshProUGUI formLevelText;
    [SerializeField] private Image effectIcon;
    [SerializeField] private TextMeshProUGUI effectLevelText; // Assure-toi que c'est lié
    [SerializeField] private Image[] modIcons;
    [SerializeField] private TextMeshProUGUI[] modLevelTexts; // Assure-toi que c'est lié

    [SerializeField] private Button clickButton;
    [SerializeField] private GameObject emptyStateVisual; // (Optionnel) un texte ou image "Empty"

    private int _slotIndex;
    private LevelUpUI _levelUpManager;

    // Affichage Standard
    public void Initialize(SpellSlot slot, int index, LevelUpUI levelUpManager = null)
    {
        _slotIndex = index;
        _levelUpManager = levelUpManager;

        if (emptyStateVisual) emptyStateVisual.SetActive(false); // On cache l'état vide

        // 1. Forme
        if (slot.formRune != null && slot.formRune.Data.icon != null)
        {
            formIcon.sprite = slot.formRune.Data.icon;
            formIcon.enabled = true;
            formIcon.color = Color.white;
            if (formLevelText) formLevelText.text = $"Lvl {slot.formRune.Level}";
        }

        // 2. Effet
        if (slot.effectRune != null)
        {
            if (slot.effectRune.Data.icon != null)
            {
                effectIcon.sprite = slot.effectRune.Data.icon;
                effectIcon.color = Color.white;
            }
            else
            {
                effectIcon.sprite = null;
                effectIcon.color = slot.effectRune.AsEffect.tintColor;
            }
            effectIcon.enabled = true;
            if (effectLevelText) effectLevelText.text = $"{slot.effectRune.Level}";
        }

        // 3. Mods
        for (int i = 0; i < modIcons.Length; i++)
        {
            bool hasMod = (i < slot.modifierRunes.Length && slot.modifierRunes[i] != null && slot.modifierRunes[i].Data != null);

            if (hasMod)
            {
                Rune modRune = slot.modifierRunes[i];
                if (modRune.Data.icon != null)
                {
                    modIcons[i].sprite = modRune.Data.icon;
                    modIcons[i].color = Color.white;
                }
                modIcons[i].enabled = true;

                if (i < modLevelTexts.Length && modLevelTexts[i])
                {
                    modLevelTexts[i].gameObject.SetActive(true);
                    modLevelTexts[i].text = $"{modRune.Level}";
                }
            }
            else
            {
                modIcons[i].enabled = false;
                if (i < modLevelTexts.Length && modLevelTexts[i]) modLevelTexts[i].gameObject.SetActive(false);
            }
        }

        SetupButton(levelUpManager);
    }

    // Affichage Slot Vide
    public void InitializeEmpty(int index, LevelUpUI levelUpManager)
    {
        _slotIndex = index;
        _levelUpManager = levelUpManager;

        if (emptyStateVisual) emptyStateVisual.SetActive(true);

        // On cache/assombrit tout
        formIcon.enabled = false;
        if (formLevelText) formLevelText.text = "";
        effectIcon.enabled = false;
        if (effectLevelText) effectLevelText.text = "";

        foreach (var img in modIcons) img.enabled = false;
        foreach (var txt in modLevelTexts) if (txt) txt.gameObject.SetActive(false);

        SetupButton(levelUpManager);
    }

    private void SetupButton(LevelUpUI manager)
    {
        clickButton.interactable = (manager != null);
        clickButton.onClick.RemoveAllListeners();
        if (manager != null)
        {
            clickButton.onClick.AddListener(() => manager.OnSlotClicked(_slotIndex));
        }
    }
}