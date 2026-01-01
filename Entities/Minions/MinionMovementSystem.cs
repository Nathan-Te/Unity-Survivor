using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

/// <summary>
/// Handles minion movement using Unity Jobs and Burst compilation for performance.
/// Minions follow the player and spread around them at a certain distance.
/// </summary>
public class MinionMovementSystem : MonoBehaviour
{
    private Transform playerTransform;
    private LayerMask obstacleLayer;
    private float rotationSpeed = 8f;
    private float avoidanceBlendSpeed = 3f;
    private int maxMinionsCapacity = 50;

    // Native collections for Job system
    private TransformAccessArray _transformAccessArray;
    private NativeList<float> _moveSpeeds;
    private NativeList<float> _followDistances;

    // Raycast buffers for obstacle avoidance
    private NativeArray<RaycastCommand> _rayCommands;
    private NativeArray<RaycastHit> _rayResults;
    private NativeArray<float3> _previousDirections;

    private bool _isInitialized = false;
    private bool _isDisposed = false;

    public void Initialize(Transform player, LayerMask obstacles, int capacity, float rotSpeed, float avoidBlend)
    {
        if (_isInitialized) return;

        playerTransform = player;
        obstacleLayer = obstacles;
        maxMinionsCapacity = capacity;
        rotationSpeed = rotSpeed;
        avoidanceBlendSpeed = avoidBlend;

        _moveSpeeds = new NativeList<float>(maxMinionsCapacity, Allocator.Persistent);
        _followDistances = new NativeList<float>(maxMinionsCapacity, Allocator.Persistent);

        _transformAccessArray = new TransformAccessArray(maxMinionsCapacity);

        int rayCapacity = maxMinionsCapacity * 3;
        _rayCommands = new NativeArray<RaycastCommand>(rayCapacity, Allocator.Persistent);
        _rayResults = new NativeArray<RaycastHit>(rayCapacity, Allocator.Persistent);
        _previousDirections = new NativeArray<float3>(maxMinionsCapacity, Allocator.Persistent);

        _isInitialized = true;
    }

    public void Cleanup()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        JobHandle.ScheduleBatchedJobs();

        if (_moveSpeeds.IsCreated) _moveSpeeds.Dispose();
        if (_followDistances.IsCreated) _followDistances.Dispose();
        if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();
        if (_rayCommands.IsCreated) _rayCommands.Dispose();
        if (_rayResults.IsCreated) _rayResults.Dispose();
        if (_previousDirections.IsCreated) _previousDirections.Dispose();

