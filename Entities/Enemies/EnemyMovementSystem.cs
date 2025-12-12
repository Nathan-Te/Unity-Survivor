using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

/// <summary>
/// Handles enemy movement using Unity Jobs and Burst compilation for performance.
/// Manages obstacle avoidance through raycasting and steering behaviors.
/// </summary>
public class EnemyMovementSystem : MonoBehaviour
{
    private Transform playerTransform;
    private LayerMask obstacleLayer;
    private float separationWeight = 1.5f;
    private float rayDistance = 5.0f;
    private float rayAngle = 45f;
    private float rotationSpeed = 8f;
    private float avoidanceBlendSpeed = 3f;
    private int maxEnemiesCapacity = 2000;

    // Native collections for Job system
    private TransformAccessArray _transformAccessArray;
    private NativeList<float> _moveSpeeds;
    private NativeList<float> _stopDistances;
    private NativeList<float> _fleeDistances;

    // Raycast buffers
    private NativeArray<RaycastCommand> _rayCommands;
    private NativeArray<RaycastHit> _rayResults;
    private NativeArray<float3> _previousDirections;

    private bool _isInitialized = false;
    private bool _isDisposed = false;

    public void Initialize(
        Transform player,
        LayerMask obstacles,
        int capacity,
        float sepWeight,
        float rayDist,
        float angle,
        float rotSpeed,
        float avoidBlend)
    {
        if (_isInitialized) return;

        playerTransform = player;
        obstacleLayer = obstacles;
        maxEnemiesCapacity = capacity;
        separationWeight = sepWeight;
        rayDistance = rayDist;
        rayAngle = angle;
        rotationSpeed = rotSpeed;
        avoidanceBlendSpeed = avoidBlend;

        _moveSpeeds = new NativeList<float>(maxEnemiesCapacity, Allocator.Persistent);
        _stopDistances = new NativeList<float>(maxEnemiesCapacity, Allocator.Persistent);
        _fleeDistances = new NativeList<float>(maxEnemiesCapacity, Allocator.Persistent);

        _transformAccessArray = new TransformAccessArray(maxEnemiesCapacity);

        int rayCapacity = maxEnemiesCapacity * 3;
        _rayCommands = new NativeArray<RaycastCommand>(rayCapacity, Allocator.Persistent);
        _rayResults = new NativeArray<RaycastHit>(rayCapacity, Allocator.Persistent);
        _previousDirections = new NativeArray<float3>(maxEnemiesCapacity, Allocator.Persistent);

        _isInitialized = true;
    }

    public void Cleanup()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        JobHandle.ScheduleBatchedJobs();

        if (_moveSpeeds.IsCreated) _moveSpeeds.Dispose();
        if (_stopDistances.IsCreated) _stopDistances.Dispose();
        if (_fleeDistances.IsCreated) _fleeDistances.Dispose();
        if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();
        if (_rayCommands.IsCreated) _rayCommands.Dispose();
        if (_rayResults.IsCreated) _rayResults.Dispose();
        if (_previousDirections.IsCreated) _previousDirections.Dispose();

