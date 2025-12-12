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
        // ProjectilePool
        if (ProjectilePool.Instance != null)
        {
            // Note : Vous devrez ajouter ClearAll() dans ProjectilePool
            ProjectilePool.Instance.ClearAll();
            if (verboseLogging)
                Debug.Log("[MemoryManager] ProjectilePool nettoyé");
        }

        // GemPool
        if (GemPool.Instance != null)
        {
            // Note : Vous devrez ajouter ClearAll() dans GemPool
            GemPool.Instance.ClearAll();
            if (verboseLogging)
                Debug.Log("[MemoryManager] GemPool nettoyé");
        }

        // EnemyPool
        if (EnemyPool.Instance != null)
        {
            EnemyPool.Instance.ClearAll();
        }

        // DamageTextPool
        if (DamageTextPool.Instance != null)
        {
            DamageTextPool.Instance.ClearAll();
        }
    }

    private void DestroyAllPools()
    {
        if (ProjectilePool.Instance != null)
        {
            // ProjectilePool.Instance.DestroyAll();
        }

        if (GemPool.Instance != null)
        {
            // GemPool.Instance.DestroyAll();
        }

        if (EnemyPool.Instance != null)
        {
            EnemyPool.Instance.DestroyAll();
        }

        if (DamageTextPool.Instance != null)
        {
            DamageTextPool.Instance.ClearAll();
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