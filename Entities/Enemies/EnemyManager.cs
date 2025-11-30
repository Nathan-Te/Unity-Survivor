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

    // Tableaux de données pour le Job
    private NativeList<float> _moveSpeeds;
    private NativeList<float> _stopDistances;
    private NativeList<float> _fleeDistances;

    private NativeArray<RaycastCommand> _rayCommands;
    private NativeArray<RaycastHit> _rayResults;
    private NativeArray<float3> _previousDirections;
    private int _currentCapacity = 0;

    private Dictionary<int, EnemyController> _colliderCache = new Dictionary<int, EnemyController>();

    private void Awake()
    {
        Instance = this;
        _moveSpeeds = new NativeList<float>(Allocator.Persistent);
        _stopDistances = new NativeList<float>(Allocator.Persistent);
        _fleeDistances = new NativeList<float>(Allocator.Persistent);
        _transformAccessArray = new TransformAccessArray(0);
    }

    private void Update()
    {
        if (playerTransform == null || _activeEnemies.Count == 0) return;

        SyncDataForJob();
        EnsureBufferSize();

        // 1. Raycasts
        PrepareRaycasts();
        JobHandle rayHandle = RaycastCommand.ScheduleBatch(_rayCommands, _rayResults, 10);

        // 2. Mouvement
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
    }

    // --- HELPERS PRIVÉS (Organisation) ---

    private void SyncDataForJob()
    {
        // Synchronisation des vitesses (pour gérer Slow/Frenzy)
        for (int i = 0; i < _activeEnemies.Count; i++)
        {
            if (_activeEnemies[i] != null)
            {
                _moveSpeeds[i] = _activeEnemies[i].currentSpeed;
            }
        }
    }

    private void EnsureBufferSize()
    {
        int requiredSize = _activeEnemies.Count * 3;
        if (!_rayCommands.IsCreated || _currentCapacity != requiredSize / 3)
        {
            if (_rayCommands.IsCreated) _rayCommands.Dispose();
            if (_rayResults.IsCreated) _rayResults.Dispose();
            if (_previousDirections.IsCreated) _previousDirections.Dispose();

            _rayCommands = new NativeArray<RaycastCommand>(requiredSize, Allocator.Persistent);
            _rayResults = new NativeArray<RaycastHit>(requiredSize, Allocator.Persistent);
            _previousDirections = new NativeArray<float3>(requiredSize / 3, Allocator.Persistent);
            _currentCapacity = requiredSize / 3;
        }
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

    // --- API PUBLIQUE ---

    public void DebugKillAllEnemies()
    {
        for (int i = _activeEnemies.Count - 1; i >= 0; i--)
        {
            if (_activeEnemies[i] != null)
            {
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

            _moveSpeeds.Add(enemy.currentSpeed);
            _stopDistances.Add(enemy.Data.stopDistance);
            _fleeDistances.Add(enemy.Data.fleeDistance);

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
                _stopDistances[index] = _stopDistances[last];
                _fleeDistances[index] = _fleeDistances[last];
            }

            _activeEnemies.RemoveAt(last);
            _moveSpeeds.RemoveAtSwapBack(last);
            _stopDistances.RemoveAtSwapBack(last);
            _fleeDistances.RemoveAtSwapBack(last);
            _transformAccessArray.RemoveAtSwapBack(index);

            if (col != null) _colliderCache.Remove(col.GetInstanceID());
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

    // DÉLÉGATION AU TARGETING SYSTEM
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