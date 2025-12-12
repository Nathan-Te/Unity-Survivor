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

public class LevelManager : Singleton<LevelManager>
{
    [Header("�tat")]
    public int currentLevel = 1;
    public int currentExperience = 0;
    public int experienceToNextLevel = 100;

    // NOUVEAU : Stockage des niveaux en attente
    private int _pendingLevelUps = 0;

    [Header("Meta-Progression")]
    public int availableRerolls = 2; // Stock de d�part
    public int availableBans = 1;    // Stock de d�part

    // Liste des runes bannies pour cette partie
    private List<string> _bannedRunes = new List<string>();

    [Header("Configuration")]
    [SerializeField] private List<XpTier> growthTiers = new List<XpTier>();
    [SerializeField] private int defaultGrowth = 100;

    public UnityEvent OnLevelUp;
    public UnityEvent<float> OnExperienceChanged;

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

        // Si l'UI n'est pas d�j� ouverte, on d�clenche le premier
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
        // Apr�s un ban, on consid�re le niveau comme "pass�" (ou on reroll, selon le design).
        // Ici, tu as demand� : "cela passe le niveau sans choisir".
        // Donc on relance la boucle.

        // On ferme l'UI actuelle via l'�v�nement (sera g�r� par LevelUpUI)
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

    // ... (GetGrowthForLevel et UpdateInterface inchang�s) ...
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