using UnityEngine;
using System.Collections.Generic;

// Structure pour se souvenir quel objet vient de quel prefab (nécessaire pour le pooling)
public struct SpawnedItem
{
    public GameObject instance;
    public GameObject sourcePrefab;
}

public class MapChunk : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private GameObject groundPlane;
    [SerializeField] private Transform obstaclesContainer;

    // Liste des objets actuellement posés sur ce chunk
    private List<SpawnedItem> _spawnedItems = new List<SpawnedItem>();
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

        int combinedSeed = coord.GetHashCode() ^ worldSeed;
        System.Random rng = new System.Random(combinedSeed);

        if (groundPlane != null)
        {
            // On tourne de 0, 90, 180 ou 270 degrés
            int randomRot = rng.Next(0, 4) * 90;
            groundPlane.transform.localRotation = Quaternion.Euler(0, randomRot, 0);
        }

        GenerateContent(coord, worldSeed);
    }

    private void GenerateContent(Vector2Int coord, int worldSeed)
    {
        // 1. RECYCLAGE (POOLING)
        if (MapObjectPool.Instance != null)
        {
            foreach (var item in _spawnedItems)
            {
                MapObjectPool.Instance.ReturnToPool(item.instance, item.sourcePrefab);
            }
        }
        else
        {
            foreach (var item in _spawnedItems) Destroy(item.instance);
        }
        _spawnedItems.Clear();

        // 2. Setup RNG
        if (MapManager.Instance == null || MapManager.Instance.CurrentProfile == null) return;
        MapGenerationProfile profile = MapManager.Instance.CurrentProfile;

        int combinedSeed = coord.GetHashCode() ^ worldSeed;
        System.Random rng = new System.Random(combinedSeed);

        // 3. GÉNÉRATION OBSTACLES (Rotation Aléatoire = OUI)
        int obsCount = rng.Next(profile.minObstaclesPerChunk, profile.maxObstaclesPerChunk + 1);
        for (int i = 0; i < obsCount; i++)
        {
            GameObject prefab = profile.PickRandomObstacle(rng);
            // Par défaut, SpawnObject utilise randomRotation = true
            if (prefab != null) SpawnObject(prefab, rng, true);
        }

        // 4. GÉNÉRATION DÉCORS (Rotation Aléatoire = OUI)
        int decoCount = rng.Next(profile.minDecorationsPerChunk, profile.maxDecorationsPerChunk + 1);
        for (int i = 0; i < decoCount; i++)
        {
            GameObject prefab = profile.PickRandomDecoration(rng);
            if (prefab != null) SpawnObject(prefab, rng, true);
        }

        // 5. GÉNÉRATION POI
        bool spawnPoi = false;
        if (rng.NextDouble() < profile.basePoiChance) spawnPoi = true;

        int gridOffset = worldSeed % profile.guaranteedPoiGridSize;
        if ((coord.x + gridOffset) % profile.guaranteedPoiGridSize == 0 &&
            (coord.y + gridOffset) % profile.guaranteedPoiGridSize == 0)
        {
            spawnPoi = true;
        }

        if (spawnPoi)
        {
            GameObject poiPrefab = profile.PickRandomPOI(rng);
            if (poiPrefab != null)
            {
                string uniqueID = $"Chunk_{coord.x}_{coord.y}_POI_0";

                if (WorldStateManager.Instance != null && WorldStateManager.Instance.IsInteracted(uniqueID))
                {
                    // Déjà fait
                }
                else
                {
                    // --- MODIFICATION ICI : randomRotation = false ---
                    GameObject obj = SpawnObject(poiPrefab, rng, false);
                    // ------------------------------------------------

                    if (obj.TryGetComponent<PointOfInterest>(out var poiScript))
                    {
                        poiScript.Initialize(uniqueID);
                    }
                }
            }
        }
    }

    // Nouvelle signature avec paramètre par défaut à true
    private GameObject SpawnObject(GameObject prefab, System.Random rng, bool randomRotation = true)
    {
        Vector3 pos = GetRandomPositionInChunk(rng);

        // Choix de la rotation
        Quaternion rot;
        if (randomRotation)
        {
            rot = Quaternion.Euler(0, (float)rng.NextDouble() * 360f, 0);
        }
        else
        {
            rot = Quaternion.identity; // Rotation (0, 0, 0) pour les POI
        }

        GameObject obj;

        if (MapObjectPool.Instance != null)
        {
            obj = MapObjectPool.Instance.Get(prefab, pos, rot, obstaclesContainer);
        }
        else
        {
            obj = Instantiate(prefab, pos, rot, obstaclesContainer);
        }

        _spawnedItems.Add(new SpawnedItem { instance = obj, sourcePrefab = prefab });

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