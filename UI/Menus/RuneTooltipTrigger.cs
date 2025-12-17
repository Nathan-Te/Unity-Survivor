using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach this to UI elements that should show rune tooltips on hover
/// </summary>
public class RuneTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Rune _rune;
    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Set the rune data for this tooltip trigger
    /// </summary>
    public void SetRune(Rune rune)
    {
        _rune = rune;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_rune != null && RuneTooltip.Instance != null)
        {
            // Calculate position for the tooltip (offset to the right of the element)
            Vector3 tooltipPosition = CalculateTooltipPosition();
            RuneTooltip.Instance.Show(_rune, tooltipPosition);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (RuneTooltip.Instance != null)
        {
            RuneTooltip.Instance.Hide();
        }
    }

    private Vector3 CalculateTooltipPosition()
    {
        // Get the corners of the UI element
        Vector3[] corners = new Vector3[4];
        _rectTransform.GetWorldCorners(corners);

        // Position tooltip to the right of the element
        // corners[2] is top-right corner
        Vector3 position = corners[2];
        position.x += 10f; // Small offset to the right

        return position;
    }

    private void OnDisable()
    {
        // Hide tooltip when this element is disabled
        if (RuneTooltip.Instance != null)
        {
            RuneTooltip.Instance.Hide();
        }
    }
}
