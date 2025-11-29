using UnityEngine;
using UnityEngine.UI;
using TMPro; // Important

public class SpellSlotUI : MonoBehaviour
{
    [Header("Composants UI")]
    [SerializeField] private Image bgImage;
    [SerializeField] private Image formIcon;
    [SerializeField] private Image effectIcon;
    [SerializeField] private Image[] modIcons; // Assure-toi d'avoir lié ces images dans le prefab

    [SerializeField] private Button clickButton;

    private int _slotIndex;
    private LevelUpUI _levelUpManager;

    public void Initialize(SpellSlot slot, int index, LevelUpUI levelUpManager = null)
    {
        _slotIndex = index;
        _levelUpManager = levelUpManager;

        // 1. Forme
        // On accède à la donnée via la Rune (slot.formRune.Data)
        if (slot.formRune != null && slot.formRune.Data.icon != null)
        {
            formIcon.sprite = slot.formRune.Data.icon;
            formIcon.enabled = true;
        }
        else formIcon.enabled = false;

        // 2. Effet (Couleur)
        if (slot.effectRune != null)
        {
            // Si l'effet a une icône, on l'affiche, sinon on garde la couleur
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

        // 3. Modificateurs
        for (int i = 0; i < modIcons.Length; i++)
        {
            // CORRECTION : On vérifie aussi que Data n'est pas null !
            if (i < slot.modifierRunes.Length &&
                slot.modifierRunes[i] != null &&
                slot.modifierRunes[i].Data != null)
            {
                Rune modRune = slot.modifierRunes[i];

                if (modRune.Data.icon != null)
                {
                    modIcons[i].sprite = modRune.Data.icon;
                    modIcons[i].color = Color.white;
                    modIcons[i].enabled = true;
                }
                else
                {
                    // Fallback si pas d'icône (carré blanc semi-transparent)
                    modIcons[i].sprite = null;
                    modIcons[i].color = new Color(1, 1, 1, 0.5f);
                    modIcons[i].enabled = true;
                }
            }
            else
            {
                modIcons[i].enabled = false; // Slot vide ou Data manquant
            }
        }

        // Gestion du Clic (Targeting Mode)
        clickButton.interactable = (_levelUpManager != null);
        clickButton.onClick.RemoveAllListeners();
        if (_levelUpManager != null)
        {
            clickButton.onClick.AddListener(() => _levelUpManager.OnSlotClicked(_slotIndex));
        }
    }
}