using System;
using UnityEngine;

public enum ComboMode
{
    Arcade,  // Multiplicateur retombe à x1 si le timer expire
    Modern   // Multiplicateur baisse d'un cran si le timer expire
}

public class ArcadeScoreSystem : Singleton<ArcadeScoreSystem>
{
    [Header("Settings")]
    [SerializeField] private ComboMode comboMode = ComboMode.Modern;

    [Header("Combo Timer - Base Settings")]
    [SerializeField] private float comboTimerMax = 3f; // Durée max du timer de combo
    [SerializeField] private float baseDecayRate = 1f; // Vitesse de décroissance de base (palier 0)
    [SerializeField] private float baseTimeAddedPerKill = 1.5f; // Temps ajouté à chaque kill (palier 0)

    [Header("Multiplier Settings")]
    [SerializeField] private int[] comboThresholds = { 0, 5, 10, 20, 50, 100 }; // Paliers de combo
    [SerializeField] private float[] multipliers = { 1f, 1.5f, 2f, 3f, 5f, 10f }; // Multiplicateurs correspondants

    [Header("Dynamic Difficulty Per Tier")]
    [Tooltip("Multiplicateur de vitesse de décroissance par palier (1 = normal, 1.5 = 50% plus rapide)")]
    [SerializeField] private float[] decayRateMultipliers = { 1f, 1.2f, 1.5f, 2f, 2.5f, 3f };

    [Tooltip("Multiplicateur de temps ajouté par kill par palier (1 = normal, 0.8 = 20% moins de temps)")]
    [SerializeField] private float[] timeAddedMultipliers = { 1f, 0.9f, 0.8f, 0.7f, 0.6f, 0.5f };

    [Header("Visual Feedback")]
    [SerializeField] private bool clampTimerToMax = true; // Empêche le timer de dépasser le max

    // --- EVENTS ---
    public event Action<int> OnScoreChanged; // Nouveau score
    public event Action<int> OnComboChanged; // Nouveau combo
    public event Action<float> OnMultiplierChanged; // Nouveau multiplicateur
    public event Action<float, float> OnComboTimerChanged; // (current, max)
    public event Action OnComboBroken; // Combo brisé

    // --- STATE ---
    private int _totalScore = 0;
    private int _currentCombo = 0;
    private float _comboTimer = 0f;
    private float _currentMultiplier = 1f;

    // --- DYNAMIC DIFFICULTY ---
    private float _currentDecayRate = 1f;
    private float _currentTimeAddedPerKill = 1.5f;

    public int TotalScore => _totalScore;
    public int CurrentCombo => _currentCombo;
    public float CurrentMultiplier => _currentMultiplier;
    public float ComboTimer => _comboTimer;
    public float ComboTimerMax => comboTimerMax;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        // Always initialize (even after scene reload)
        // Initialiser les valeurs dynamiques au palier de base
        _currentDecayRate = baseDecayRate;
        _currentTimeAddedPerKill = baseTimeAddedPerKill;

