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

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float avoidanceBlendSpeed = 3f;

    // Données Job System
    private List<EnemyController> _activeEnemies = new List<EnemyController>();
    private TransformAccessArray _transformAccessArray;
    private NativeList<float> _moveSpeeds;

    // Données Raycasts - Utiliser NativeArray au lieu de NativeList
    private NativeArray<RaycastCommand> _rayCommands;
    private NativeArray<RaycastHit> _rayResults;
    private int _currentCapacity = 0;

    // Stockage des directions précédentes pour le lissage
    private NativeArray<float3> _previousDirections;

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

        // --- ETAPE 1 : PRÉPARATION ---
        QueryParameters queryParams = new QueryParameters
        {
            layerMask = obstacleLayer,
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
            PreviousDirections = _previousDirections,
            AvoidanceWeight = separationWeight,
            RotationSpeed = rotationSpeed,
            AvoidanceBlendSpeed = avoidanceBlendSpeed
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
        if (_previousDirections.IsCreated) _previousDirections.Dispose();
    }
}

// --- JOB DE MOUVEMENT AMÉLIORÉ ---
[BurstCompile]
public struct MoveEnemiesJob : IJobParallelForTransform
{
    public float3 PlayerPosition;
    public float DeltaTime;
    public float AvoidanceWeight;
    public float RotationSpeed;
    public float AvoidanceBlendSpeed;

    [ReadOnly] public NativeArray<float> MoveSpeeds;
    [ReadOnly] public NativeArray<RaycastHit> RayResults;
    public NativeArray<float3> PreviousDirections;

    public void Execute(int index, TransformAccess transform)
    {
        float3 currentPos = transform.position;
        float3 dirToPlayer = math.normalize(PlayerPosition - currentPos);
        dirToPlayer.y = 0;

        int baseIndex = index * 3;
        bool hitCenter = RayResults[baseIndex].colliderInstanceID != 0;
        bool hitLeft = RayResults[baseIndex + 1].colliderInstanceID != 0;
        bool hitRight = RayResults[baseIndex + 2].colliderInstanceID != 0;

        float3 finalDirection = dirToPlayer;
        float3 right = transform.rotation * new float3(1, 0, 0);
        float3 left = transform.rotation * new float3(-1, 0, 0);

        // Calcul de l'évitement plus progressif
        if (hitCenter)
        {
            float3 avoidDirection;
            if (!hitLeft && !hitRight)
            {
                // Choix aléatoire mais cohérent basé sur la position
                avoidDirection = (index % 2 == 0) ? left : right;
            }
            else if (!hitLeft)
            {
                avoidDirection = left;
            }
            else
            {
                avoidDirection = right;
            }

            // Blend progressif entre la direction du joueur et l'évitement
            finalDirection = math.normalize(math.lerp(dirToPlayer, avoidDirection, 0.7f));
        }
        else if (hitLeft || hitRight)
        {
            float3 nudge = float3.zero;
            if (hitLeft) nudge += right * 0.5f;
            if (hitRight) nudge += left * 0.5f;
            finalDirection = math.normalize(dirToPlayer + nudge * AvoidanceWeight);
        }

        // Lissage de la direction avec la frame précédente
        if (math.lengthsq(PreviousDirections[index]) > 0.01f)
        {
            finalDirection = math.normalize(math.lerp(
                PreviousDirections[index],
                finalDirection,
                AvoidanceBlendSpeed * DeltaTime
            ));
        }

        PreviousDirections[index] = finalDirection;

        // Déplacement
        transform.position += (Vector3)(finalDirection * MoveSpeeds[index] * DeltaTime);

        // Rotation lissée
        if (math.lengthsq(finalDirection) > 0.01f)
        {
            quaternion targetRot = quaternion.LookRotation(finalDirection, new float3(0, 1, 0));
            transform.rotation = math.slerp(transform.rotation, targetRot, RotationSpeed * DeltaTime);
        }
    }
}