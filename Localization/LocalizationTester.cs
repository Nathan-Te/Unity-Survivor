using UnityEngine;
using TMPro;

namespace SurvivorGame.Localization
{
    /// <summary>
    /// Simple test script to verify localization system is working.
    ///
    /// Usage:
    /// 1. Create a Canvas with TextMeshPro text element
    /// 2. Add this component to the Canvas
    /// 3. Assign the text element
    /// 4. Enter Play Mode
    /// 5. Press 'E' for English, 'F' for French
    ///
    /// You should see the text update automatically when switching languages.
    /// </summary>
    public class LocalizationTester : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI testText;

        [Header("Test Settings")]
        [SerializeField] private bool showInstructions = true;
        [SerializeField] private KeyCode englishKey = KeyCode.E;
        [SerializeField] private KeyCode frenchKey = KeyCode.F;

        private void Start()
        {
            if (testText == null)
            {
                Debug.LogError("Test Text not assigned! Assign a TextMeshProUGUI component in the inspector.");
                enabled = false;
                return;
            }

            // Test that LocalizationManager exists
            if (LocalizationManager.Instance == null)
            {
                Debug.LogError("LocalizationManager not found in scene! Create a GameObject with LocalizationManager component.");
                testText.text = "ERROR: LocalizationManager not found!";
                testText.color = Color.red;
                enabled = false;
                return;
            }

            // Subscribe to language changes
            LocalizationManager.OnLanguageChanged += UpdateDisplay;

            // Initial display
            UpdateDisplay();

            Debug.Log("Localization Tester initialized. Press E for English, F for French.");
        }

        private void Update()
        {
            if (Input.GetKeyDown(englishKey))
            {
                LocalizationManager.Instance.SetLanguage(Language.English);
                Debug.Log("Language set to English");
            }

            if (Input.GetKeyDown(frenchKey))
            {
                LocalizationManager.Instance.SetLanguage(Language.French);
                Debug.Log("Language set to French");
            }
        }

        private void UpdateDisplay()
        {
            if (testText == null || LocalizationManager.Instance == null)
                return;

            Language currentLanguage = LocalizationManager.Instance.CurrentLanguage;

            // Build test display
            string display = "";

            if (showInstructions)
            {
                display += $"<size=24><b>Localization System Test</b></size>\n\n";
                display += $"Current Language: <color=yellow>{currentLanguage}</color>\n";
                display += $"Press [{englishKey}] for English | Press [{frenchKey}] for Français\n\n";
                display += "--- Test Strings ---\n\n";
            }

            // Test UI strings
            display += TestUIStrings();
            display += "\n";

            // Test Stats strings
            display += TestStatsStrings();
            display += "\n";

            // Test Combat strings
            display += TestCombatStrings();
            display += "\n";

            // Test Enums
            display += TestEnumLocalization();
            display += "\n";

            // Test Helpers
            display += TestHelpers();

            testText.text = display;
            testText.color = Color.white;
        }

        private string TestUIStrings()
        {
            string result = "<b>UI Strings:</b>\n";

            result += $"  Level Up: {LocalizationHelper.GetUIString(LocalizationKeys.UI_LEVELUP_TITLE)}\n";
            result += $"  Special: {LocalizationHelper.GetUIString(LocalizationKeys.UI_LEVELUP_SPECIAL)}\n";
            result += $"  Enemies: {LocalizationHelper.FormatEnemyCount(42)}\n";
            result += $"  Kills: {LocalizationHelper.FormatKillCount(100)}\n";
            result += $"  Level: {LocalizationHelper.FormatLevel(5)}\n";

            return result;
        }

        private string TestStatsStrings()
        {
            string result = "<b>Stat Names:</b>\n";

            result += $"  {LocalizationHelper.GetStatsString(LocalizationKeys.STAT_MOVE_SPEED)}\n";
            result += $"  {LocalizationHelper.GetStatsString(LocalizationKeys.STAT_MAX_HEALTH)}\n";
            result += $"  {LocalizationHelper.GetStatsString(LocalizationKeys.STAT_GLOBAL_DAMAGE)}\n";

            return result;
        }

        private string TestCombatStrings()
        {
            string result = "<b>Combat Labels:</b>\n";

            result += $"  {LocalizationHelper.GetDamageLabel()}\n";
            result += $"  {LocalizationHelper.GetCooldownLabel()}\n";
            result += $"  {LocalizationHelper.FormatBurn(10f, 3f)}\n";

            return result;
        }

        private string TestEnumLocalization()
        {
            string result = "<b>Enum Localization:</b>\n";

            result += $"  Stat: {EnumLocalizer.GetStatName(StatType.MoveSpeed)}\n";
            result += $"  Element: {EnumLocalizer.GetElementName(ElementType.Fire)}\n";
            result += $"  Rarity: {EnumLocalizer.GetRarityName(Rarity.Legendary)}\n";

            return result;
        }

        private string TestHelpers()
        {
            string result = "<b>Helper Functions:</b>\n";

            result += $"  Score: {LocalizationHelper.FormatScore(12345)}\n";
            result += $"  Combo: {LocalizationHelper.FormatCombo(10)}\n";
            result += $"  Health: {LocalizationHelper.FormatHealth(80, 100)}\n";

            return result;
        }

        private void OnDestroy()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.OnLanguageChanged -= UpdateDisplay;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (testText == null)
            {
                testText = GetComponentInChildren<TextMeshProUGUI>();
            }
        }
#endif
    }
}
