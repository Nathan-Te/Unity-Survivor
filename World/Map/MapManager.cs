using System.Collections.Generic;
using UnityEngine;

public class MapManager : Singleton<MapManager>
{
    [Header("Configuration")]
    [SerializeField] private MapGenerationProfile currentProfile; // <-- NOUVEAU
    [SerializeField] private GameObject chunkPrefab;
    [SerializeField] private float chunkSize = 40f;
    [SerializeField] private int viewDistance = 1;

    [Header("G�n�ration")]
    [SerializeField] private bool randomizeSeed = true;
    [SerializeField] private int worldSeed = 12345;

    private Transform _playerTransform;
    private Vector2Int _currentChunkCoord;
    private Dictionary<Vector2Int, MapChunk> _activeChunks = new Dictionary<Vector2Int, MapChunk>();

    // Getter pour le chunk
    public MapGenerationProfile CurrentProfile => currentProfile;

    [Header("Rendu")]
    [SerializeField] private Material groundMaterial;

    protected override void Awake()
    {
        base.Awake();

        // Always initialize (even after scene reload)
        if (randomizeSeed) worldSeed = Random.Range(-1000000, 1000000);

        if (groundMaterial != null)
        {
            // On g�n�re un d�calage �norme bas� sur la seed
            // (Les shaders aiment les Vector2 pour les offsets)
            float offsetX = Random.Range(-10000f, 10000f);
            float offsetY = Random.Range(-10000f, 10000f);

            // On envoie �a au Shader
            // Assurez-vous que le nom "Noise_Offset" correspond exactement � celui du Shader Graph
            groundMaterial.SetVector("_Noise_Offset", new Vector2(offsetX, offsetY));
            Debug.Log("Vector : " + groundMaterial.GetVector("_Noise_Offset"));
        }

        // S�curit�
        if (currentProfile == null) Debug.LogError("MapManager : Aucun Profil de g�n�ration assign� !");
    }

    // ... (Start, Update et UpdateChunks restent inchang�s) ...
    // Le code existant est bon, car il appelle chunk.Reposition(..., worldSeed)
    // Nous allons modifier MapChunk pour qu'il lise le profil via le Singleton.

    private void Start()
    {
        if (PlayerController.Instance != null)
        {
            _playerTransform = PlayerController.Instance.transform;
            UpdateChunks(true);
        }
    }

    private void Update()
    {
        if (_playerTransform == null) return;
        Vector2Int playerChunkCoord = new Vector2Int(
            Mathf.RoundToInt(_playerTransform.position.x / chunkSize),
            Mathf.RoundToInt(_playerTransform.position.z / chunkSize)
        );
        if (playerChunkCoord != _currentChunkCoord)
        {
            _currentChunkCoord = playerChunkCoord;
            UpdateChunks();
        }
    }

    private void UpdateChunks(bool forceInit = false)
    {
        List<Vector2Int> requiredCoords = new List<Vector2Int>();
        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int y = -viewDistance; y <= viewDistance; y++)
            {
                requiredCoords.Add(new Vector2Int(_currentChunkCoord.x + x, _currentChunkCoord.y + y));
            }
        }

        List<Vector2Int> coordsToRemove = new List<Vector2Int>();
        foreach (var kvp in _activeChunks)
        {
            if (!requiredCoords.Contains(kvp.Key)) coordsToRemove.Add(kvp.Key);
        }

        Queue<MapChunk> freeChunks = new Queue<MapChunk>();
        foreach (var coord in coordsToRemove)
        {
            freeChunks.Enqueue(_activeChunks[coord]);
            _activeChunks.Remove(coord);
        }

        foreach (var coord in requiredCoords)
        {
            if (!_activeChunks.ContainsKey(coord))
            {
                MapChunk chunk;
                Vector3 position = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);

                if (freeChunks.Count > 0)
                {
                    chunk = freeChunks.Dequeue();
                    chunk.Reposition(position, coord, worldSeed);
                }
                else
                {
                    GameObject obj = Instantiate(chunkPrefab, position, Quaternion.identity, transform);
                    chunk = obj.GetComponent<MapChunk>();
                    chunk.Initialize(chunkSize);
                    chunk.Reposition(position, coord, worldSeed);
                }

                _activeChunks.Add(coord, chunk);
            }
        }
    }
}