using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct XpTier
{
    public string name;
    public int maxLevel;
    public int growthAmount;
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("État")]
    public int currentLevel = 1;
    public int currentExperience = 0;
    public int experienceToNextLevel = 100;

    // NOUVEAU : Stockage des niveaux en attente
    private int _pendingLevelUps = 0;

    [Header("Meta-Progression")]
    public int availableRerolls = 2; // Stock de départ
    public int availableBans = 1;    // Stock de départ

    // Liste des runes bannies pour cette partie
    private List<string> _bannedRunes = new List<string>();

    [Header("Configuration")]
    [SerializeField] private List<XpTier> growthTiers = new List<XpTier>();
    [SerializeField] private int defaultGrowth = 100;

    public UnityEvent OnLevelUp;
    public UnityEvent<float> OnExperienceChanged;

    private void Awake()
    {
        Instance = this;
    }

    public void AddExperience(int amount)
    {
        // Applique le multiplicateur
        float mult = PlayerStats.Instance != null ? PlayerStats.Instance.ExperienceMultiplier : 1f;
        int finalAmount = Mathf.RoundToInt(amount * mult);

        currentExperience += finalAmount;
        CheckLevelUp();
        UpdateInterface();
    }

    private void CheckLevelUp()
    {
        while (currentExperience >= experienceToNextLevel)
        {
            currentExperience -= experienceToNextLevel;
            currentLevel++;

            int growth = GetGrowthForLevel(currentLevel);
            experienceToNextLevel += growth;

            _pendingLevelUps++; // On empile les niveaux
        }

        // Si l'UI n'est pas déjà ouverte, on déclenche le premier
        if (_pendingLevelUps > 0 && Time.timeScale > 0)
        {
            TriggerNextLevelUp();
        }
    }

    public void TriggerNextLevelUp()
    {
        if (_pendingLevelUps > 0)
        {
            _pendingLevelUps--;
            Debug.Log($"LEVEL UP! Reste à traiter : {_pendingLevelUps}");
            OnLevelUp?.Invoke();
        }
    }

    // Gestion des Bans
    public void BanRune(string runeName)
    {
        if (!_bannedRunes.Contains(runeName) && availableBans > 0)
        {
            _bannedRunes.Add(runeName);
            availableBans--;
        }
        // Après un ban, on considère le niveau comme "passé" (ou on reroll, selon le design).
        // Ici, tu as demandé : "cela passe le niveau sans choisir".
        // Donc on relance la boucle.

        // On ferme l'UI actuelle via l'événement (sera géré par LevelUpUI)
    }

    public bool IsRuneBanned(string runeName) => _bannedRunes.Contains(runeName);

    public bool ConsumeReroll()
    {
        if (availableRerolls > 0)
        {
            availableRerolls--;
            return true;
        }
        return false;
    }

    // ... (GetGrowthForLevel et UpdateInterface inchangés) ...
    private int GetGrowthForLevel(int level)
    {
        foreach (var tier in growthTiers)
        {
            if (level <= tier.maxLevel) return tier.growthAmount;
        }
        return defaultGrowth;
    }

    private void UpdateInterface()
    {
        if (experienceToNextLevel == 0) experienceToNextLevel = 100;
        float ratio = (float)currentExperience / experienceToNextLevel;
        OnExperienceChanged?.Invoke(ratio);
    }
}