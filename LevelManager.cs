using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct XpTier
{
    public string name;      // Juste pour t'y retrouver dans l'inspecteur (ex: "Débutant")
    public int maxLevel;     // Jusqu'à quel niveau ce palier s'applique (ex: 10)
    public int growthAmount; // Combien d'XP on ajoute à chaque niveau (ex: 10)
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("État")]
    public int currentLevel = 1;
    public int currentExperience = 0;
    public int experienceToNextLevel = 100; // Valeur de départ (Niveau 1->2)

    [Header("Configuration des Paliers")]
    [SerializeField] private List<XpTier> growthTiers = new List<XpTier>();
    [SerializeField] private int defaultGrowth = 100; // Fallback si on dépasse le dernier palier

    // Événements
    public UnityEvent OnLevelUp;
    public UnityEvent<float> OnExperienceChanged;

    private void Awake()
    {
        Instance = this;
    }

    public void AddExperience(int amount)
    {
        currentExperience += amount;
        CheckLevelUp();
        UpdateInterface();
    }

    private void CheckLevelUp()
    {
        // On utilise une boucle while au cas où on gagne assez d'XP pour passer 2 niveaux d'un coup
        while (currentExperience >= experienceToNextLevel)
        {
            // 1. On consomme l'XP
            currentExperience -= experienceToNextLevel;

            // 2. On monte de niveau
            currentLevel++;

            // 3. On calcule le coût du PROCHAIN niveau
            int growth = GetGrowthForLevel(currentLevel);
            experienceToNextLevel += growth;

            Debug.Log($"LEVEL UP! Niveau {currentLevel}. Prochain requis : {experienceToNextLevel} (+{growth})");
            OnLevelUp?.Invoke();
        }
    }

    private int GetGrowthForLevel(int level)
    {
        // On cherche dans quel palier on se trouve
        foreach (var tier in growthTiers)
        {
            if (level <= tier.maxLevel)
            {
                return tier.growthAmount;
            }
        }
        // Si on a dépassé tous les paliers, on utilise la valeur par défaut
        return defaultGrowth;
    }

    private void UpdateInterface()
    {
        // Sécurité pour éviter la division par zéro
        if (experienceToNextLevel == 0) experienceToNextLevel = 100;

        float ratio = (float)currentExperience / experienceToNextLevel;
        OnExperienceChanged?.Invoke(ratio);
    }
}