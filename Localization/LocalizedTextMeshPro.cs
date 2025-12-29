using UnityEngine;
using TMPro;

namespace SurvivorGame.Localization
{
    /// <summary>
    /// Component that automatically updates a TextMeshProUGUI component with localized text.
    /// Listens to language changes and updates text accordingly.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizedTextMeshPro : MonoBehaviour
    {
        [Header("Localization Settings")]
        [Tooltip("Name of the localization table to use")]
        [SerializeField] private string _tableName = "UI";

        [Tooltip("Key for the localized string in the table")]
        [SerializeField] private string _key;

        [Header("Formatting (Optional)")]
        [Tooltip("If true, uses string.Format with dynamic arguments set via code")]
        [SerializeField] private bool _useFormatting = false;

        private TextMeshProUGUI _textComponent;
        private object[] _formatArgs;

        private void Awake()
        {
            _textComponent = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            LocalizationManager.OnLanguageChanged += UpdateText;
            UpdateText();
        }

        private void OnDisable()
        {
            LocalizationManager.OnLanguageChanged -= UpdateText;
        }

        /// <summary>
        /// Updates the text from the localization table.
        /// </summary>
        public void UpdateText()
        {
            if (_textComponent == null || LocalizationManager.Instance == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(_key))
            {
                Debug.LogWarning($"LocalizedTextMeshPro on {gameObject.name} has no key set.", this);
                return;
            }

            if (_useFormatting && _formatArgs != null && _formatArgs.Length > 0)
            {
                _textComponent.text = LocalizationManager.Instance.GetFormattedString(_tableName, _key, _formatArgs);
            }
            else
            {
                _textComponent.text = LocalizationManager.Instance.GetString(_tableName, _key, _key);
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
        /// Sets the localization key with a specific table and updates the text.
        /// </summary>
        public void SetKey(string tableName, string key)
        {
            _tableName = tableName;
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
            UpdateText();
        }

        /// <summary>
        /// Sets both key and format arguments, then updates.
        /// </summary>
        public void SetKeyAndFormat(string key, params object[] args)
        {
            _key = key;
            _formatArgs = args;
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
