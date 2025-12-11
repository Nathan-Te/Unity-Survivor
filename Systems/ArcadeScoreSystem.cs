using System;
using UnityEngine;

public enum ComboMode
{
    Arcade,  // Multiplicateur retombe à x1 si le timer expire
    Modern   // Multiplicateur baisse d'un cran si le timer expire
}

public class ArcadeScoreSystem : MonoBehaviour
{
    public static ArcadeScoreSystem Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private ComboMode comboMode = ComboMode.Modern;

    [Header("Combo Timer")]
    [SerializeField] private float comboTimerMax = 3f; // Durée max du timer de combo
    [SerializeField] private float comboTimerDecayRate = 1f; // Vitesse de décroissance par seconde
    [SerializeField] private float timeAddedPerKill = 1.5f; // Temps ajouté à chaque kill

    [Header("Multiplier Settings")]
    [SerializeField] private int[] comboThresholds = { 0, 5, 10, 20, 50, 100 }; // Paliers de combo
    [SerializeField] private float[] multipliers = { 1f, 1.5f, 2f, 3f, 5f, 10f }; // Multiplicateurs correspondants

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

    public int TotalScore => _totalScore;
    public int CurrentCombo => _currentCombo;
    public float CurrentMultiplier => _currentMultiplier;
    public float ComboTimer => _comboTimer;
    public float ComboTimerMax => comboTimerMax;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // S'abonner aux morts d'ennemis
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.OnEnemyKilledWithScore += OnEnemyKilled;
        }
    }

    private void Update()
    {
        // Décrémenter le timer de combo
        if (_comboTimer > 0f)
        {
            _comboTimer -= comboTimerDecayRate * Time.deltaTime;

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

        // Calculer et mettre à jour le multiplicateur
        UpdateMultiplier();

        // Ajouter le score avec multiplicateur
        int scoreToAdd = Mathf.RoundToInt(baseScore * _currentMultiplier);
        _totalScore += scoreToAdd;
        OnScoreChanged?.Invoke(_totalScore);

        // Recharger le timer de combo
        _comboTimer += timeAddedPerKill;

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

        OnScoreChanged?.Invoke(_totalScore);
        OnComboChanged?.Invoke(_currentCombo);
        OnMultiplierChanged?.Invoke(_currentMultiplier);
        OnComboTimerChanged?.Invoke(_comboTimer, comboTimerMax);
    }

    private void OnDestroy()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.OnEnemyKilledWithScore -= OnEnemyKilled;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
