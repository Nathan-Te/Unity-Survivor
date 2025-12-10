using UnityEngine;
using System.Collections.Generic;

public class MapChunk : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private GameObject groundPlane;
    [SerializeField] private Transform obstaclesContainer;

    // PLUS DE LISTES ICI ! Tout vient du Profile.

    private List<GameObject> _spawnedObjects = new List<GameObject>();
    private float _chunkSize;

    public void Initialize(float size)
    {
        _chunkSize = size;
        if (groundPlane != null)
        {
            float scale = size / 10f;
            groundPlane.transform.localScale = new Vector3(scale, 1, scale);
        }
    }

    public void Reposition(Vector3 newPosition, Vector2Int coord, int worldSeed)
    {
        transform.position = newPosition;
        GenerateContent(coord, worldSeed);
    }

    private void GenerateContent(Vector2Int coord, int worldSeed)
    {
        // 1. Récupération du Profil
        if (MapManager.Instance == null || MapManager.Instance.CurrentProfile == null) return;
        MapGenerationProfile profile = MapManager.Instance.CurrentProfile;

        // 2. Nettoyage
        foreach (var obj in _spawnedObjects) if (obj != null) Destroy(obj);
        _spawnedObjects.Clear();

        // 3. RNG Déterministe
        int combinedSeed = coord.GetHashCode() ^ worldSeed;
        System.Random rng = new System.Random(combinedSeed);

        // --- GÉNÉRATION DÉCORS (OBSTACLES) ---
        // Nombre aléatoire entre Min et Max défini dans le profil
        int count = rng.Next(profile.minObstaclesPerChunk, profile.maxObstaclesPerChunk + 1);

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = profile.PickRandomObstacle(rng);
            if (prefab != null) SpawnObject(prefab, rng);
        }

        // --- GÉNÉRATION POI (LOGIQUE INTELLIGENTE) ---
        bool spawnPoi = false;

        // A. Chance Aléatoire Pure (Ex: 10%)
        if (rng.NextDouble() < profile.basePoiChance)
        {
            spawnPoi = true;
        }

        // B. Garantie de Distance (Grid Logic)
        // Si GridSize = 3, alors tous les chunks dont les coord sont multiples de 3 auront un POI forcé.
        // Cela garantit qu'on ne parcourt jamais plus de 3 chunks sans en voir un.
        // On ajoute un offset via la seed pour que la grille ne soit pas toujours alignée sur (0,0)
        int gridOffset = worldSeed % profile.guaranteedPoiGridSize;
        if ((coord.x + gridOffset) % profile.guaranteedPoiGridSize == 0 &&
            (coord.y + gridOffset) % profile.guaranteedPoiGridSize == 0)
        {
            spawnPoi = true;
        }

        if (spawnPoi)
        {
            // Tirage Pondéré (Loot Table)
            GameObject poiPrefab = profile.PickRandomPOI(rng);

            if (poiPrefab != null)
            {
                GameObject obj = SpawnObject(poiPrefab, rng);

                // Init Persistance
                if (obj.TryGetComponent<PointOfInterest>(out var poiScript))
                {
                    string uniqueID = $"Chunk_{coord.x}_{coord.y}_POI_0";
                    poiScript.Initialize(uniqueID);
                }
            }
        }
    }

    private GameObject SpawnObject(GameObject prefab, System.Random rng)
    {
        Vector3 pos = GetRandomPositionInChunk(rng);
        Quaternion rot = Quaternion.Euler(0, (float)rng.NextDouble() * 360f, 0);

        GameObject obj = Instantiate(prefab, pos, rot, obstaclesContainer);
        _spawnedObjects.Add(obj);
        return obj;
    }

    private Vector3 GetRandomPositionInChunk(System.Random rng)
    {
        float halfSize = _chunkSize / 2f - 2f;
        float x = (float)rng.NextDouble() * _chunkSize - (_chunkSize / 2f);
        float z = (float)rng.NextDouble() * _chunkSize - (_chunkSize / 2f);
        return transform.position + new Vector3(x, 0, z);
    }
}