using System.Collections.Generic;
using UnityEngine;

public static class TargetingUtils
{
    // Vérifie la ligne de vue (Line of Sight)
    public static bool IsVisible(Vector3 start, Vector3 end, LayerMask obstacleLayer)
    {
        // On vise le torse (hauteur approximative) pour éviter le sol
        Vector3 targetPoint = new Vector3(end.x, start.y, end.z);
        Vector3 dir = targetPoint - start;
        float dist = dir.magnitude;

        return !Physics.Raycast(start, dir.normalized, dist, obstacleLayer);
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
        // Réflexe de Survie : Si un ennemi est trop près (< 4m), on l'abat en priorité
        Transform panicTarget = GetNearestEnemy(enemies, sourcePos, 4.0f, checkVisibility, obstacleLayer);
        if (panicTarget != null) return panicTarget;

        Transform bestTarget = null;
        int maxNeighbors = -1;
        float rangeSqr = range * range;
        float areaSqr = areaSize * areaSize;

        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyController candidate = enemies[i];
            if (candidate == null) continue;
            if ((candidate.transform.position - sourcePos).sqrMagnitude > rangeSqr) continue;
            if (checkVisibility && !IsVisible(sourcePos, candidate.transform.position, obstacleLayer)) continue;

            int neighborCount = 0;
            for (int j = 0; j < enemies.Count; j++)
            {
                if (i == j) continue;
                if ((enemies[j].transform.position - candidate.transform.position).sqrMagnitude <= areaSqr)
                    neighborCount++;
            }

            if (neighborCount > maxNeighbors)
            {
                maxNeighbors = neighborCount;
                bestTarget = candidate.transform;
            }
        }
        return bestTarget != null ? bestTarget : GetNearestEnemy(enemies, sourcePos, range, checkVisibility, obstacleLayer);
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