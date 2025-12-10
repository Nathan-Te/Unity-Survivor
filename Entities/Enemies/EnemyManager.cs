using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using System;

[DefaultExecutionOrder(-50)]
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    // --- EVENTS ---
    public event Action<int> OnEnemyCountChanged; // Pour l'UI
    public event Action<Vector3> OnEnemyDeathPosition; // Pour les Autels

    [Header("Réglages")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Performance")]
    [SerializeField] private int maxEnemiesCapacity = 2000;

    // --- C'est cette ligne qui manquait ! ---
    public bool IsAtCapacity => _activeEnemies.Count >= maxEnemiesCapacity;

    [Header("Steering Settings")]
    [SerializeField] private float separationWeight = 1.5f;
    [SerializeField] private float rayDistance = 5.0f;
    [SerializeField] private float rayAngle = 45f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float avoidanceBlendSpeed = 3f;

    // Listes C#
    private List<EnemyController> _activeEnemies = new List<EnemyController>();
    private TransformAccessArray _transformAccessArray;

    // Données Job
    private NativeList<float> _moveSpeeds;
    private NativeList<float> _stopDistances;
    private NativeList<float> _fleeDistances;

    // Buffers Fixes
    private NativeArray<RaycastCommand> _rayCommands;
    private NativeArray<RaycastHit> _rayResults;
    private NativeArray<float3> _previousDirections;

    private Dictionary<int, EnemyController> _colliderCache = new Dictionary<int, EnemyController>();

    private bool _isDisposed = false;

    private void Awake()
    {
        Instance = this;

        _moveSpeeds = new NativeList<float>(maxEnemiesCapacity, Allocator.Persistent);
        _stopDistances = new NativeList<float>(maxEnemiesCapacity, Allocator.Persistent);
        _fleeDistances = new NativeList<float>(maxEnemiesCapacity, Allocator.Persistent);

        _transformAccessArray = new TransformAccessArray(maxEnemiesCapacity);

        int rayCapacity = maxEnemiesCapacity * 3;
        _rayCommands = new NativeArray<RaycastCommand>(rayCapacity, Allocator.Persistent);
        _rayResults = new NativeArray<RaycastHit>(rayCapacity, Allocator.Persistent);

        _previousDirections = new NativeArray<float3>(maxEnemiesCapacity, Allocator.Persistent);
    }

    private void OnApplicationQuit()
    {
        CleanupNativeCollections();
    }

    private void OnDestroy()
    {
        CleanupNativeCollections();
    }

    private void CleanupNativeCollections()
    {
        if (_isDisposed) return; // Évite double Dispose
        _isDisposed = true;

        _activeEnemies.Clear();
        _colliderCache.Clear();

        // Complete any pending jobs first
        JobHandle.ScheduleBatchedJobs();

        if (_moveSpeeds.IsCreated) _moveSpeeds.Dispose();
        if (_stopDistances.IsCreated) _stopDistances.Dispose();
        if (_fleeDistances.IsCreated) _fleeDistances.Dispose();
        if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();

        if (_rayCommands.IsCreated) _rayCommands.Dispose();
        if (_rayResults.IsCreated) _rayResults.Dispose();
        if (_previousDirections.IsCreated) _previousDirections.Dispose();

        Debug.Log("[EnemyManager] Native Collections libérées");
    }

    /// <summary>
    /// Tente de libérer une place en supprimant l'ennemi le plus éloigné
    /// </summary>
    /// <param name="minDistanceRecycle">Distance minimum pour accepter le recyclage (ex: 40m)</param>
    /// <returns>True si une place a été libérée</returns>
    public bool TryFreeSpaceByRecycling(float minDistanceRecycle)
    {
        if (_activeEnemies.Count == 0) return false;

        float maxDistSq = -1f;
        int bestIndex = -1;
        float minDistSq = minDistanceRecycle * minDistanceRecycle;
        Vector3 playerPos = playerTransform.position;

        // On parcourt la liste pour trouver le plus loin
        // C'est rapide (O(N)) et on ne le fait que si le spawn est bloqué
        for (int i = 0; i < _activeEnemies.Count; i++)
        {
            if (_activeEnemies[i] == null) continue;

            // On utilise le Transform directement (plus fiable que le buffer qui a 1 frame de retard)
            float dSq = (_activeEnemies[i].transform.position - playerPos).sqrMagnitude;

            if (dSq > maxDistSq)
            {
                maxDistSq = dSq;
                bestIndex = i;
            }
        }

        // Si on a trouvé un candidat assez loin
        if (bestIndex != -1 && maxDistSq > minDistSq)
        {
            // On le supprime silencieusement
            _activeEnemies[bestIndex].SilentDespawn();
            return true; // Une place est libre !
        }

        return false; // Tous les ennemis sont trop proches, on ne peut rien faire
    }

    private void Update()
    {
        if (playerTransform == null || _activeEnemies.Count == 0) return;

        SyncDataForJob();

        // 1. Raycasts
        PrepareRaycasts();

        // 2. Exécution des Raycasts
        int activeRayCount = _activeEnemies.Count * 3;
        if (activeRayCount > _rayCommands.Length) activeRayCount = _rayCommands.Length;

        NativeArray<RaycastCommand> cmdSlice = _rayCommands.GetSubArray(0, activeRayCount);
        NativeArray<RaycastHit> resSlice = _rayResults.GetSubArray(0, activeRayCount);

        JobHandle rayHandle = RaycastCommand.ScheduleBatch(cmdSlice, resSlice, 10);

        // 3. Mouvement
        MoveEnemiesJob moveJob = new MoveEnemiesJob
        {
            PlayerPosition = playerTransform.position,
            DeltaTime = Time.deltaTime,

            MoveSpeeds = _moveSpeeds.AsArray(),
            StopDistances = _stopDistances.AsArray(),
            FleeDistances = _fleeDistances.AsArray(),

            RayResults = _rayResults,
            PreviousDirections = _previousDirections,

            AvoidanceWeight = separationWeight,
            RotationSpeed = rotationSpeed,
            AvoidanceBlendSpeed = avoidanceBlendSpeed
        };

        JobHandle moveHandle = moveJob.Schedule(_transformAccessArray, rayHandle);
        moveHandle.Complete();

        Physics.SyncTransforms();
    }

    private void SyncDataForJob()
    {
        for (int i = 0; i < _activeEnemies.Count; i++)
        {
            if (_activeEnemies[i] != null)
                _moveSpeeds[i] = _activeEnemies[i].currentSpeed;
        }
    }

    private void PrepareRaycasts()
    {
        QueryParameters queryParams = new QueryParameters { layerMask = obstacleLayer, hitTriggers = QueryTriggerInteraction.Ignore };

        int count = _activeEnemies.Count;
        if (count > maxEnemiesCapacity) count = maxEnemiesCapacity;

        for (int i = 0; i < count; i++)
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

    // --- GESTION LISTES ---

    public bool RegisterEnemy(EnemyController enemy, Collider col)
    {
        // Si plein, on renvoie FAUX (Échec)
        if (_activeEnemies.Count >= maxEnemiesCapacity)
        {
            return false;
        }

        if (!_activeEnemies.Contains(enemy))
        {
            _activeEnemies.Add(enemy);
            _transformAccessArray.Add(enemy.transform);

            _moveSpeeds.Add(enemy.currentSpeed);
            _stopDistances.Add(enemy.Data.stopDistance);
            _fleeDistances.Add(enemy.Data.fleeDistance);

            if (col != null) _colliderCache.TryAdd(col.GetInstanceID(), enemy);

            OnEnemyCountChanged?.Invoke(_activeEnemies.Count);
        }

        return true; // SUCCÈS
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
                _stopDistances[index] = _stopDistances[last];
                _fleeDistances[index] = _fleeDistances[last];

                _previousDirections[index] = _previousDirections[last];
                _previousDirections[last] = float3.zero;
            }
            else
            {
                _previousDirections[last] = float3.zero;
            }

            _activeEnemies.RemoveAt(last);
            _moveSpeeds.RemoveAtSwapBack(last);
            _stopDistances.RemoveAtSwapBack(last);
            _fleeDistances.RemoveAtSwapBack(last);
            _transformAccessArray.RemoveAtSwapBack(index);

            if (col != null) _colliderCache.Remove(col.GetInstanceID());

            OnEnemyCountChanged?.Invoke(_activeEnemies.Count);
        }
    }

    public void NotifyEnemyDeath(Vector3 position)
    {
        OnEnemyDeathPosition?.Invoke(position);
    }

    public void DebugKillAllEnemies()
    {
        for (int i = _activeEnemies.Count - 1; i >= 0; i--)
        {
            if (_activeEnemies[i] != null) _activeEnemies[i].TakeDamage(99999f);
        }
    }

    public bool TryGetEnemyByCollider(Collider col, out EnemyController enemy) => _colliderCache.TryGetValue(col.GetInstanceID(), out enemy);

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

    public Transform GetTarget(Vector3 sourcePos, float range, TargetingMode mode, float areaSize = 2f, bool checkVisibility = true)
    {
        switch (mode)
        {
            case TargetingMode.Nearest:
                return TargetingUtils.GetNearestEnemy(_activeEnemies, sourcePos, range, checkVisibility, obstacleLayer);
            case TargetingMode.HighestDensity:
                return TargetingUtils.GetDensestCluster(_activeEnemies, sourcePos, range, areaSize, checkVisibility, obstacleLayer);
            case TargetingMode.Random:
                return TargetingUtils.GetRandomEnemy(_activeEnemies, sourcePos, range, checkVisibility, obstacleLayer);
            default:
                return null;
        }
    }
}

