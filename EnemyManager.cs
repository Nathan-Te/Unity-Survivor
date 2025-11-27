using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [Header("Réglages")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private bool rotateTowardsPlayer = true;

    // Gestion du Job System (Mouvement)
    private List<EnemyController> _activeEnemies = new List<EnemyController>();
    private TransformAccessArray _transformAccessArray;
    private NativeList<float> _moveSpeeds;

    // --- OPTIMISATION COLLISION (NOUVEAU) ---
    // Dictionnaire qui lie l'ID unique d'un Collider -> au Script EnemyController
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

        MoveEnemiesJob moveJob = new MoveEnemiesJob
        {
            PlayerPosition = playerTransform.position,
            DeltaTime = Time.deltaTime,
            MoveSpeeds = _moveSpeeds,
            RotateTowards = rotateTowardsPlayer
        };

        JobHandle handle = moveJob.Schedule(_transformAccessArray);
        handle.Complete();
    }

    // --- Gestion des Ennemis ---

    public void RegisterEnemy(EnemyController enemy, Collider enemyCollider)
    {
        if (!_activeEnemies.Contains(enemy))
        {
            _activeEnemies.Add(enemy);
            _transformAccessArray.Add(enemy.transform);
            _moveSpeeds.Add(enemy.currentSpeed);

            // Enregistrement dans le cache de collision
            if (enemyCollider != null)
            {
                int colliderID = enemyCollider.GetInstanceID();
                if (!_colliderCache.ContainsKey(colliderID))
                {
                    _colliderCache.Add(colliderID, enemy);
                }
            }
        }
    }

    public void UnregisterEnemy(EnemyController enemy, Collider enemyCollider)
    {
        if (_activeEnemies.Contains(enemy))
        {
            int index = _activeEnemies.IndexOf(enemy);
            _activeEnemies.RemoveAtSwapBack(index);
            _transformAccessArray.RemoveAtSwapBack(index);
            _moveSpeeds.RemoveAtSwapBack(index);

            // Nettoyage du cache
            if (enemyCollider != null)
            {
                int colliderID = enemyCollider.GetInstanceID();
                if (_colliderCache.ContainsKey(colliderID))
                {
                    _colliderCache.Remove(colliderID);
                }
            }
        }
    }

    // --- Méthode Rapide pour le Player (Lookup O(1)) ---
    public bool TryGetEnemyByCollider(Collider col, out EnemyController enemy)
    {
        // GetInstanceID est très rapide (c'est juste un int)
        return _colliderCache.TryGetValue(col.GetInstanceID(), out enemy);
    }

    private void OnDestroy()
    {
        if (_moveSpeeds.IsCreated) _moveSpeeds.Dispose();
        if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();
    }
}

[BurstCompile]
public struct MoveEnemiesJob : IJobParallelForTransform
{
    public Vector3 PlayerPosition;
    public float DeltaTime;
    [ReadOnly] public NativeList<float> MoveSpeeds;
    [ReadOnly] public bool RotateTowards;

    public void Execute(int index, TransformAccess transform)
    {
        Vector3 currentPos = transform.position;
        Vector3 direction = PlayerPosition - currentPos;
        direction.y = 0;

        float distanceSqr = direction.sqrMagnitude;
        if (distanceSqr > 0.01f)
        {
            Vector3 normDir = direction.normalized;
            transform.position += normDir * MoveSpeeds[index] * DeltaTime;

            if (RotateTowards)
                transform.rotation = Quaternion.LookRotation(normDir);
        }
    }
}