using UnityEngine;
using TMPro;

namespace SurvivorGame.Localization
{
    /// <summary>
    /// Simple component that auto-updates TextMeshPro with localized text.
    /// Just set the key in the Inspector!
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class SimpleLocalizedText : MonoBehaviour
    {
        [Header("Localization")]
        [Tooltip("Key from the JSON localization file (e.g., 'LEVELUP_TITLE')")]
        [SerializeField] private string _key;

        [Header("Formatting (Optional)")]
        [Tooltip("If true, you can provide format arguments via code using SetFormatArgs()")]
        [SerializeField] private bool _useFormatting = false;

        private TextMeshProUGUI _textComponent;
        private object[] _formatArgs;

        private void Awake()
        {
            _textComponent = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            SimpleLocalizationManager.OnLanguageChanged += UpdateText;
            UpdateText();
        }

        private void OnDisable()
        {
            SimpleLocalizationManager.OnLanguageChanged -= UpdateText;
        }

        /// <summary>
        /// Updates the text from localization.
        /// </summary>
        public void UpdateText()
        {
            if (_textComponent == null || SimpleLocalizationManager.Instance == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(_key))
            {
                Debug.LogWarning($"SimpleLocalizedText on {gameObject.name} has no key set.", this);
                return;
            }

            if (_useFormatting && _formatArgs != null && _formatArgs.Length > 0)
            {
                _textComponent.text = SimpleLocalizationManager.Instance.GetFormattedString(_key, _formatArgs);
            }
            else
            {
                _textComponent.text = SimpleLocalizationManager.Instance.GetString(_key, _key);
            }
        }

        /// <summary>
        /// Sets the localization key and updates the text.
        /// </summary>
        public void SetKey(string key)
        {
            _key = key;
            UpdateText();
        }

        /// <summary>
        /// Sets format arguments for string formatting and updates the text.
        /// Example: SetFormatArgs(5) for "LVL {0}" -> "LVL 5"
        /// </summary>
        public void SetFormatArgs(params object[] args)
        {
            _formatArgs = args;
            _useFormatting = true;
            UpdateText();
        }

        /// <summary>
        /// Sets both key and format arguments, then updates.
        /// </summary>
        public void SetKeyAndFormat(string key, params object[] args)
        {
            _key = key;
            _formatArgs = args;
            _useFormatting = true;
            UpdateText();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_textComponent == null)
            {
                _textComponent = GetComponent<TextMeshProUGUI>();
            }

            // Update text in editor if playing
            if (Application.isPlaying && enabled)
            {
                UpdateText();
            }
        }
#endif
    }
}
