// EXAMPLE: Refactored PlayerHUD.cs with localization support
// This is a reference implementation showing how to migrate PlayerHUD to use the localization system
// Copy the relevant changes to your actual PlayerHUD.cs file

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SurvivorGame.Localization; // ADD THIS

public class PlayerHUD_LocalizedExample : MonoBehaviour
{
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private GameObject slotUIPrefab;

    [Header("Stat Icons")]
    [SerializeField] private Transform statsContainer;
    [SerializeField] private GameObject statIconPrefab;

    [Header("Health Bar")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Expérience")]
    [SerializeField] private Slider xpSlider;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Infos Combat")]
    [SerializeField] private TextMeshProUGUI enemyCountText;
    [SerializeField] private TextMeshProUGUI killCountText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Arcade Score System")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI multiplierText;
    [SerializeField] private Slider comboTimerSlider;

    private SpellManager _spellManager;
    private PlayerController _playerController;
    private PlayerStats _playerStats;
    private LevelManager _levelManager;
    private EnemyManager _enemyManager;
    private GameTimer _gameTimer;
    private ArcadeScoreSystem _scoreSystem;

    // Cache current values to avoid redundant updates
    private int _cachedEnemyCount = -1;
    private int _cachedKillCount = -1;
    private int _cachedScore = -1;
    private int _cachedCombo = -1;
    private int _cachedLevel = -1;

    private void Start()
    {
        // Setup Spells
        _spellManager = FindFirstObjectByType<SpellManager>();
        if (_spellManager != null)
        {
            _spellManager.OnInventoryUpdated += RefreshSpellsUI;
            RefreshSpellsUI();
        }

        // Setup Health
        _playerController = PlayerController.Instance;
        if (_playerController != null)
        {
            _playerController.OnHealthChanged += UpdateHealth;
            UpdateHealth(_playerController.CurrentHp, _playerController.MaxHp);
        }

        // Setup Stats Display
        _playerStats = PlayerStats.Instance;
        if (_playerStats != null)
        {
            _playerStats.OnStatsChanged += RefreshStatsUI;
            RefreshStatsUI();
        }

        _levelManager = LevelManager.Instance;
        if (_levelManager != null)
        {
            _levelManager.OnExperienceChanged.AddListener(UpdateXPBar);
            _levelManager.OnLevelUp.AddListener(UpdateLevelText);

            UpdateXPBar(0);
            UpdateLevelText();
        }

        _enemyManager = EnemyManager.Instance;
        if (_enemyManager != null)
        {
            _enemyManager.OnEnemyCountChanged += UpdateEnemyCount;
            _enemyManager.OnKillCountChanged += UpdateKillCount;
            UpdateEnemyCount(0);
            UpdateKillCount(0);
        }

        _gameTimer = GameTimer.Instance;
        if (_gameTimer != null)
        {
            _gameTimer.OnTimeChanged += UpdateTimer;
            UpdateTimer(0f);
        }

        _scoreSystem = ArcadeScoreSystem.Instance;
        if (_scoreSystem != null)
        {
            _scoreSystem.OnScoreChanged += UpdateScore;
            _scoreSystem.OnComboChanged += UpdateCombo;
            _scoreSystem.OnMultiplierChanged += UpdateMultiplier;
            _scoreSystem.OnComboTimerChanged += UpdateComboTimer;
            UpdateScore(0);
            UpdateCombo(0);
            UpdateMultiplier(1f);
            UpdateComboTimer(0f, _scoreSystem.ComboTimerMax);
        }

        // ADDED: Subscribe to language changes to update all text
        LocalizationManager.OnLanguageChanged += RefreshAllText;
    }

    // ADDED: Refresh all localized text when language changes
    private void RefreshAllText()
    {
        // Force refresh of all cached text
        if (_enemyManager != null)
            UpdateEnemyCount(_cachedEnemyCount);

        if (_cachedKillCount >= 0)
            UpdateKillCount(_cachedKillCount);

        if (_cachedScore >= 0)
            UpdateScore(_cachedScore);

        if (_cachedCombo >= 0)
            UpdateCombo(_cachedCombo);

        if (_cachedLevel >= 0 && _levelManager != null)
            UpdateLevelText();

        if (_playerController != null)
            UpdateHealth(_playerController.CurrentHp, _playerController.MaxHp);
    }

    private void RefreshSpellsUI()
    {
        // ⭐ CORRECTION : Détruire immédiatement pour éviter les accumulations
        List<GameObject> childrenToDestroy = new List<GameObject>();
        foreach (Transform child in slotsContainer)
        {
            childrenToDestroy.Add(child.gameObject);
        }
        foreach (var child in childrenToDestroy)
        {
            DestroyImmediate(child);
        }

        if (_spellManager == null) return;

        List<SpellSlot> slots = _spellManager.GetSlots();
        for (int i = 0; i < slots.Count; i++)
        {
            GameObject obj = Instantiate(slotUIPrefab, slotsContainer);
            if (obj.TryGetComponent<SpellSlotUI>(out var ui))
            {
                ui.Initialize(slots[i], i, null);
            }
        }
    }

    private void RefreshStatsUI()
    {
        // Clear existing stat icons
        if (statsContainer != null)
        {
            List<GameObject> childrenToDestroy = new List<GameObject>();
            foreach (Transform child in statsContainer)
            {
                childrenToDestroy.Add(child.gameObject);
            }
            foreach (var child in childrenToDestroy)
            {
                DestroyImmediate(child);
            }
        }

        if (_playerStats == null || statsContainer == null || statIconPrefab == null)
            return;

        // Get acquired stat types
        var acquiredStats = _playerStats.GetAcquiredStatTypes();

        // Display each acquired stat
        foreach (var statType in acquiredStats)
        {
            GameObject obj = Instantiate(statIconPrefab, statsContainer);
            if (obj.TryGetComponent<StatIconUI>(out var ui))
            {
                // Get the StatUpgradeSO for this stat type
                StatUpgradeSO statSO = null;
                if (StatUpgradeRegistry.Instance != null)
                {
                    statSO = StatUpgradeRegistry.Instance.GetStatUpgrade(statType);
                }

                ui.Initialize(statType, statSO);
            }
        }
    }

    // REFACTORED: Use LocalizationHelper for enemy count
    private void UpdateEnemyCount(int count)
    {
        _cachedEnemyCount = count; // Cache for language switching

        if (enemyCountText != null)
        {
            // OLD: enemyCountText.text = $"Ennemis : {count}";
            // NEW:
            enemyCountText.text = LocalizationHelper.FormatEnemyCount(count);

            // Optionnel : Changer la couleur si ça devient critique (+ de 300)
            if (count > 300) enemyCountText.color = Color.red;
            else enemyCountText.color = Color.white;
        }
    }

    // REFACTORED: Use LocalizationHelper for kill count
    private void UpdateKillCount(int count)
    {
        _cachedKillCount = count; // Cache for language switching

        if (killCountText != null)
        {
            // OLD: killCountText.text = $"Kills : {count}";
            // NEW:
            killCountText.text = LocalizationHelper.FormatKillCount(count);
        }
    }

    // Timer doesn't need localization (it's just numbers and colons)
    private void UpdateTimer(float elapsedTime)
    {
        if (timerText != null)
        {
            timerText.text = GameTimer.FormatTime(elapsedTime, hideHoursIfZero: true);
        }
    }

    // --- ARCADE SCORE SYSTEM ---

    // REFACTORED: Use LocalizationHelper for score
    private void UpdateScore(int score)
    {
        _cachedScore = score; // Cache for language switching

        if (scoreText != null)
        {
            // OLD: scoreText.text = $"Score : {score:N0}";
            // NEW:
            scoreText.text = LocalizationHelper.FormatScore(score);
        }
    }

    // REFACTORED: Use LocalizationHelper for combo
    private void UpdateCombo(int combo)
    {
        _cachedCombo = combo; // Cache for language switching

        if (comboText != null)
        {
            if (combo > 0)
            {
                // OLD: comboText.text = $"Combo x{combo}";
                // NEW:
                comboText.text = LocalizationHelper.FormatCombo(combo);
                comboText.gameObject.SetActive(true);
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }
    }

    // REFACTORED: Use LocalizationHelper for multiplier
    private void UpdateMultiplier(float multiplier)
    {
        if (multiplierText != null)
        {
            // OLD: multiplierText.text = $"x{multiplier:F1}";
            // NEW:
            multiplierText.text = LocalizationHelper.FormatMultiplier(multiplier);

            // Changer la couleur selon le multiplicateur
            if (multiplier >= 5f)
                multiplierText.color = new Color(1f, 0.3f, 0f); // Orange vif
            else if (multiplier >= 3f)
                multiplierText.color = new Color(1f, 0.7f, 0f); // Orange
            else if (multiplier >= 2f)
                multiplierText.color = new Color(1f, 1f, 0f); // Jaune
            else
                multiplierText.color = Color.white;
        }
    }

    private void UpdateComboTimer(float current, float max)
    {
        if (comboTimerSlider != null)
        {
            comboTimerSlider.maxValue = max;
            comboTimerSlider.value = current;

            // Masquer le slider si le combo est à 0
            if (current <= 0f)
            {
                comboTimerSlider.gameObject.SetActive(false);
            }
            else
            {
                comboTimerSlider.gameObject.SetActive(true);
            }
        }
    }

    // --- UI EXPERIENCE ---

    private void UpdateXPBar(float ratio)
    {
        if (xpSlider != null)
        {
            xpSlider.value = ratio;
        }
    }

    // REFACTORED: Use LocalizationHelper for level text
    private void UpdateLevelText()
    {
        if (levelText != null && _levelManager != null)
        {
            _cachedLevel = _levelManager.currentLevel; // Cache for language switching

            // OLD: levelText.text = $"LVL {_levelManager.currentLevel}";
            // NEW:
            levelText.text = LocalizationHelper.FormatLevel(_levelManager.currentLevel);
        }
    }

    // REFACTORED: Use LocalizationHelper for health display
    private void UpdateHealth(float current, float max)
    {
        if (healthSlider)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
        if (healthText)
        {
            // OLD: healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
            // NEW:
            healthText.text = LocalizationHelper.FormatHealth(
                Mathf.CeilToInt(current),
                Mathf.CeilToInt(max)
            );
        }
    }

    // ⭐ CORRECTION CRITIQUE : Se désabonner des événements
    private void OnDestroy()
    {
        if (_spellManager != null)
            _spellManager.OnInventoryUpdated -= RefreshSpellsUI;

        if (_playerController != null)
            _playerController.OnHealthChanged -= UpdateHealth;

        if (_playerStats != null)
            _playerStats.OnStatsChanged -= RefreshStatsUI;

        if (_levelManager != null)
        {
            _levelManager.OnExperienceChanged.RemoveListener(UpdateXPBar);
            _levelManager.OnLevelUp.RemoveListener(UpdateLevelText);
        }

        if (_enemyManager != null)
        {
            _enemyManager.OnEnemyCountChanged -= UpdateEnemyCount;
            _enemyManager.OnKillCountChanged -= UpdateKillCount;
        }

        if (_gameTimer != null)
        {
            _gameTimer.OnTimeChanged -= UpdateTimer;
        }

        if (_scoreSystem != null)
        {
            _scoreSystem.OnScoreChanged -= UpdateScore;
            _scoreSystem.OnComboChanged -= UpdateCombo;
            _scoreSystem.OnMultiplierChanged -= UpdateMultiplier;
            _scoreSystem.OnComboTimerChanged -= UpdateComboTimer;
        }

        // ADDED: Unsubscribe from language changes
        LocalizationManager.OnLanguageChanged -= RefreshAllText;
    }
}

/*
 * MIGRATION SUMMARY FOR PlayerHUD.cs:
 *
 * 1. Add using: using SurvivorGame.Localization;
 *
 * 2. Add caching fields for values that need to refresh on language change:
 *    - private int _cachedEnemyCount = -1;
 *    - private int _cachedKillCount = -1;
 *    - private int _cachedScore = -1;
 *    - private int _cachedCombo = -1;
 *    - private int _cachedLevel = -1;
 *
 * 3. In Start(), subscribe to language changes:
 *    LocalizationManager.OnLanguageChanged += RefreshAllText;
 *
 * 4. Add RefreshAllText() method to handle language switching
 *
 * 5. Replace all hardcoded strings with LocalizationHelper calls:
 *    - "Ennemis : {count}" → LocalizationHelper.FormatEnemyCount(count)
 *    - "Kills : {count}" → LocalizationHelper.FormatKillCount(count)
 *    - "Score : {score:N0}" → LocalizationHelper.FormatScore(score)
 *    - "Combo x{combo}" → LocalizationHelper.FormatCombo(combo)
 *    - "x{multiplier:F1}" → LocalizationHelper.FormatMultiplier(multiplier)
 *    - "LVL {level}" → LocalizationHelper.FormatLevel(level)
 *    - "{current} / {max}" → LocalizationHelper.FormatHealth(current, max)
 *
 * 6. In OnDestroy(), unsubscribe:
 *    LocalizationManager.OnLanguageChanged -= RefreshAllText;
 *
 * That's it! The UI will now automatically update when language changes.
 */
