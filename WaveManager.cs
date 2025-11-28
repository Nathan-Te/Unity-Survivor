using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct EnemySpawnConfig
{
    public EnemyData enemy;
    [Range(0f, 100f)] public float weight; // Probabilité relative (ex: 80 pour Squelette, 20 pour Archer)
}

[System.Serializable]
public struct TimedSpawn
{
    public string name; // Juste pour l'orga dans l'inspecteur (ex: "Mid-Boss")
    public EnemyData enemy;
    public float spawnTime; // À quelle seconde de la vague il apparaît (ex: 30)
    public int count;       // Combien en faire apparaître d'un coup (ex: 1 Boss, ou 5 Élites)
}

[System.Serializable]
public class WaveDefinition
{
    public string waveName = "Vague 1";
    public float duration = 60f;
    public float spawnInterval = 1f;

    [Header("Spawns Aléatoires (En continu)")]
    public List<EnemySpawnConfig> randomSpawns;

    [Header("Spawns Fixes (Événements)")]
    public List<TimedSpawn> timedSpawns;

    // Cache pour le runtime
    [HideInInspector] public float totalWeight;
}

public class WaveManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private List<WaveDefinition> waves = new List<WaveDefinition>();
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private bool loopLastWave = true;

    [Header("État (Read Only)")]
    public int currentWaveIndex = 0;
    public float waveTimer = 0f;

    private float _spawnTimer;
    private int _nextTimedSpawnIndex; // Pour savoir quel est le prochain événement
    private Transform _playerTransform;

    // Listes triées pour l'exécution
    private List<TimedSpawn> _currentWaveTimedSpawns;

    private void Start()
    {
        if (PlayerController.Instance != null)
            _playerTransform = PlayerController.Instance.transform;

        if (waves.Count > 0)
        {
            InitializeWave(0);
        }
    }

    private void Update()
    {
        if (_playerTransform == null || waves.Count == 0) return;

        // Gestion de la boucle finale
        if (currentWaveIndex >= waves.Count)
        {
            if (loopLastWave) InitializeWave(waves.Count - 1);
            else return;
        }

        WaveDefinition currentWave = waves[currentWaveIndex];

        // 1. Avancée du Temps
        waveTimer += Time.deltaTime;

        // 2. Vérification des Spawns Fixes (Boss / Élites)
        CheckTimedSpawns();

        // 3. Vérification des Spawns Aléatoires (Horde)
        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= currentWave.spawnInterval)
        {
            SpawnRandomEnemy(currentWave);
            _spawnTimer = 0f;
        }

        // 4. Fin de Vague
        if (waveTimer >= currentWave.duration)
        {
            NextWave();
        }
    }

    private void InitializeWave(int index)
    {
        currentWaveIndex = index;
        waveTimer = 0f;
        _spawnTimer = 0f;
        _nextTimedSpawnIndex = 0;

        WaveDefinition wave = waves[currentWaveIndex];

        // A. Calcul des poids pour le random
        wave.totalWeight = 0f;
        foreach (var config in wave.randomSpawns)
        {
            wave.totalWeight += config.weight;
        }

        // B. Préparation des événements fixes (Triés par temps pour l'efficacité)
        if (wave.timedSpawns != null)
        {
            _currentWaveTimedSpawns = wave.timedSpawns.OrderBy(x => x.spawnTime).ToList();
        }
        else
        {
            _currentWaveTimedSpawns = new List<TimedSpawn>();
        }

        Debug.Log($"WaveManager: Début {wave.waveName}");
    }

    private void NextWave()
    {
        InitializeWave(currentWaveIndex + 1);
    }

    // --- LOGIQUE SPAWN ALÉATOIRE (PONDÉRÉ) ---
    private void SpawnRandomEnemy(WaveDefinition wave)
    {
        if (wave.randomSpawns == null || wave.randomSpawns.Count == 0) return;

        // Algorithme de sélection pondérée
        float randomValue = Random.Range(0, wave.totalWeight);
        float currentSum = 0;
        EnemyData selectedEnemy = null;

        foreach (var config in wave.randomSpawns)
        {
            currentSum += config.weight;
            if (randomValue <= currentSum)
            {
                selectedEnemy = config.enemy;
                break;
            }
        }

        if (selectedEnemy != null)
        {
            SpawnEntity(selectedEnemy);
        }
    }

    // --- LOGIQUE SPAWN FIXE (BOSS) ---
    private void CheckTimedSpawns()
    {
        // Tant qu'il reste des événements et que le temps est dépassé
        while (_nextTimedSpawnIndex < _currentWaveTimedSpawns.Count &&
               waveTimer >= _currentWaveTimedSpawns[_nextTimedSpawnIndex].spawnTime)
        {
            TimedSpawn spawnInfo = _currentWaveTimedSpawns[_nextTimedSpawnIndex];

            // Spawn multiple (si count > 1)
            for (int i = 0; i < spawnInfo.count; i++)
            {
                SpawnEntity(spawnInfo.enemy);
            }

            Debug.Log($"WaveManager: Event '{spawnInfo.name}' déclenché à {waveTimer:F1}s");
            _nextTimedSpawnIndex++;
        }
    }

    // Méthode générique de spawn
    private void SpawnEntity(EnemyData data)
    {
        if (data == null || data.prefab == null) return;

        Vector2 randomCircle = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPos = _playerTransform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        GameObject enemyObj = EnemyPool.Instance.GetEnemy(data.prefab, spawnPos, Quaternion.identity);

        if (enemyObj.TryGetComponent<EnemyController>(out var controller))
        {
            controller.ResetEnemy();
        }
    }
}