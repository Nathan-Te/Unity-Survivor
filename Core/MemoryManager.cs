using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class MemoryManager : Singleton<MemoryManager>
{
    [Header("Settings")]
    [SerializeField] private bool autoCleanOnSceneChange = true;
    [SerializeField] private bool verboseLogging = true;
    [SerializeField] private bool aggressiveCleanup = true;

    protected override void Awake()
    {
        base.Awake();

        if (Instance == this)
        {
            // Ensure this GameObject is at root level for DontDestroyOnLoad
            if (transform.parent != null)
            {
                Debug.LogWarning("[MemoryManager] MemoryManager must be on a root GameObject. Moving to root.");
                transform.SetParent(null);
            }

            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }
    }

    protected override void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;

        base.OnDestroy();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (verboseLogging)
            Debug.Log($"[MemoryManager] Scène chargée : {scene.name}");
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (verboseLogging)
            Debug.Log($"[MemoryManager] Nettoyage après déchargement de : {scene.name}");

        if (autoCleanOnSceneChange)
        {
            CleanupAll();
        }
    }

    /// <summary>
    /// Nettoyage complet de tous les pools et caches
    /// </summary>
    public static void CleanupAll()
    {
        if (Instance == null) return;

        Instance.StartCleanup();
    }

    private void StartCleanup()
    {
        float startTime = Time.realtimeSinceStartup;

        // 1. Nettoyage du cache de visibilité
        TargetingUtils.ClearCache();

        // 2. Nettoyage des pools
        if (aggressiveCleanup)
            DestroyAllPools();
        else
            ClearAllPools();

        // 3. Force le Garbage Collector
        ForceGC();

        float duration = Time.realtimeSinceStartup - startTime;

        if (verboseLogging)
            Debug.Log($"[MemoryManager] Nettoyage complet terminé en {duration * 1000:F1}ms");
    }

    private void ClearAllPools()
    {
        // Use FindFirstObjectByType to check existence without triggering Singleton getter errors
        // Scene-based singletons may be destroyed during scene unload

        // ProjectilePool
        var projectilePool = FindFirstObjectByType<ProjectilePool>();
        if (projectilePool != null)
        {
            // Note : Vous devrez ajouter ClearAll() dans ProjectilePool
            projectilePool.ClearAll();
            if (verboseLogging)
                Debug.Log("[MemoryManager] ProjectilePool nettoyé");
        }

        // GemPool
        var gemPool = FindFirstObjectByType<GemPool>();
        if (gemPool != null)
        {
            // Note : Vous devrez ajouter ClearAll() dans GemPool
            gemPool.ClearAll();
            if (verboseLogging)
                Debug.Log("[MemoryManager] GemPool nettoyé");
        }

        // EnemyPool
        var enemyPool = FindFirstObjectByType<EnemyPool>();
        if (enemyPool != null)
        {
            enemyPool.ClearAll();
        }

        // DamageTextPool
        var damageTextPool = FindFirstObjectByType<DamageTextPool>();
        if (damageTextPool != null)
        {
            damageTextPool.ClearAll();
        }
    }

    private void DestroyAllPools()
    {
        // Use FindFirstObjectByType to check existence without triggering Singleton getter errors
        // Scene-based singletons are destroyed when scene unloads, so we check safely

        var projectilePool = FindFirstObjectByType<ProjectilePool>();
        if (projectilePool != null)
        {
            // ProjectilePool.Instance.DestroyAll();
        }

        var gemPool = FindFirstObjectByType<GemPool>();
        if (gemPool != null)
        {
            // GemPool.Instance.DestroyAll();
        }

        var enemyPool = FindFirstObjectByType<EnemyPool>();
        if (enemyPool != null)
        {
            enemyPool.DestroyAll();
        }

        var damageTextPool = FindFirstObjectByType<DamageTextPool>();
        if (damageTextPool != null)
        {
            damageTextPool.ClearAll();
        }

        if (verboseLogging)
            Debug.Log("[MemoryManager] Tous les pools détruits (mode agressif)");
    }

    private void ForceGC()
    {
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        System.GC.Collect();

        // Libère aussi les ressources Unity non utilisées
        Resources.UnloadUnusedAssets();

        if (verboseLogging)
            Debug.Log("[MemoryManager] GC forcé + UnloadUnusedAssets");
    }

    // ⭐ Méthode publique pour appel manuel (ex: depuis GameDirector)
    public static void ForceCleanup()
    {
        if (Instance != null)
        {
            Instance.StartCleanup();
        }
    }
}