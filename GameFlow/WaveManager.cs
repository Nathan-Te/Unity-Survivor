using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct EnemySpawnConfig
{
    public EnemyData enemy;
    [Range(0f, 100f)] public float weight;
}

[System.Serializable]
public struct TimedSpawn
{
    public string name;
    public EnemyData enemy;
    public float spawnTime;
    public int count;
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

    [Header("Sécurité Spawn")] // --- NOUVEAU ---
    [SerializeField] private LayerMask invalidSpawnLayers; // Mettre Obstacle + Destructible (POI)
    [SerializeField] private float checkRadius = 0.5f;     // Taille de la zone à vérifier
    [SerializeField] private float recycleDistance = 45f;

    private float _spawnTimer;
    private int _nextTimedSpawnIndex;
    private Transform _playerTransform;
    private List<TimedSpawn> _currentWaveTimedSpawns;

    private void Start()
    {
        if (PlayerController.Instance != null)
            _playerTransform = PlayerController.Instance.transform;

        if (waves.Count > 0) InitializeWave(0);
        else enabled = false;
    }

    private void Update()
    {
        if (_playerTransform == null) return;
        if (currentWaveIndex >= waves.Count) return;

        WaveDefinition currentWave = waves[currentWaveIndex];

        // 1. Avancée du Temps
        waveTimer += Time.deltaTime;

        // 2. Vérification des Spawns Fixes (Boss/Elites)
        CheckTimedSpawns();

        // 3. Vérification des Spawns Aléatoires (Horde)
        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= currentWave.spawnInterval)
        {
            bool canSpawn = false;

            // CAS A : Il y a de la place
            if (EnemyManager.Instance != null && !EnemyManager.Instance.IsAtCapacity)
            {
                canSpawn = true;
            }
            // CAS B : C'est plein -> On essaie de RECYCLER
            else if (EnemyManager.Instance != null)
            {
                // On demande au Manager : "T'as pas un truc inutile au fond de la classe ?"
                if (EnemyManager.Instance.TryFreeSpaceByRecycling(recycleDistance))
                {
                    canSpawn = true; // Une place s'est libérée !
                }
            }

            if (canSpawn)
            {
                SpawnRandomEnemy(currentWave);
                _spawnTimer = 0f;
            }
        }

        // 4. Fin de Vague
        if (waveTimer >= currentWave.duration)
        {
            NextWave();
        }
    }

    private void InitializeWave(int index)
    {
        if (index >= waves.Count) return;

        currentWaveIndex = index;
        waveTimer = 0f;
        _spawnTimer = 0f;
        _nextTimedSpawnIndex = 0;

        WaveDefinition wave = waves[currentWaveIndex];

        wave.totalWeight = 0f;
        foreach (var config in wave.randomSpawns)
        {
            wave.totalWeight += config.weight;
        }

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
        int nextIndex = currentWaveIndex + 1;

        if (nextIndex >= waves.Count)
        {
            if (loopLastWave && waves.Count > 0)
            {
                InitializeWave(waves.Count - 1);
                Debug.Log("WaveManager: Boucle sur la dernière vague");
            }
            else
            {
                Debug.Log("WaveManager: Toutes les vagues sont terminées !");
                enabled = false;
            }
        }
        else
        {
            InitializeWave(nextIndex);
        }
    }

    private void CheckTimedSpawns()
    {
        if (_currentWaveTimedSpawns == null) return;

        // Pour les events, on force un peu plus, mais on respecte le cap si on ne peut pas recycler
        if (EnemyManager.Instance != null && EnemyManager.Instance.IsAtCapacity)
        {
            // On tente un recyclage agressif pour faire place au Boss/Elite
            if (!EnemyManager.Instance.TryFreeSpaceByRecycling(spawnRadius + 5f))
                return; // Vraiment pas de place, on attend
        }

        while (_nextTimedSpawnIndex < _currentWaveTimedSpawns.Count &&
               waveTimer >= _currentWaveTimedSpawns[_nextTimedSpawnIndex].spawnTime)
        {
            TimedSpawn spawnInfo = _currentWaveTimedSpawns[_nextTimedSpawnIndex];
            for (int i = 0; i < spawnInfo.count; i++) SpawnEntity(spawnInfo.enemy);
            Debug.Log($"WaveManager: Event '{spawnInfo.name}'");
            _nextTimedSpawnIndex++;
        }
    }

    private void SpawnRandomEnemy(WaveDefinition wave)
    {
        if (wave.randomSpawns == null || wave.randomSpawns.Count == 0) return;

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

        if (selectedEnemy != null) SpawnEntity(selectedEnemy);
    }

    private void SpawnEntity(EnemyData data)
    {
        if (data == null || data.prefab == null) return;

        // --- NOUVEAU : Recherche de position valide ---
        Vector3 spawnPos;
        if (TryGetValidSpawnPosition(out spawnPos))
        {
            if (EnemyPool.Instance != null)
            {
                GameObject enemyObj = EnemyPool.Instance.GetEnemy(data.prefab, spawnPos, Quaternion.identity);
                if (enemyObj.TryGetComponent<EnemyController>(out var controller))
                {
                    controller.ResetEnemy();
                }
            }
        }
    }

    private bool TryGetValidSpawnPosition(out Vector3 position)
    {
        position = Vector3.zero;
        int maxAttempts = 10; // On essaie 10 fois max pour pas freezer le jeu

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized * spawnRadius;
            Vector3 candidatePos = _playerTransform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            // Vérification Physics.CheckSphere
            // On vérifie si ça touche le Layer "Obstacle" ou "Destructible" (POI)
            if (!Physics.CheckSphere(candidatePos, checkRadius, invalidSpawnLayers))
            {
                position = candidatePos;
                return true; // C'est libre !
            }
        }

        return false; // Pas trouvé de place (trop dense)
    }
}