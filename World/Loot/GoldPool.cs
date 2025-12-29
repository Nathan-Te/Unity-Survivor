using System.Collections.Generic;
using UnityEngine;

public class GoldPool : Singleton<GoldPool>
{
    [Header("Configuration")]
    [SerializeField] private GameObject[] goldPrefabs; // Multiple prefabs (coin, bag, ingot, etc.)
    [SerializeField] private int[] goldValues; // Corresponding values for each prefab
    [SerializeField] private int maxActiveCoins = 400; // Limite dure pour éviter le crash

    // File pour le recyclage standard (une queue par prefab)
    private List<Queue<GameObject>> _inactivePools = new List<Queue<GameObject>>();

    // Liste des pièces actives pour gérer la limite (Fusion/Despawn)
    private List<GoldCoin> _activeCoins = new List<GoldCoin>();

    protected override void Awake()
    {
        base.Awake();

        // Initialize pools for each prefab type
        if (goldPrefabs != null)
        {
            for (int i = 0; i < goldPrefabs.Length; i++)
            {
                _inactivePools.Add(new Queue<GameObject>());
            }
        }
    }

    protected override void OnDestroy()
    {
        _activeCoins.Clear();
        _inactivePools.Clear();

        base.OnDestroy();
    }

    /// <summary>
    /// Spawns a gold coin with the specified value.
    /// Automatically selects the appropriate prefab based on value thresholds.
    /// </summary>
    public void Spawn(Vector3 position, int goldValue)
    {
        // Select prefab based on gold value
        int prefabIndex = GetPrefabIndexForValue(goldValue);
        SpawnSpecific(position, goldValue, prefabIndex);
    }

    /// <summary>
    /// Spawns a specific gold prefab type at the given position.
    /// </summary>
    public void SpawnSpecific(Vector3 position, int goldValue, int prefabIndex)
    {
        if (goldPrefabs == null || prefabIndex < 0 || prefabIndex >= goldPrefabs.Length)
        {
            Debug.LogError($"[GoldPool] Invalid prefab index: {prefabIndex}");
            return;
        }

        GoldCoin coinScript;

        // CAS 1 : Limite atteinte -> On recycle la plus vieille (FIFO)
        if (_activeCoins.Count >= maxActiveCoins)
        {
            coinScript = _activeCoins[0];
            _activeCoins.RemoveAt(0);
        }
        // CAS 2 : Récupération du pool inactif pour ce prefab
        else if (_inactivePools[prefabIndex].Count > 0)
        {
            GameObject obj = _inactivePools[prefabIndex].Dequeue();
            obj.SetActive(true);
            coinScript = obj.GetComponent<GoldCoin>();
        }
        // CAS 3 : Création d'une nouvelle
        else
        {
            // On instancie sous le Manager pour garder la hiérarchie propre
            GameObject newObj = Instantiate(goldPrefabs[prefabIndex], transform);
            coinScript = newObj.GetComponent<GoldCoin>();
        }

        // On applique la position ICI pour tous les cas
        coinScript.transform.position = position;

        // Initialisation et Ajout à la liste active
        coinScript.Initialize(goldValue);
        _activeCoins.Add(coinScript);
    }

    public void ReturnToPool(GameObject coinObj)
    {
        if (coinObj.TryGetComponent<GoldCoin>(out var script))
        {
            // On l'enlève de la liste active
            if (_activeCoins.Contains(script))
            {
                _activeCoins.Remove(script);
            }
        }

        coinObj.SetActive(false);

        // Find which prefab this belongs to and return to appropriate pool
        int prefabIndex = GetPrefabIndexForObject(coinObj);
        if (prefabIndex >= 0 && prefabIndex < _inactivePools.Count)
        {
            _inactivePools[prefabIndex].Enqueue(coinObj);
        }
    }

    public void ClearAll()
    {
        foreach (var coin in _activeCoins)
        {
            if (coin != null && coin.gameObject != null)
                Destroy(coin.gameObject);
        }
        _activeCoins.Clear();

        foreach (var pool in _inactivePools)
        {
            while (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                if (obj != null) Destroy(obj);
            }
        }

        Debug.Log("[GoldPool] Pool vidé");
    }

    /// <summary>
    /// Determines which prefab to use based on gold value.
    /// Returns prefab index.
    /// </summary>
    private int GetPrefabIndexForValue(int goldValue)
    {
        if (goldValues == null || goldValues.Length == 0)
            return 0;

        // Find the first prefab whose threshold is >= goldValue
        for (int i = goldValues.Length - 1; i >= 0; i--)
        {
            if (goldValue >= goldValues[i])
                return i;
        }

        return 0; // Default to first prefab (smallest value)
    }

    /// <summary>
    /// Finds which prefab index this GameObject belongs to.
    /// </summary>
    private int GetPrefabIndexForObject(GameObject obj)
    {
        if (goldPrefabs == null) return 0;

        string objName = obj.name.Replace("(Clone)", "").Trim();

        for (int i = 0; i < goldPrefabs.Length; i++)
        {
            if (goldPrefabs[i] != null && goldPrefabs[i].name == objName)
                return i;
        }

        return 0; // Default to first pool
    }
}