        // S'abonner aux morts d'ennemis
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.OnEnemyKilledWithScore += OnEnemyKilled;
        }
    }

    private void Update()
    {
        // Décrémenter le timer de combo avec la vitesse dynamique
        if (_comboTimer > 0f)
        {
            _comboTimer -= _currentDecayRate * Time.deltaTime;

            if (_comboTimer <= 0f)
            {
                _comboTimer = 0f;
                BreakCombo();
            }

            OnComboTimerChanged?.Invoke(_comboTimer, comboTimerMax);
        }
    }

    private void OnEnemyKilled(int scoreValue, Vector3 position)
    {
        AddScore(scoreValue);
    }

    /// <summary>
    /// Ajoute des points au score avec le multiplicateur actuel
    /// </summary>
    public void AddScore(int baseScore)
    {
        // Incrémenter le combo
        _currentCombo++;
        OnComboChanged?.Invoke(_currentCombo);

        // Calculer et mettre à jour le multiplicateur ET la difficulté dynamique
        UpdateMultiplier();
        UpdateDynamicDifficulty();

        // Ajouter le score avec multiplicateur
        int scoreToAdd = Mathf.RoundToInt(baseScore * _currentMultiplier);
        _totalScore += scoreToAdd;
        OnScoreChanged?.Invoke(_totalScore);

        // Recharger le timer de combo avec la valeur dynamique
        _comboTimer += _currentTimeAddedPerKill;

        if (clampTimerToMax)
        {
            _comboTimer = Mathf.Min(_comboTimer, comboTimerMax);
        }

        OnComboTimerChanged?.Invoke(_comboTimer, comboTimerMax);
    }

    /// <summary>
    /// Met à jour le multiplicateur en fonction du combo actuel
    /// </summary>
    private void UpdateMultiplier()
    {
        float newMultiplier = 1f;

        // Trouver le bon palier
        for (int i = comboThresholds.Length - 1; i >= 0; i--)
        {
            if (_currentCombo >= comboThresholds[i])
            {
                newMultiplier = multipliers[i];
                break;
            }
        }

        if (newMultiplier != _currentMultiplier)
        {
            _currentMultiplier = newMultiplier;
            OnMultiplierChanged?.Invoke(_currentMultiplier);
        }
    }

    /// <summary>
    /// Met à jour la difficulté dynamique (decay rate et time added) en fonction du palier actuel
    /// </summary>
    private void UpdateDynamicDifficulty()
    {
        int tierIndex = GetCurrentTierIndex();

        // Vérifier que l'index est valide
        if (tierIndex < 0 || tierIndex >= decayRateMultipliers.Length)
        {
            tierIndex = 0;
        }

        // Calculer la nouvelle vitesse de décroissance
        _currentDecayRate = baseDecayRate * decayRateMultipliers[tierIndex];

        // Calculer le nouveau temps ajouté par kill
        if (tierIndex < timeAddedMultipliers.Length)
        {
            _currentTimeAddedPerKill = baseTimeAddedPerKill * timeAddedMultipliers[tierIndex];
        }
        else
        {
            _currentTimeAddedPerKill = baseTimeAddedPerKill * timeAddedMultipliers[timeAddedMultipliers.Length - 1];
        }
    }

    /// <summary>
    /// Brise le combo (appelé quand le timer atteint 0)
    /// </summary>
    private void BreakCombo()
    {
        if (_currentCombo == 0) return; // Déjà brisé

        OnComboBroken?.Invoke();

        switch (comboMode)
        {
            case ComboMode.Arcade:
                // Mode Arcade : Reset complet
                _currentCombo = 0;
                _currentMultiplier = 1f;
                OnComboChanged?.Invoke(_currentCombo);
                OnMultiplierChanged?.Invoke(_currentMultiplier);
                break;

            case ComboMode.Modern:
                // Mode Moderne : Baisse d'un palier
                int currentTierIndex = GetCurrentTierIndex();
                if (currentTierIndex > 0)
                {
                    // Descendre au palier précédent
                    int previousThreshold = comboThresholds[currentTierIndex - 1];
                    _currentCombo = previousThreshold;
                    OnComboChanged?.Invoke(_currentCombo);
                    UpdateMultiplier();
                }
                else
                {
                    // On est déjà au palier le plus bas
                    _currentCombo = 0;
                    _currentMultiplier = 1f;
                    OnComboChanged?.Invoke(_currentCombo);
                    OnMultiplierChanged?.Invoke(_currentMultiplier);
                }
                break;
        }

        _comboTimer = 0f;

        // Réinitialiser la difficulté dynamique
        UpdateDynamicDifficulty();
    }

    /// <summary>
    /// Retourne l'index du palier actuel
    /// </summary>
    private int GetCurrentTierIndex()
    {
        for (int i = comboThresholds.Length - 1; i >= 0; i--)
        {
            if (_currentCombo >= comboThresholds[i])
            {
                return i;
            }
        }
        return 0;
    }

    /// <summary>
    /// Réinitialise le système de score
    /// </summary>
    public void ResetScore()
    {
        _totalScore = 0;
        _currentCombo = 0;
        _comboTimer = 0f;
        _currentMultiplier = 1f;

        // Réinitialiser la difficulté dynamique au palier de base
        _currentDecayRate = baseDecayRate;
        _currentTimeAddedPerKill = baseTimeAddedPerKill;

        OnScoreChanged?.Invoke(_totalScore);
        OnComboChanged?.Invoke(_currentCombo);
        OnMultiplierChanged?.Invoke(_currentMultiplier);
        OnComboTimerChanged?.Invoke(_comboTimer, comboTimerMax);
    }

    protected override void OnDestroy()
    {
        // Use FindFirstObjectByType to avoid Singleton getter error during scene unload
        var enemyManager = FindFirstObjectByType<EnemyManager>();
        if (enemyManager != null)
        {
            enemyManager.OnEnemyKilledWithScore -= OnEnemyKilled;
        }

        base.OnDestroy();

        // Instance is managed by Singleton<T> base class, no need to set to null
    }
}
