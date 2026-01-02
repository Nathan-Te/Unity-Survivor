using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : Singleton<EnemyPool>
{
    // Dictionnaire : ID du Prefab -> File d'attente
    private Dictionary<int, Queue<GameObject>> _pools = new Dictionary<int, Queue<GameObject>>();

    [Header("Pool Settings")]
    [SerializeField] private int maxPoolSizePerPrefab = 50;

    [Header("Spawn Pop Effect")]
    [SerializeField] private float spawnPopDuration = 0.3f;
    [SerializeField] private AnimationCurve spawnPopCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    protected override void OnDestroy()
    {
        _pools?.Clear();

        base.OnDestroy();
    }

    // On demande un ennemi sp�cifique (prefab)
    public GameObject GetEnemy(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int key = prefab.GetInstanceID();

        if (!_pools.ContainsKey(key))
        {
            _pools.Add(key, new Queue<GameObject>());
        }

        GameObject obj;

        if (_pools[key].Count > 0)
        {
            obj = _pools[key].Dequeue();
            if (obj == null)
            {
                obj = Instantiate(prefab, position, rotation, transform);
            }
            else
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);
            }
        }
        else
        {
            // On instancie si la file est vide
            obj = Instantiate(prefab, position, rotation, transform);
        }

        // --- SPAWN POP EFFECT ---
        StartCoroutine(SpawnPopAnimation(obj));

        return obj;
    }

    private IEnumerator SpawnPopAnimation(GameObject enemy)
    {
        if (enemy == null) yield break;

        Transform enemyTransform = enemy.transform;
        Vector3 originalScale = enemyTransform.localScale;

        // Commence à scale 0
        enemyTransform.localScale = Vector3.zero;

        float elapsed = 0f;

        while (elapsed < spawnPopDuration)
        {
            if (enemy == null || !enemy.activeInHierarchy)
            {
                yield break; // Arrête si l'ennemi est détruit ou désactivé
            }

            elapsed += Time.deltaTime;
            float t = elapsed / spawnPopDuration;

            // Utilise la courbe d'animation pour plus de contrôle
            float curveValue = spawnPopCurve.Evaluate(t);

            enemyTransform.localScale = originalScale * curveValue;
            yield return null;
        }

        // S'assurer que le scale final est exact
        if (enemy != null && enemy.activeInHierarchy)
        {
            enemyTransform.localScale = originalScale;
        }
    }

    public void ReturnToPool(GameObject enemy, GameObject originalPrefab)
    {
        enemy.SetActive(false);
        int key = originalPrefab.GetInstanceID();

        if (!_pools.ContainsKey(key))
        {
            _pools.Add(key, new Queue<GameObject>());
        }

        if (_pools[key].Count >= maxPoolSizePerPrefab)
        {
            Destroy(enemy);
            return;
        }

        _pools[key].Enqueue(enemy);
    }

    /// <summary>
    /// Deactivates all active enemies (children of this pool).
    /// Called during scene transitions to clean up before reload.
    /// Does NOT destroy objects - they'll be destroyed with scene reload.
    /// </summary>
    public void ClearAll()
    {
        // Deactivate all active enemies (children of this GameObject)
        int deactivatedCount = 0;
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
                deactivatedCount++;
            }
        }

        Debug.Log($"[EnemyPool] Deactivated {deactivatedCount} active enemies. Pool has {_pools.Count} prefab types.");
    }

    public void DestroyAll()
    {
        ClearAll();

        // D�truit aussi tous les enfants de ce GameObject (ennemis actifs)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        Debug.Log("[EnemyPool] TOUS les ennemis d�truits (pool + actifs)");
    }
}