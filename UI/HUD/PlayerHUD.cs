using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private GameObject slotUIPrefab;

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
    private LevelManager _levelManager;
    private EnemyManager _enemyManager;
    private GameTimer _gameTimer;
    private ArcadeScoreSystem _scoreSystem;

    private void Start()
    {
        // Setup Spells
        _spellManager = FindFirstObjectByType<SpellManager>();
        if (_spellManager != null)
        {
            _spellManager.OnInventoryUpdated += RefreshUI;
            RefreshUI();
        }

        // Setup Health
        _playerController = PlayerController.Instance;
        if (_playerController != null)
        {
            _playerController.OnHealthChanged += UpdateHealth;
            UpdateHealth(_playerController.CurrentHp, _playerController.MaxHp);
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
    }

    private void RefreshUI()
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

    private void UpdateEnemyCount(int count)
    {
        if (enemyCountText != null)
        {
            enemyCountText.text = $"Ennemis : {count}";

            // Optionnel : Changer la couleur si ça devient critique (+ de 300)
            if (count > 300) enemyCountText.color = Color.red;
            else enemyCountText.color = Color.white;
        }
    }

    private void UpdateKillCount(int count)
    {
        if (killCountText != null)
        {
            killCountText.text = $"Kills : {count}";
        }
    }

    private void UpdateTimer(float elapsedTime)
    {
        if (timerText != null)
        {
            timerText.text = GameTimer.FormatTime(elapsedTime, hideHoursIfZero: true);
        }
    }

    // --- ARCADE SCORE SYSTEM ---

    private void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score : {score:N0}";
        }
    }

    private void UpdateCombo(int combo)
    {
        if (comboText != null)
        {
            if (combo > 0)
            {
                comboText.text = $"Combo x{combo}";
                comboText.gameObject.SetActive(true);
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateMultiplier(float multiplier)
    {
        if (multiplierText != null)
        {
            multiplierText.text = $"x{multiplier:F1}";

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

    private void UpdateLevelText()
    {
        if (levelText != null && _levelManager != null)
        {
            levelText.text = $"LVL {_levelManager.currentLevel}";
        }
    }

    private void UpdateHealth(float current, float max)
    {
        if (healthSlider)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
        if (healthText)
        {
            healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }
    }

    // ⭐ CORRECTION CRITIQUE : Se désabonner des événements
    private void OnDestroy()
    {
        if (_spellManager != null)
            _spellManager.OnInventoryUpdated -= RefreshUI;

        if (_playerController != null)
            _playerController.OnHealthChanged -= UpdateHealth;

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
    }
}