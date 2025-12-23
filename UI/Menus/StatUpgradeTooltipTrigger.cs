using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach this to UI elements that should show stat upgrade tooltips on hover
/// </summary>
public class StatUpgradeTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Rune _statRune;
    private RectTransform _rectTransform;
    private Vector3 _cachedTooltipPosition;
    private bool _positionCached;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Set the stat rune data for this tooltip trigger
    /// </summary>
    public void SetStatRune(Rune statRune)
    {
        _statRune = statRune;

        // Invalidate position cache when stat changes
        _positionCached = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_statRune != null && RuneTooltip.Instance != null)
        {
            // Use cached position (GetWorldCorners allocates array every call)
            if (!_positionCached)
            {
                _cachedTooltipPosition = CalculateTooltipPosition();
                _positionCached = true;
            }

            RuneTooltip.Instance.ShowForStatUpgrade(_statRune, _cachedTooltipPosition);
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
