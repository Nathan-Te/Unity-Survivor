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
    [SerializeField] private float separationWeight = 8.0f;
    [SerializeField] private float rayDistance = 5.0f;
    [SerializeField] private float rayAngle = 45f;

    // Données Job System
    private List<EnemyController> _activeEnemies = new List<EnemyController>();
    private TransformAccessArray _transformAccessArray;
    private NativeList<float> _moveSpeeds;

    // Données Raycasts - Utiliser NativeArray au lieu de NativeList
    private NativeArray<RaycastCommand> _rayCommands;
    private NativeArray<RaycastHit> _rayResults;
    private int _currentCapacity = 0;

    private Dictionary<int, EnemyController> _colliderCache = new Dictionary<int, EnemyController>();

    private void Awake()
    {
        Instance = this;
        _moveSpeeds = new NativeList<float>(Allocator.Persistent);
        _transformAccessArray = new TransformAccessArray(0);
    }

    private void Update()
    {
        if (playerTransform == null || _activeEnemies.Count == 0) return;

        int requiredSize = _activeEnemies.Count * 3;

        // Redimensionner les arrays si nécessaire
        if (!_rayCommands.IsCreated || _currentCapacity != requiredSize)
        {
            if (_rayCommands.IsCreated) _rayCommands.Dispose();
            if (_rayResults.IsCreated) _rayResults.Dispose();

            _rayCommands = new NativeArray<RaycastCommand>(requiredSize, Allocator.Persistent);
            _rayResults = new NativeArray<RaycastHit>(requiredSize, Allocator.Persistent);
            _currentCapacity = requiredSize;
        }

        // --- ETAPE 1 : PRÉPARATION ---
        QueryParameters queryParams = new QueryParameters
        {
            layerMask = -1,
            hitTriggers = QueryTriggerInteraction.Ignore,
            hitBackfaces = false
        };

        for (int i = 0; i < _activeEnemies.Count; i++)
        {
            EnemyController enemy = _activeEnemies[i];
            if (enemy == null) continue;

            int baseIndex = i * 3;

            Vector3 forward = enemy.transform.forward;
            Vector3 pos = enemy.transform.position + Vector3.up * 1.0f;

            // Commande 1 : Centre
            _rayCommands[baseIndex] = new RaycastCommand(pos, forward, queryParams, rayDistance);

            // Commande 2 : Gauche
            Vector3 leftDir = Quaternion.Euler(0, -rayAngle, 0) * forward;
            _rayCommands[baseIndex + 1] = new RaycastCommand(pos, leftDir, queryParams, rayDistance);

            // Commande 3 : Droite
            Vector3 rightDir = Quaternion.Euler(0, rayAngle, 0) * forward;
            _rayCommands[baseIndex + 2] = new RaycastCommand(pos, rightDir, queryParams, rayDistance);
        }

        // --- ETAPE 2 : EXÉCUTION PARALLÈLE ---
        JobHandle raycastHandle = RaycastCommand.ScheduleBatch(
            _rayCommands,
            _rayResults,
            10
        );

        // --- ETAPE 3 : MOUVEMENT ---
        MoveEnemiesJob moveJob = new MoveEnemiesJob
        {
            PlayerPosition = playerTransform.position,
            DeltaTime = Time.deltaTime,
            MoveSpeeds = _moveSpeeds.AsArray(),
            RayResults = _rayResults,
            AvoidanceWeight = separationWeight
        };

        JobHandle moveHandle = moveJob.Schedule(_transformAccessArray, raycastHandle);
        moveHandle.Complete();
    }

    // --- VISUALISATION ---
    private void OnDrawGizmos()
    {
        if (_activeEnemies == null || _activeEnemies.Count == 0) return;
        if (!_rayResults.IsCreated || _rayResults.Length == 0) return;

        EnemyController enemy = _activeEnemies[0];
        if (enemy == null) return;

        int baseIndex = 0;
        bool hitCenter = _rayResults[baseIndex].colliderInstanceID != 0;
        bool hitLeft = _rayResults[baseIndex + 1].colliderInstanceID != 0;
        bool hitRight = _rayResults[baseIndex + 2].colliderInstanceID != 0;

        Vector3 pos = enemy.transform.position + Vector3.up * 1.0f;
        Vector3 forward = enemy.transform.forward;

        Gizmos.color = hitCenter ? Color.red : Color.green;
        Gizmos.DrawLine(pos, pos + forward * rayDistance);

        Vector3 leftDir = Quaternion.Euler(0, -rayAngle, 0) * forward;
        Gizmos.color = hitLeft ? Color.red : Color.green;
        Gizmos.DrawLine(pos, pos + leftDir * rayDistance);

        Vector3 rightDir = Quaternion.Euler(0, rayAngle, 0) * forward;
        Gizmos.color = hitRight ? Color.red : Color.green;
        Gizmos.DrawLine(pos, pos + rightDir * rayDistance);
    }

    // --- GESTION LISTES ---
    public void RegisterEnemy(EnemyController enemy, Collider enemyCollider)
    {
        if (!_activeEnemies.Contains(enemy))
        {
            _activeEnemies.Add(enemy);
            _transformAccessArray.Add(enemy.transform);
            _moveSpeeds.Add(enemy.currentSpeed);

            if (enemyCollider != null)
            {
                int id = enemyCollider.GetInstanceID();
                if (!_colliderCache.ContainsKey(id)) _colliderCache.Add(id, enemy);
            }
        }
    }

    public void UnregisterEnemy(EnemyController enemy, Collider enemyCollider)
    {
        if (_activeEnemies.Contains(enemy))
        {
            int index = _activeEnemies.IndexOf(enemy);
            int lastIndex = _activeEnemies.Count - 1;

            if (index != lastIndex)
            {
                _activeEnemies[index] = _activeEnemies[lastIndex];
                _moveSpeeds[index] = _moveSpeeds[lastIndex];
            }

            _activeEnemies.RemoveAt(lastIndex);
            _moveSpeeds.RemoveAtSwapBack(lastIndex);
            _transformAccessArray.RemoveAtSwapBack(index);

            if (enemyCollider != null)
            {
                int id = enemyCollider.GetInstanceID();
                if (_colliderCache.ContainsKey(id)) _colliderCache.Remove(id);
            }
        }
    }

    public bool TryGetEnemyByCollider(Collider col, out EnemyController enemy)
    {
        return _colliderCache.TryGetValue(col.GetInstanceID(), out enemy);
    }

    private void OnDestroy()
    {
        if (_moveSpeeds.IsCreated) _moveSpeeds.Dispose();
        if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();
        if (_rayCommands.IsCreated) _rayCommands.Dispose();
        if (_rayResults.IsCreated) _rayResults.Dispose();
    }
}

