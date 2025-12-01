using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpellSlotUI : MonoBehaviour
{
    [SerializeField] private Image formIcon;
    [SerializeField] private Image effectIcon;
    [SerializeField] private Image[] modIcons;
    [SerializeField] private Button clickButton;
    [SerializeField] private TextMeshProUGUI formLevelText; // Assure-toi de l'avoir lié

    private int _slotIndex;
    private LevelUpUI _levelUpManager;

    public void Initialize(SpellSlot slot, int index, LevelUpUI levelUpManager = null)
    {
        _slotIndex = index;
        _levelUpManager = levelUpManager;

        if (slot.formRune != null && slot.formRune.Data.icon != null)
        {
            formIcon.sprite = slot.formRune.Data.icon;
            formIcon.enabled = true;
            // AFFICHE LE NIVEAU
            formLevelText.text = $"Lvl {slot.formRune.Level}";
        }
        else formIcon.enabled = false;

        if (slot.effectRune != null)
        {
            if (slot.effectRune.Data.icon != null)
            {
                effectIcon.sprite = slot.effectRune.Data.icon;
                effectIcon.color = Color.white;
            }
            else
            {
                effectIcon.color = slot.effectRune.AsEffect.tintColor;
            }
        }

        for (int i = 0; i < modIcons.Length; i++)
        {
            if (i < slot.modifierRunes.Length && slot.modifierRunes[i] != null && slot.modifierRunes[i].Data != null)
            {
                if (slot.modifierRunes[i].Data.icon != null)
                {
                    modIcons[i].sprite = slot.modifierRunes[i].Data.icon;
                    modIcons[i].color = Color.white;
                    modIcons[i].enabled = true;
                }
                else
                {
                    modIcons[i].sprite = null;
                    modIcons[i].color = new Color(1, 1, 1, 0.5f);
                    modIcons[i].enabled = true;
                }
            }
            else modIcons[i].enabled = false;
        }

        clickButton.interactable = (_levelUpManager != null);
        clickButton.onClick.RemoveAllListeners();
        if (_levelUpManager != null)
            clickButton.onClick.AddListener(() => _levelUpManager.OnSlotClicked(_slotIndex));
    }
}