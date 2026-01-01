using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SurvivorGame.Progression;
using SurvivorGame.Localization;
using System;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Individual level selection button component.
    /// Displays level info and handles locked/unlocked states.
    /// </summary>
    public class LevelSelectButton : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI difficultyText;
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject lockedOverlay;

        private LevelDefinition _levelData;
        private Action<LevelDefinition> _onSelected;

        private void Awake()
        {
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClicked);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClicked);
            }
        }

        /// <summary>
        /// Initializes the button with level data
        /// </summary>
        public void Initialize(LevelDefinition levelData, Action<LevelDefinition> onSelected)
        {
            _levelData = levelData;
            _onSelected = onSelected;

            Refresh();
        }

        /// <summary>
        /// Refreshes the button display based on current progression
        /// </summary>
        public void Refresh()
        {
            if (_levelData == null) return;

            // Update name
            if (nameText != null)
            {
                nameText.text = SimpleLocalizationHelper.Get(_levelData.nameKey, _levelData.levelId);
            }

            // Update difficulty
            if (difficultyText != null)
            {
                string stars = new string('★', _levelData.difficulty);
                difficultyText.text = stars;
            }

            // Update icon
            if (iconImage != null && _levelData.icon != null)
            {
                iconImage.sprite = _levelData.icon;
            }

            // Check if unlocked
            var progression = ProgressionManager.Instance?.CurrentProgression;
            bool isUnlocked = progression != null && progression.IsLevelUnlocked(_levelData.levelId);

            // Update locked state
            if (lockedOverlay != null)
            {
                lockedOverlay.SetActive(!isUnlocked);
            }

            // Enable/disable button
            if (button != null)
            {
                button.interactable = isUnlocked;
            }
        }

        private void OnButtonClicked()
        {
            _onSelected?.Invoke(_levelData);
        }
    }
}
