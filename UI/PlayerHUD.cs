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

    private SpellManager _spellManager;
    private PlayerController _playerController;
    private LevelManager _levelManager;
    private EnemyManager _enemyManager;

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
            UpdateEnemyCount(0);
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
            _enemyManager.OnEnemyCountChanged -= UpdateEnemyCount;
    }
}