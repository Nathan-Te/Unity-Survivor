using UnityEngine;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Simple rotation animation for loading spinner.
    /// Uses unscaled time to work even when the game is paused.
    /// </summary>
    public class LoadingSpinner : MonoBehaviour
    {
        [Header("Rotation Settings")]
        [Tooltip("Rotation speed in degrees per second")]
        [SerializeField] private float rotationSpeed = 180f;

        [Tooltip("Rotation axis (default: Z-axis for 2D UI)")]
        [SerializeField] private Vector3 rotationAxis = new Vector3(0, 0, -1);

        [Header("Optional Pulsing")]
        [Tooltip("Enable scale pulsing effect")]
        [SerializeField] private bool enablePulsing = false;

        [Tooltip("Pulse speed")]
        [SerializeField] private float pulseSpeed = 2f;

        [Tooltip("Pulse scale range")]
        [SerializeField] private Vector2 pulseRange = new Vector2(0.9f, 1.1f);

        private Vector3 _originalScale;

        private void Awake()
        {
            _originalScale = transform.localScale;
        }

        private void Update()
        {
            // Rotate (using unscaled time to work during pause)
            transform.Rotate(rotationAxis, rotationSpeed * Time.unscaledDeltaTime);

            // Optional pulsing effect
            if (enablePulsing)
            {
                float pulse = Mathf.Lerp(pulseRange.x, pulseRange.y,
                    (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f);
                transform.localScale = _originalScale * pulse;
            }
        }

        private void OnDisable()
        {
            // Reset scale when disabled
            if (enablePulsing)
            {
                transform.localScale = _originalScale;
            }
        }
    }
}
