using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpellSlotUI : MonoBehaviour
{
    [Header("Composants UI")]
    [SerializeField] private Image formIcon;
    [SerializeField] private TextMeshProUGUI formLevelText;
    [SerializeField] private Image effectIcon;
    [SerializeField] private TextMeshProUGUI effectLevelText; // Assure-toi que c'est li�
    [SerializeField] private Image[] modIcons;
    [SerializeField] private TextMeshProUGUI[] modLevelTexts; // Assure-toi que c'est li�

    [SerializeField] private Button clickButton;
    [SerializeField] private GameObject emptyStateVisual; // (Optionnel) un texte ou image "Empty"

    private int _slotIndex;
    private LevelUpUI _levelUpManager;
    private UpgradeData _pendingUpgrade;

    // Tooltip triggers
    private RuneTooltipTrigger _formTooltipTrigger;
    private RuneTooltipTrigger _effectTooltipTrigger;
    private RuneTooltipTrigger[] _modTooltipTriggers;

    // Affichage Standard
    public void Initialize(SpellSlot slot, int index, LevelUpUI levelUpManager = null, UpgradeData pendingUpgrade = null)
    {
        _slotIndex = index;
        _levelUpManager = levelUpManager;
        _pendingUpgrade = pendingUpgrade;

        if (emptyStateVisual) emptyStateVisual.SetActive(false); // On cache l'�tat vide

        // Setup tooltip triggers if not already done
        SetupTooltipTriggers();

        // 1. Forme
        if (slot.formRune != null && slot.formRune.Data.icon != null)
        {
            formIcon.sprite = slot.formRune.Data.icon;
            formIcon.enabled = true;
            formIcon.color = Color.white;
            if (formLevelText) formLevelText.text = $"Lvl {slot.formRune.Level}";

            // Set tooltip data (pass slot for full context)
            if (_formTooltipTrigger != null)
                _formTooltipTrigger.SetRune(slot.formRune, slot);
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

            // Set tooltip data (pass slot for full context)
            if (_effectTooltipTrigger != null)
                _effectTooltipTrigger.SetRune(slot.effectRune, slot);
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

                // Set tooltip data (pass slot for full context)
                if (_modTooltipTriggers != null && i < _modTooltipTriggers.Length && _modTooltipTriggers[i] != null)
                    _modTooltipTriggers[i].SetRune(modRune, slot);
            }
            else
            {
                modIcons[i].enabled = false;
                if (i < modLevelTexts.Length && modLevelTexts[i]) modLevelTexts[i].gameObject.SetActive(false);

                // Clear tooltip data
                if (_modTooltipTriggers != null && i < _modTooltipTriggers.Length && _modTooltipTriggers[i] != null)
                    _modTooltipTriggers[i].SetRune(null);
            }
        }

        SetupButton(levelUpManager);
    }

    // Affichage Slot Vide
    public void InitializeEmpty(int index, LevelUpUI levelUpManager, UpgradeData pendingUpgrade = null)
    {
        _slotIndex = index;
        _levelUpManager = levelUpManager;
        _pendingUpgrade = pendingUpgrade;

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
            clickButton.onClick.AddListener(() => manager.OnSlotClicked(_slotIndex, _pendingUpgrade));
        }
    }

    private void SetupTooltipTriggers()
    {
        // Setup form tooltip
        if (_formTooltipTrigger == null && formIcon != null)
        {
            _formTooltipTrigger = formIcon.gameObject.GetComponent<RuneTooltipTrigger>();
            if (_formTooltipTrigger == null)
                _formTooltipTrigger = formIcon.gameObject.AddComponent<RuneTooltipTrigger>();
        }

        // Setup effect tooltip
        if (_effectTooltipTrigger == null && effectIcon != null)
        {
            _effectTooltipTrigger = effectIcon.gameObject.GetComponent<RuneTooltipTrigger>();
            if (_effectTooltipTrigger == null)
                _effectTooltipTrigger = effectIcon.gameObject.AddComponent<RuneTooltipTrigger>();
        }

        // Setup modifier tooltips
        if (_modTooltipTriggers == null || _modTooltipTriggers.Length != modIcons.Length)
        {
            _modTooltipTriggers = new RuneTooltipTrigger[modIcons.Length];
        }

        for (int i = 0; i < modIcons.Length; i++)
        {
            if (_modTooltipTriggers[i] == null && modIcons[i] != null)
            {
                _modTooltipTriggers[i] = modIcons[i].gameObject.GetComponent<RuneTooltipTrigger>();
                if (_modTooltipTriggers[i] == null)
                    _modTooltipTriggers[i] = modIcons[i].gameObject.AddComponent<RuneTooltipTrigger>();
            }
        }
    }
}