        Debug.Log("[MinionMovementSystem] Native Collections libérées");
    }

    /// <summary>
    /// Updates movement for all active minions using Jobs
    /// </summary>
    public void UpdateMovement(List<MinionController> activeMinions)
    {
        if (playerTransform == null || activeMinions.Count == 0) return;

        SyncDataForJob(activeMinions);
        PrepareRaycasts(activeMinions);

        // Schedule raycasts
        int activeRayCount = activeMinions.Count * 3;
        if (activeRayCount > _rayCommands.Length) activeRayCount = _rayCommands.Length;

        NativeArray<RaycastCommand> cmdSlice = _rayCommands.GetSubArray(0, activeRayCount);
        NativeArray<RaycastHit> resSlice = _rayResults.GetSubArray(0, activeRayCount);

        JobHandle rayHandle = RaycastCommand.ScheduleBatch(cmdSlice, resSlice, 10);

        // Schedule movement job
        MoveMinionsJob moveJob = new MoveMinionsJob
        {
            PlayerPosition = playerTransform.position,
            DeltaTime = Time.deltaTime,
            MoveSpeeds = _moveSpeeds.AsArray(),
            FollowDistances = _followDistances.AsArray(),
            RayResults = _rayResults,
            PreviousDirections = _previousDirections,
            RotationSpeed = rotationSpeed,
            AvoidanceBlendSpeed = avoidanceBlendSpeed
        };

        JobHandle moveHandle = moveJob.Schedule(_transformAccessArray, rayHandle);
        moveHandle.Complete();

        Physics.SyncTransforms();
    }

    /// <summary>
    /// Syncs current minion speeds to job data
    /// </summary>
    private void SyncDataForJob(List<MinionController> activeMinions)
    {
        for (int i = 0; i < activeMinions.Count; i++)
        {
            if (activeMinions[i] != null)
                _moveSpeeds[i] = activeMinions[i].currentSpeed;
        }
    }

    /// <summary>
    /// Prepares raycast commands for obstacle detection
    /// </summary>
    private void PrepareRaycasts(List<MinionController> activeMinions)
    {
        QueryParameters queryParams = new QueryParameters
        {
            layerMask = obstacleLayer,
            hitTriggers = QueryTriggerInteraction.Ignore
        };

        int count = Mathf.Min(activeMinions.Count, maxMinionsCapacity);

        for (int i = 0; i < count; i++)
        {
            MinionController minion = activeMinions[i];
            if (minion == null) continue;

            int baseIndex = i * 3;
            Vector3 fwd = minion.transform.forward;
            Vector3 pos = minion.transform.position + Vector3.up * 1.0f;

            _rayCommands[baseIndex] = new RaycastCommand(pos, fwd, queryParams, 5f);
            _rayCommands[baseIndex + 1] = new RaycastCommand(pos, Quaternion.Euler(0, -45f, 0) * fwd, queryParams, 5f);
            _rayCommands[baseIndex + 2] = new RaycastCommand(pos, Quaternion.Euler(0, 45f, 0) * fwd, queryParams, 5f);
        }
    }

    public void AddMinionToMovement(Transform minionTransform, float speed, float followDistance)
    {
        _transformAccessArray.Add(minionTransform);
        _moveSpeeds.Add(speed);
        _followDistances.Add(followDistance);
    }

    public void RemoveMinionFromMovement(int index, int lastIndex)
    {
        // Swap-back removal for performance
        if (index != lastIndex)
        {
            _moveSpeeds[index] = _moveSpeeds[lastIndex];
            _followDistances[index] = _followDistances[lastIndex];
            _previousDirections[index] = _previousDirections[lastIndex];
            _previousDirections[lastIndex] = float3.zero;
        }
        else
        {
            _previousDirections[lastIndex] = float3.zero;
        }

        _moveSpeeds.RemoveAtSwapBack(lastIndex);
        _followDistances.RemoveAtSwapBack(lastIndex);
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
/// Burst-compiled job for parallel minion movement
/// Minions orbit around the player at their follow distance
/// </summary>
[BurstCompile]
public struct MoveMinionsJob : IJobParallelForTransform
{
    public float3 PlayerPosition;
    public float DeltaTime;
    public float RotationSpeed;
    public float AvoidanceBlendSpeed;

    [ReadOnly] public NativeArray<float> MoveSpeeds;
    [ReadOnly] public NativeArray<float> FollowDistances;
    [ReadOnly] public NativeArray<RaycastHit> RayResults;
    public NativeArray<float3> PreviousDirections;

    public void Execute(int index, TransformAccess transform)
    {
        float3 currentPos = transform.position;
        float3 vectorToPlayer = PlayerPosition - currentPos;
        float distanceToPlayer = math.length(vectorToPlayer);

        float3 dirToPlayer = math.normalize(vectorToPlayer);
        dirToPlayer.y = 0; // Keep movement on XZ plane

        float followDist = FollowDistances[index];

        // Calculate desired position (orbit around player at follow distance)
        float3 desiredDir = float3.zero;

        if (distanceToPlayer > followDist + 1f)
        {
            // Too far from player - move towards them
            desiredDir = dirToPlayer;
        }
        else if (distanceToPlayer < followDist - 1f)
        {
            // Too close to player - move away
            desiredDir = -dirToPlayer;
        }
        else
        {
            // Within follow range - maintain position (slight drift for natural movement)
            desiredDir = float3.zero;
        }

        float3 finalDirection = desiredDir;

        // Obstacle avoidance (same as enemies)
        if (math.lengthsq(desiredDir) > 0.01f)
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

                finalDirection = math.normalize(desiredDir + nudge * 1.5f);
            }
        }

        // Smooth direction changes
        if (math.lengthsq(PreviousDirections[index]) > 0.01f)
        {
            finalDirection = math.normalize(math.lerp(PreviousDirections[index], finalDirection, AvoidanceBlendSpeed * DeltaTime));
        }
        PreviousDirections[index] = finalDirection;

        // Apply movement
        transform.position += (Vector3)(finalDirection * MoveSpeeds[index] * DeltaTime);

        // Apply rotation (face movement direction)
        if (math.lengthsq(finalDirection) > 0.01f)
        {
            quaternion targetRot = quaternion.LookRotation(finalDirection, new float3(0, 1, 0));
            transform.rotation = math.slerp(transform.rotation, targetRot, RotationSpeed * DeltaTime);
        }
    }
}
