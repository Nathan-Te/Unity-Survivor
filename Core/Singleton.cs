using UnityEngine;

/// <summary>
/// Non-generic base class to hold truly global singleton state.
/// </summary>
public static class SingletonGlobalState
{
    private static bool _isSceneLoading = false;
    private static bool _isApplicationQuitting = false;

    public static bool IsSceneLoading
    {
        get => _isSceneLoading;
        set => _isSceneLoading = value;
    }

    public static bool IsApplicationQuitting
    {
        get => _isApplicationQuitting;
        set => _isApplicationQuitting = value;
    }
}

/// <summary>
/// Generic Singleton pattern for MonoBehaviour classes.
/// Automatically handles instance management and prevents duplicates.
/// </summary>
/// <typeparam name="T">The type of the singleton class</typeparam>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();

    /// <summary>
    /// Gets the singleton instance of this class.
    /// </summary>
    public static T Instance
    {
        get
        {
            if (SingletonGlobalState.IsApplicationQuitting || SingletonGlobalState.IsSceneLoading)
            {
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();

                    // REMOVED: Don't log error during initialization phase
                    // The error was triggering too early when checking "if (Instance == null)"
                    // Now we silently return null if instance doesn't exist yet
                }

                return _instance;
            }
        }
    }

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Override this method if you need custom Awake logic, but always call base.Awake().
    /// </summary>
    protected virtual void Awake()
    {
        // Check if existing instance is actually destroyed (Unity keeps reference even after Destroy)
        if (_instance != null && _instance.gameObject == null)
        {
            Debug.Log($"[Singleton] Previous instance of '{typeof(T)}' was destroyed. Clearing stale reference.");
            _instance = null;
        }

        if (_instance == null)
        {
            _instance = this as T;
        }
        else if (_instance != this)
        {
            Debug.LogWarning($"[Singleton] Duplicate instance of '{typeof(T)}' found on '{gameObject.name}'. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Clears the singleton instance reference. Used during scene reloads.
    /// </summary>
    public static void ClearInstance()
    {
        _instance = null;
    }

    /// <summary>
    /// Called when the MonoBehaviour is destroyed.
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>
    /// Called when the application is quitting.
    /// </summary>
    protected virtual void OnApplicationQuit()
    {
        SingletonGlobalState.IsApplicationQuitting = true;
    }
}
