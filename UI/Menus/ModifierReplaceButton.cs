using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component for modifier replacement buttons, displays icon and level
/// </summary>
public class ModifierReplaceButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button button;

    public Button Button => button;

    /// <summary>
    /// Initializes the button with modifier data
    /// </summary>
    public void Initialize(Rune modifierRune)
    {
        if (modifierRune == null || modifierRune.Data == null)
        {
            Debug.LogWarning("ModifierReplaceButton: Tried to initialize with null modifier");
            return;
        }

        // Set icon
        if (iconImage != null && modifierRune.Data.icon != null)
        {
            iconImage.sprite = modifierRune.Data.icon;
            iconImage.enabled = true;
        }
        else if (iconImage != null)
        {
            iconImage.enabled = false;
        }

        // Set level text
        if (levelText != null)
        {
            levelText.text = $"Lvl {modifierRune.Level}";
        }
    }
}