        Debug.Log("[EnemyMovementSystem] Native Collections libérées");
    }

    /// <summary>
    /// Updates movement for all active enemies using Jobs
    /// </summary>
    public void UpdateMovement(List<EnemyController> activeEnemies)
    {
        if (playerTransform == null || activeEnemies.Count == 0) return;

        SyncDataForJob(activeEnemies);
        PrepareRaycasts(activeEnemies);

        // Schedule raycasts
        int activeRayCount = activeEnemies.Count * 3;
        if (activeRayCount > _rayCommands.Length) activeRayCount = _rayCommands.Length;

        NativeArray<RaycastCommand> cmdSlice = _rayCommands.GetSubArray(0, activeRayCount);
        NativeArray<RaycastHit> resSlice = _rayResults.GetSubArray(0, activeRayCount);

        JobHandle rayHandle = RaycastCommand.ScheduleBatch(cmdSlice, resSlice, 10);

        // Schedule movement job
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

    /// <summary>
    /// Syncs current enemy speeds to job data
    /// </summary>
    private void SyncDataForJob(List<EnemyController> activeEnemies)
    {
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            if (activeEnemies[i] != null)
                _moveSpeeds[i] = activeEnemies[i].currentSpeed;
        }
    }

    /// <summary>
    /// Prepares raycast commands for obstacle detection
    /// </summary>
    private void PrepareRaycasts(List<EnemyController> activeEnemies)
    {
        QueryParameters queryParams = new QueryParameters
        {
            layerMask = obstacleLayer,
            hitTriggers = QueryTriggerInteraction.Ignore
        };

        int count = Mathf.Min(activeEnemies.Count, maxEnemiesCapacity);

        for (int i = 0; i < count; i++)
        {
            EnemyController enemy = activeEnemies[i];
            if (enemy == null) continue;

            int baseIndex = i * 3;
            Vector3 fwd = enemy.transform.forward;
            Vector3 pos = enemy.transform.position + Vector3.up * 1.0f;

            _rayCommands[baseIndex] = new RaycastCommand(pos, fwd, queryParams, rayDistance);
            _rayCommands[baseIndex + 1] = new RaycastCommand(pos, Quaternion.Euler(0, -rayAngle, 0) * fwd, queryParams, rayDistance);
            _rayCommands[baseIndex + 2] = new RaycastCommand(pos, Quaternion.Euler(0, rayAngle, 0) * fwd, queryParams, rayDistance);
        }
    }

    public void AddEnemyToMovement(Transform enemyTransform, float speed, float stopDistance, float fleeDistance)
    {
        _transformAccessArray.Add(enemyTransform);
        _moveSpeeds.Add(speed);
        _stopDistances.Add(stopDistance);
        _fleeDistances.Add(fleeDistance);
    }

    public void RemoveEnemyFromMovement(int index, int lastIndex)
    {
        // Swap-back removal for performance
        if (index != lastIndex)
        {
            _moveSpeeds[index] = _moveSpeeds[lastIndex];
            _stopDistances[index] = _stopDistances[lastIndex];
            _fleeDistances[index] = _fleeDistances[lastIndex];
            _previousDirections[index] = _previousDirections[lastIndex];
            _previousDirections[lastIndex] = float3.zero;
        }
        else
        {
            _previousDirections[lastIndex] = float3.zero;
        }

        _moveSpeeds.RemoveAtSwapBack(lastIndex);
        _stopDistances.RemoveAtSwapBack(lastIndex);
        _fleeDistances.RemoveAtSwapBack(lastIndex);
        _transformAccessArray.RemoveAtSwapBack(index);
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    private void OnApplicationQuit()
    {
        Cleanup();
    }
}

/// <summary>
/// Burst-compiled job for parallel enemy movement
/// </summary>
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

        // Flee behavior
        if (fleeDist > 0 && distanceToPlayerSqr < (fleeDist * fleeDist))
        {
            behaviorDir = -dirToPlayer;
        }
        // Stop behavior
        else if (stopDist > 0 && distanceToPlayerSqr < (stopDist * stopDist))
        {
            behaviorDir = float3.zero;
            currentSpeedMultiplier = 0f;
        }

        float3 finalDirection = behaviorDir;

        // Obstacle avoidance
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

        // Smooth direction changes
        if (math.lengthsq(PreviousDirections[index]) > 0.01f)
        {
            finalDirection = math.normalize(math.lerp(PreviousDirections[index], finalDirection, AvoidanceBlendSpeed * DeltaTime));
        }
        PreviousDirections[index] = finalDirection;

        // Apply movement
        transform.position += (Vector3)(finalDirection * MoveSpeeds[index] * currentSpeedMultiplier * DeltaTime);

        // Apply rotation
        if (math.lengthsq(finalDirection) > 0.01f)
        {
            quaternion targetRot = quaternion.LookRotation(finalDirection, new float3(0, 1, 0));
            transform.rotation = math.slerp(transform.rotation, targetRot, RotationSpeed * DeltaTime);
        }
    }
}