// --- JOB DE MOUVEMENT ---
[BurstCompile]
public struct MoveEnemiesJob : IJobParallelForTransform
{
    public Vector3 PlayerPosition;
    public float DeltaTime;
    public float AvoidanceWeight;

    [ReadOnly] public NativeArray<float> MoveSpeeds;
    [ReadOnly] public NativeArray<RaycastHit> RayResults;

    public void Execute(int index, TransformAccess transform)
    {
        Vector3 currentPos = transform.position;
        Vector3 dirToPlayer = (PlayerPosition - currentPos).normalized;
        dirToPlayer.y = 0;

        int baseIndex = index * 3;
        bool hitCenter = RayResults[baseIndex].colliderInstanceID != 0;
        bool hitLeft = RayResults[baseIndex + 1].colliderInstanceID != 0;
        bool hitRight = RayResults[baseIndex + 2].colliderInstanceID != 0;

        Vector3 finalDirection = dirToPlayer;
        Vector3 right = transform.rotation * Vector3.right;
        Vector3 left = transform.rotation * Vector3.left;

        if (hitCenter)
        {
            if (!hitLeft) finalDirection = left;
            else finalDirection = right;
            finalDirection += dirToPlayer * 0.2f;
            finalDirection = finalDirection.normalized * AvoidanceWeight;
        }
        else if (hitLeft || hitRight)
        {
            Vector3 nudge = Vector3.zero;
            if (hitLeft) nudge += right;
            if (hitRight) nudge += left;
            finalDirection += nudge.normalized * AvoidanceWeight;
        }

        finalDirection.Normalize();
        transform.position += finalDirection * MoveSpeeds[index] * DeltaTime;

        if (finalDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(finalDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 15f * DeltaTime);
        }
    }
}