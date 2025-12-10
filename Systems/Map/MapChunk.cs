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
        // Au lieu de Destroy, on rend les objets au MapObjectPool
        if (MapObjectPool.Instance != null)
        {
            foreach (var item in _spawnedItems)
            {
                MapObjectPool.Instance.ReturnToPool(item.instance, item.sourcePrefab);
            }
        }
        else
        {
            // Fallback si pas de pool (sécurité)
            foreach (var item in _spawnedItems) Destroy(item.instance);
        }
        _spawnedItems.Clear();

        // 2. Setup RNG
        if (MapManager.Instance == null || MapManager.Instance.CurrentProfile == null) return;
        MapGenerationProfile profile = MapManager.Instance.CurrentProfile;

        int combinedSeed = coord.GetHashCode() ^ worldSeed;
        System.Random rng = new System.Random(combinedSeed);

        // 3. GÉNÉRATION OBSTACLES
        int obsCount = rng.Next(profile.minObstaclesPerChunk, profile.maxObstaclesPerChunk + 1);
        for (int i = 0; i < obsCount; i++)
        {
            GameObject prefab = profile.PickRandomObstacle(rng);
            if (prefab != null) SpawnObject(prefab, rng);
        }

        // 4. GÉNÉRATION DÉCORS (NOUVEAU)
        int decoCount = rng.Next(profile.minDecorationsPerChunk, profile.maxDecorationsPerChunk + 1);
        for (int i = 0; i < decoCount; i++)
        {
            GameObject prefab = profile.PickRandomDecoration(rng);
            if (prefab != null) SpawnObject(prefab, rng);
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
                // Pour les POI, on doit gérer la persistance AVANT de spawn
                // Si le POI est déjà détruit dans le WorldState, on ne le spawn même pas (économie)
                string uniqueID = $"Chunk_{coord.x}_{coord.y}_POI_0";

                if (WorldStateManager.Instance != null && WorldStateManager.Instance.IsInteracted(uniqueID))
                {
                    // Déjà fait, on ne spawn rien
                }
                else
                {
                    GameObject obj = SpawnObject(poiPrefab, rng);
                    if (obj.TryGetComponent<PointOfInterest>(out var poiScript))
                    {
                        poiScript.Initialize(uniqueID);
                    }
                }
            }
        }
    }

    private GameObject SpawnObject(GameObject prefab, System.Random rng)
    {
        Vector3 pos = GetRandomPositionInChunk(rng);
        Quaternion rot = Quaternion.Euler(0, (float)rng.NextDouble() * 360f, 0);

        GameObject obj;

        // Utilisation du Pool
        if (MapObjectPool.Instance != null)
        {
            obj = MapObjectPool.Instance.Get(prefab, pos, rot, obstaclesContainer);
        }
        else
        {
            obj = Instantiate(prefab, pos, rot, obstaclesContainer);
        }

        // On mémorise le couple (Instance, PrefabSource) pour pouvoir le rendre plus tard
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