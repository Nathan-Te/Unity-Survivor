using System.Collections.Generic;
using UnityEngine;

public static class TargetingUtils
{
    // Vérifie la ligne de vue (Line of Sight)
    private static Dictionary<int, (bool visible, float timestamp)> _visibilityCache = new Dictionary<int, (bool, float)>();
    private const float CACHE_DURATION = 0.2f; // 200ms de cache

    public static bool IsVisible(Vector3 start, Vector3 end, LayerMask obstacleLayer, int targetID = 0)
    {
        // Si on a un ID, vérifier le cache
        if (targetID != 0 && _visibilityCache.TryGetValue(targetID, out var cached))
        {
            if (Time.time - cached.timestamp < CACHE_DURATION)
                return cached.visible;
        }

        Vector3 targetPoint = new Vector3(end.x, start.y, end.z);
        Vector3 dir = targetPoint - start;
        float dist = dir.magnitude;

        bool visible = !Physics.Raycast(start, dir.normalized, dist, obstacleLayer);

        if (targetID != 0)
            _visibilityCache[targetID] = (visible, Time.time);

        return visible;
    }

    public static Transform GetNearestEnemy(List<EnemyController> enemies, Vector3 sourcePos, float range, bool checkVisibility, LayerMask obstacleLayer)
    {
        EnemyController nearest = null;
        float minDistSqr = range * range;

        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyController enemy = enemies[i];
            if (enemy == null) continue;

            float distSqr = (enemy.transform.position - sourcePos).sqrMagnitude;
            if (distSqr < minDistSqr)
            {
                if (checkVisibility && !IsVisible(sourcePos, enemy.transform.position, obstacleLayer)) continue;

                minDistSqr = distSqr;
                nearest = enemy;
            }
        }
        return nearest != null ? nearest.transform : null;
    }

    public static Transform GetDensestCluster(List<EnemyController> enemies, Vector3 sourcePos, float range, float areaSize, bool checkVisibility, LayerMask obstacleLayer)
    {
        Transform panicTarget = GetNearestEnemy(enemies, sourcePos, 4.0f, checkVisibility, obstacleLayer);
        if (panicTarget != null) return panicTarget;

        Transform bestTarget = null;
        int maxNeighbors = -1;
        float rangeSqr = range * range;
        float areaSqr = areaSize * areaSize;

        // OPTIMISATION : Spatial Hashing ou Grid-based approach
        // Au lieu de O(n²), utiliser une grille spatiale
        Dictionary<Vector2Int, List<EnemyController>> grid = new Dictionary<Vector2Int, List<EnemyController>>();
        int cellSize = Mathf.CeilToInt(areaSize);

        // Phase 1 : Remplir la grille (O(n))
        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyController enemy = enemies[i];
            if (enemy == null) continue;

            Vector3 pos = enemy.transform.position;
            if ((pos - sourcePos).sqrMagnitude > rangeSqr) continue;

            Vector2Int cell = new Vector2Int(
                Mathf.FloorToInt(pos.x / cellSize),
                Mathf.FloorToInt(pos.z / cellSize)
            );

            if (!grid.ContainsKey(cell)) grid[cell] = new List<EnemyController>();
            grid[cell].Add(enemy);
        }

        // Phase 2 : Compter les voisins uniquement dans les cellules adjacentes (O(n))
        foreach (var kvp in grid)
        {
            Vector2Int cell = kvp.Key;

            foreach (var candidate in kvp.Value)
            {
                if (checkVisibility && !IsVisible(sourcePos, candidate.transform.position, obstacleLayer))
                    continue;

                int neighborCount = 0;

                // Vérifier seulement les 9 cellules adjacentes (3x3)
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        Vector2Int neighborCell = new Vector2Int(cell.x + dx, cell.y + dz);
                        if (!grid.ContainsKey(neighborCell)) continue;

                        foreach (var neighbor in grid[neighborCell])
                        {
                            if (neighbor == candidate) continue;
                            if ((neighbor.transform.position - candidate.transform.position).sqrMagnitude <= areaSqr)
                                neighborCount++;
                        }
                    }
                }

                if (neighborCount > maxNeighbors)
                {
                    maxNeighbors = neighborCount;
                    bestTarget = candidate.transform;
                }
            }
        }

        return bestTarget;
    }

    public static Transform GetRandomEnemy(List<EnemyController> enemies, Vector3 sourcePos, float range, bool checkVisibility, LayerMask obstacleLayer)
    {
        List<Transform> candidates = new List<Transform>();
        float rangeSqr = range * range;

        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] == null) continue;
            if ((enemies[i].transform.position - sourcePos).sqrMagnitude <= rangeSqr)
            {
                if (!checkVisibility || IsVisible(sourcePos, enemies[i].transform.position, obstacleLayer))
                    candidates.Add(enemies[i].transform);
            }
        }

        if (candidates.Count > 0) return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        return null;
    }
}