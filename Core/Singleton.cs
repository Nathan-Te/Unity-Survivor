using UnityEngine;

/// <summary>
/// Generic Singleton pattern for MonoBehaviour classes.
/// Automatically handles instance management and prevents duplicates.
/// </summary>
/// <typeparam name="T">The type of the singleton class</typeparam>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _applicationIsQuitting = false;

    /// <summary>
    /// Gets the singleton instance of this class.
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();

                    if (_instance == null)
                    {
                        Debug.LogError($"[Singleton] An instance of '{typeof(T)}' is needed but none exists in the scene.");
                    }
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
        _applicationIsQuitting = true;
    }
}
