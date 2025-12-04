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

    private void Start()
    {
        // Setup Spells
        SpellManager sm = FindFirstObjectByType<SpellManager>();
        if (sm != null)
        {
            sm.OnInventoryUpdated += RefreshUI;
            RefreshUI();
        }

        // Setup Health
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.OnHealthChanged += UpdateHealth;
            UpdateHealth(PlayerController.Instance.CurrentHp, PlayerController.Instance.MaxHp);
            // Force update initiale (si le joueur est déjà init)
            // Note: Idéalement, PlayerController devrait exposer CurrentHp/MaxHp publiquement pour lire l'état initial
        }

        if (LevelManager.Instance != null)
        {
            // On s'abonne aux événements du LevelManager
            LevelManager.Instance.OnExperienceChanged.AddListener(UpdateXPBar);
            LevelManager.Instance.OnLevelUp.AddListener(UpdateLevelText);

            // Initialisation de l'affichage (au cas où on commence niveau 1 avec 0 XP)
            UpdateXPBar(0);
            UpdateLevelText();
        }
    }

    private void RefreshUI()
    {
        // Nettoyage
        foreach (Transform child in slotsContainer) Destroy(child.gameObject);

        SpellManager sm = FindFirstObjectByType<SpellManager>();
        if (sm == null) return;

        List<SpellSlot> slots = sm.GetSlots();
        for (int i = 0; i < slots.Count; i++)
        {
            GameObject obj = Instantiate(slotUIPrefab, slotsContainer);
            if (obj.TryGetComponent<SpellSlotUI>(out var ui))
            {
                // Pas de manager passé ici, donc non-cliquable (juste affichage)
                ui.Initialize(slots[i], i, null);
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
        if (levelText != null && LevelManager.Instance != null)
        {
            levelText.text = $"LVL {LevelManager.Instance.currentLevel}";
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
}