[BurstCompile]
public struct MoveEnemiesJob : IJobParallelForTransform
{
    public float3 PlayerPosition;
    public float DeltaTime;
    public float AvoidanceWeight;
    public float RotationSpeed;
    public float AvoidanceBlendSpeed;

    [ReadOnly] public NativeArray<float> MoveSpeeds;
    [ReadOnly] public NativeArray<float> StopDistances;
    [ReadOnly] public NativeArray<float> FleeDistances;
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

        float3 behaviorDir = dirToPlayer;
        float currentSpeedMultiplier = 1f;

        if (fleeDist > 0 && distanceToPlayerSqr < (fleeDist * fleeDist))
        {
            behaviorDir = -dirToPlayer;
        }
        else if (stopDist > 0 && distanceToPlayerSqr < (stopDist * stopDist))
        {
            behaviorDir = float3.zero;
            currentSpeedMultiplier = 0f;
        }

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
                float3 nudge = float3.zero;
                if (hitCenter) nudge = (index % 2 == 0) ? right : left;
                if (hitLeft) nudge += right;
                if (hitRight) nudge += left;

                finalDirection = math.normalize(behaviorDir + nudge * AvoidanceWeight);
            }
        }

        if (math.lengthsq(PreviousDirections[index]) > 0.01f)
        {
            finalDirection = math.normalize(math.lerp(PreviousDirections[index], finalDirection, AvoidanceBlendSpeed * DeltaTime));
        }
        PreviousDirections[index] = finalDirection;

        transform.position += (Vector3)(finalDirection * MoveSpeeds[index] * currentSpeedMultiplier * DeltaTime);

        if (math.lengthsq(finalDirection) > 0.01f)
        {
            quaternion targetRot = quaternion.LookRotation(finalDirection, new float3(0, 1, 0));
            transform.rotation = math.slerp(transform.rotation, targetRot, RotationSpeed * DeltaTime);
        }
    }
}