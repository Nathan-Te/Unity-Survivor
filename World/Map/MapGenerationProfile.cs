using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public struct SpawnEntry
{
    public GameObject prefab;
    [Range(0f, 100f)] public float weight; // Plus c'est haut, plus c'est commun
}

[CreateAssetMenu(fileName = "NewMapProfile", menuName = "Map/Generation Profile")]
public class MapGenerationProfile : ScriptableObject
{
    [Header("Décor (Obstacles)")]
    public List<SpawnEntry> obstacleTable;
    public int minObstaclesPerChunk = 5;
    public int maxObstaclesPerChunk = 15;

    [Header("Décors (Visuels / Pass-Through)")]
    public List<SpawnEntry> decorationTable;
    public int minDecorationsPerChunk = 20;
    public int maxDecorationsPerChunk = 40;

    [Header("Points d'Intérêt (POI)")]
    public List<SpawnEntry> poiTable;

    [Tooltip("Probabilité aléatoire (0-1) d'avoir un POI sur un chunk normal")]
    [Range(0f, 1f)] public float basePoiChance = 0.1f;

    [Tooltip("Distance garantie : Force un POI tous les X chunks (Grille)")]
    public int guaranteedPoiGridSize = 3; // Exemple : 1 POI garanti dans chaque zone de 3x3

    // --- LOGIQUE DE TIRAGE PONDÉRÉ ---

    public GameObject PickRandomObstacle(System.Random rng) => PickWeighted(obstacleTable, rng);
    public GameObject PickRandomDecoration(System.Random rng) => PickWeighted(decorationTable, rng);
    public GameObject PickRandomPOI(System.Random rng) => PickWeighted(poiTable, rng);

    private GameObject PickWeighted(List<SpawnEntry> table, System.Random rng)
    {
        if (table == null || table.Count == 0) return null;

        float totalWeight = table.Sum(x => x.weight);
        double roll = rng.NextDouble() * totalWeight;

        float currentSum = 0;
        foreach (var entry in table)
        {
            currentSum += entry.weight;
            if (roll <= currentSum) return entry.prefab;
        }

        return table[0].prefab; // Fallback
    }
}