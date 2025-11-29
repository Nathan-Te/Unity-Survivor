using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

[DefaultExecutionOrder(-50)]
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [Header("Réglages")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Steering Settings")]
    [SerializeField] private float separationWeight = 1.5f;
    [SerializeField] private float rayDistance = 5.0f;
    [SerializeField] private float rayAngle = 45f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float avoidanceBlendSpeed = 3f;

    // Données Job System
    private List<EnemyController> _activeEnemies = new List<EnemyController>();
    private TransformAccessArray _transformAccessArray;

    // NOUVEAUX TABLEAUX DE DONNÉES
    private NativeList<float> _moveSpeeds;
    private NativeList<float> _stopDistances; // Distance d'arrêt
    private NativeList<float> _fleeDistances; // Distance de fuite

    private NativeArray<RaycastCommand> _rayCommands;
    private NativeArray<RaycastHit> _rayResults;
    private NativeArray<float3> _previousDirections;
    private int _currentCapacity = 0;

    private Dictionary<int, EnemyController> _colliderCache = new Dictionary<int, EnemyController>();

    private void Awake()
    {
        Instance = this;
        _moveSpeeds = new NativeList<float>(Allocator.Persistent);
        _stopDistances = new NativeList<float>(Allocator.Persistent); // Nouveau
        _fleeDistances = new NativeList<float>(Allocator.Persistent); // Nouveau
        _transformAccessArray = new TransformAccessArray(0);
    }

    private void Update()
    {
        if (playerTransform == null || _activeEnemies.Count == 0) return;

        // --- CORRECTION : Synchronisation des Vitesses ---
        // On met à jour les données du Job avec les valeurs actuelles des scripts
        // (au cas où un Slow ou un Frenzy a été appliqué)
        for (int i = 0; i < _activeEnemies.Count; i++)
        {
            if (_activeEnemies[i] != null)
            {
                _moveSpeeds[i] = _activeEnemies[i].currentSpeed;
            }
        }

        int requiredSize = _activeEnemies.Count * 3;
        if (!_rayCommands.IsCreated || _currentCapacity != requiredSize / 3)
        {
            ResizeBuffers(requiredSize);
        }

        // 1. Raycasts (Inchangé)
        PrepareRaycasts();
        JobHandle rayHandle = RaycastCommand.ScheduleBatch(_rayCommands, _rayResults, 10);

        // 2. Mouvement (Avec logique de Fuite/Arrêt)
        MoveEnemiesJob moveJob = new MoveEnemiesJob
        {
            PlayerPosition = playerTransform.position,
            DeltaTime = Time.deltaTime,

            MoveSpeeds = _moveSpeeds.AsArray(),
            StopDistances = _stopDistances.AsArray(), // Passer les données
            FleeDistances = _fleeDistances.AsArray(), // Passer les données

            RayResults = _rayResults,
            PreviousDirections = _previousDirections,
            AvoidanceWeight = separationWeight,
            RotationSpeed = rotationSpeed,
            AvoidanceBlendSpeed = avoidanceBlendSpeed
        };

        JobHandle moveHandle = moveJob.Schedule(_transformAccessArray, rayHandle);
        moveHandle.Complete();
    }

    private void ResizeBuffers(int requiredSize)
    {
        if (_rayCommands.IsCreated) _rayCommands.Dispose();
        if (_rayResults.IsCreated) _rayResults.Dispose();
        if (_previousDirections.IsCreated) _previousDirections.Dispose();

        _rayCommands = new NativeArray<RaycastCommand>(requiredSize, Allocator.Persistent);
        _rayResults = new NativeArray<RaycastHit>(requiredSize, Allocator.Persistent);
        _previousDirections = new NativeArray<float3>(requiredSize / 3, Allocator.Persistent);
        _currentCapacity = requiredSize / 3;
    }

    private void PrepareRaycasts()
    {
        QueryParameters queryParams = new QueryParameters { layerMask = obstacleLayer, hitTriggers = QueryTriggerInteraction.Ignore };

        for (int i = 0; i < _activeEnemies.Count; i++)
        {
            EnemyController enemy = _activeEnemies[i];
            if (enemy == null) continue;

            int baseIndex = i * 3;
            Vector3 fwd = enemy.transform.forward;
            Vector3 pos = enemy.transform.position + Vector3.up * 1.0f;

            _rayCommands[baseIndex] = new RaycastCommand(pos, fwd, queryParams, rayDistance);
            _rayCommands[baseIndex + 1] = new RaycastCommand(pos, Quaternion.Euler(0, -rayAngle, 0) * fwd, queryParams, rayDistance);
            _rayCommands[baseIndex + 2] = new RaycastCommand(pos, Quaternion.Euler(0, rayAngle, 0) * fwd, queryParams, rayDistance);
        }
    }

    public void DebugKillAllEnemies()
    {
        // On boucle à l'envers car la liste va rétrécir à chaque suppression
        for (int i = _activeEnemies.Count - 1; i >= 0; i--)
        {
            if (_activeEnemies[i] != null)
            {
                // On simule des dégâts infinis pour déclencher la mort propre (Drop XP + Pool)
                _activeEnemies[i].TakeDamage(99999f);
            }
        }
    }

    public void RegisterEnemy(EnemyController enemy, Collider col)
    {
        if (!_activeEnemies.Contains(enemy))
        {
            _activeEnemies.Add(enemy);
            _transformAccessArray.Add(enemy.transform);

            // On enregistre les stats de comportement
            _moveSpeeds.Add(enemy.currentSpeed);
            _stopDistances.Add(enemy.Data.stopDistance); // Nouveau
            _fleeDistances.Add(enemy.Data.fleeDistance); // Nouveau

            if (col != null) _colliderCache.TryAdd(col.GetInstanceID(), enemy);
        }
    }

    public void UnregisterEnemy(EnemyController enemy, Collider col)
    {
        if (_activeEnemies.Contains(enemy))
        {
            int index = _activeEnemies.IndexOf(enemy);
            int last = _activeEnemies.Count - 1;

            if (index != last)
            {
                _activeEnemies[index] = _activeEnemies[last];
                _moveSpeeds[index] = _moveSpeeds[last];
                _stopDistances[index] = _stopDistances[last]; // Nouveau
                _fleeDistances[index] = _fleeDistances[last]; // Nouveau
            }

            _activeEnemies.RemoveAt(last);
            _moveSpeeds.RemoveAtSwapBack(last);
            _stopDistances.RemoveAtSwapBack(last); // Nouveau
            _fleeDistances.RemoveAtSwapBack(last); // Nouveau
            _transformAccessArray.RemoveAtSwapBack(index);

            if (col != null) _colliderCache.Remove(col.GetInstanceID());
        }
    }

    // --- SYSTÈME DE CIBLAGE (Le cerveau du SpellManager) ---

    public Transform GetTarget(Vector3 sourcePos, float range, TargetingMode mode, float areaSize = 2f, bool checkVisibility = true)
    {
        switch (mode)
        {
            case TargetingMode.Nearest:
                return GetNearestEnemy(sourcePos, range, checkVisibility);
            case TargetingMode.HighestDensity:
                return GetDensestCluster(sourcePos, range, areaSize, checkVisibility);
            case TargetingMode.Random:
                return GetRandomEnemy(sourcePos, range, checkVisibility);
            default:
                return null;
        }
    }

    // 1. LE PLUS PROCHE
    private Transform GetNearestEnemy(Vector3 sourcePos, float range, bool checkVisibility)
    {
        EnemyController nearest = null;
        float minDistSqr = range * range;

        for (int i = 0; i < _activeEnemies.Count; i++)
        {
            EnemyController enemy = _activeEnemies[i];
            if (enemy == null) continue;

            float distSqr = (enemy.transform.position - sourcePos).sqrMagnitude;
            if (distSqr < minDistSqr)
            {
                if (checkVisibility && !IsVisible(sourcePos, enemy.transform.position)) continue;

                minDistSqr = distSqr;
                nearest = enemy;
            }
        }
        return nearest != null ? nearest.transform : null;
    }

    // 2. LE PLUS GROS GROUPE
    private Transform GetDensestCluster(Vector3 sourcePos, float range, float areaSize, bool checkVisibility)
    {
        // Réflexe de Survie : Si un ennemi est trop près (< 4m), on l'abat en priorité
        Transform panicTarget = GetNearestEnemy(sourcePos, 4.0f, checkVisibility);
        if (panicTarget != null) return panicTarget;

        Transform bestTarget = null;
        int maxNeighbors = -1;
        float rangeSqr = range * range;
        float areaSqr = areaSize * areaSize;

        for (int i = 0; i < _activeEnemies.Count; i++)
        {
            EnemyController candidate = _activeEnemies[i];
            if (candidate == null) continue;
            if ((candidate.transform.position - sourcePos).sqrMagnitude > rangeSqr) continue;
            if (checkVisibility && !IsVisible(sourcePos, candidate.transform.position)) continue;

            int neighborCount = 0;
            for (int j = 0; j < _activeEnemies.Count; j++)
            {
                if (i == j) continue;
                if ((_activeEnemies[j].transform.position - candidate.transform.position).sqrMagnitude <= areaSqr)
                    neighborCount++;
            }

            if (neighborCount > maxNeighbors)
            {
                maxNeighbors = neighborCount;
                bestTarget = candidate.transform;
            }
        }
        return bestTarget != null ? bestTarget : GetNearestEnemy(sourcePos, range, checkVisibility);
    }

    // 3. ALÉATOIRE
    private Transform GetRandomEnemy(Vector3 sourcePos, float range, bool checkVisibility)
    {
        List<Transform> candidates = new List<Transform>();
        float rangeSqr = range * range;

        for (int i = 0; i < _activeEnemies.Count; i++)
        {
            if (_activeEnemies[i] == null) continue;
            if ((_activeEnemies[i].transform.position - sourcePos).sqrMagnitude <= rangeSqr)
            {
                if (!checkVisibility || IsVisible(sourcePos, _activeEnemies[i].transform.position))
                    candidates.Add(_activeEnemies[i].transform);
            }
        }

        if (candidates.Count > 0) return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        return null;
    }

    // --- UTILITAIRE : VUE & AOE ---

    // Vérifie si un mur bloque la vue (La fonction manquante !)
    private bool IsVisible(Vector3 start, Vector3 end)
    {
        // On s'assure que la destination est aussi surélevée pour éviter de tirer dans le sol
        Vector3 targetPoint = new Vector3(end.x, start.y, end.z);
        Vector3 dir = targetPoint - start;
        float dist = dir.magnitude;

        // Si on touche un obstacle
        if (Physics.Raycast(start, dir.normalized, dist, obstacleLayer))
        {
            return false;
        }
        return true;
    }

    // Pour les explosions (AOE)
    public List<EnemyController> GetEnemiesInRange(Vector3 center, float radius)
    {
        List<EnemyController> results = new List<EnemyController>();
        float radiusSqr = radius * radius;
        for (int i = 0; i < _activeEnemies.Count; i++)
        {
            if (_activeEnemies[i] == null) continue;
            if ((_activeEnemies[i].transform.position - center).sqrMagnitude <= radiusSqr)
                results.Add(_activeEnemies[i]);
        }
        return results;
    }

    // Méthodes helpers nécessaires (copiées/collées de l'ancienne version)
    public bool TryGetEnemyByCollider(Collider col, out EnemyController enemy) => _colliderCache.TryGetValue(col.GetInstanceID(), out enemy);

    private void OnDestroy()
    {
        if (_moveSpeeds.IsCreated) _moveSpeeds.Dispose();
        if (_stopDistances.IsCreated) _stopDistances.Dispose();
        if (_fleeDistances.IsCreated) _fleeDistances.Dispose();
        if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();
        if (_rayCommands.IsCreated) _rayCommands.Dispose();
        if (_rayResults.IsCreated) _rayResults.Dispose();
        if (_previousDirections.IsCreated) _previousDirections.Dispose();
    }
}

// --- JOB DE MOUVEMENT MIS À JOUR ---
[BurstCompile]
public struct MoveEnemiesJob : IJobParallelForTransform
{
    public float3 PlayerPosition;
    public float DeltaTime;
    public float AvoidanceWeight;
    public float RotationSpeed;
    public float AvoidanceBlendSpeed;

    [ReadOnly] public NativeArray<float> MoveSpeeds;
    [ReadOnly] public NativeArray<float> StopDistances; // Nouveau
    [ReadOnly] public NativeArray<float> FleeDistances; // Nouveau
    [ReadOnly] public NativeArray<RaycastHit> RayResults;
    public NativeArray<float3> PreviousDirections;

    public void Execute(int index, TransformAccess transform)
    {
        float3 currentPos = transform.position;
        float3 vectorToPlayer = PlayerPosition - currentPos;
        float distanceToPlayerSqr = math.lengthsq(vectorToPlayer);

        float3 dirToPlayer = math.normalize(vectorToPlayer);
        dirToPlayer.y = 0;

        float stopDist = StopDistances[index];
        float fleeDist = FleeDistances[index];

        // --- LOGIQUE DE COMPORTEMENT (Melee / Range / Flee) ---
        float3 behaviorDir = dirToPlayer;
        float currentSpeedMultiplier = 1f;

        // Si on est trop près (Zone de Fuite)
        if (fleeDist > 0 && distanceToPlayerSqr < (fleeDist * fleeDist))
        {
            behaviorDir = -dirToPlayer; // On inverse la direction (FUITE)
        }
        // Si on est à bonne distance (Zone d'Arrêt pour tirer)
        else if (stopDist > 0 && distanceToPlayerSqr < (stopDist * stopDist))
        {
            behaviorDir = float3.zero; // On s'arrête
            currentSpeedMultiplier = 0f;
        }
        // Sinon : On avance vers le joueur (Comportement par défaut)

        // --- STEERING (Obstacles) ---
        // (Uniquement si on bouge)
        float3 finalDirection = behaviorDir;

        if (currentSpeedMultiplier > 0.01f)
        {
            int baseIndex = index * 3;
            bool hitCenter = RayResults[baseIndex].colliderInstanceID != 0;
            bool hitLeft = RayResults[baseIndex + 1].colliderInstanceID != 0;
            bool hitRight = RayResults[baseIndex + 2].colliderInstanceID != 0;

            if (hitCenter || hitLeft || hitRight)
            {
                float3 right = transform.rotation * new float3(1, 0, 0);
                float3 left = transform.rotation * new float3(-1, 0, 0);

                // Logique simplifiée d'évitement
                float3 nudge = float3.zero;
                if (hitCenter) nudge = (index % 2 == 0) ? right : left;
                if (hitLeft) nudge += right;
                if (hitRight) nudge += left;

                finalDirection = math.normalize(behaviorDir + nudge * AvoidanceWeight);
            }
        }

        // Lissage
        if (math.lengthsq(PreviousDirections[index]) > 0.01f)
        {
            finalDirection = math.normalize(math.lerp(PreviousDirections[index], finalDirection, AvoidanceBlendSpeed * DeltaTime));
        }
        PreviousDirections[index] = finalDirection;

        // Application
        transform.position += (Vector3)(finalDirection * MoveSpeeds[index] * currentSpeedMultiplier * DeltaTime);

        if (math.lengthsq(finalDirection) > 0.01f)
        {
            quaternion targetRot = quaternion.LookRotation(finalDirection, new float3(0, 1, 0));
            transform.rotation = math.slerp(transform.rotation, targetRot, RotationSpeed * DeltaTime);
        }
    }
}