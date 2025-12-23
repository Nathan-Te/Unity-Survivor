using UnityEngine;

/// <summary>
/// Helper script to automatically setup persistent managers in the scene.
/// Attach this to an empty GameObject in your scene.
/// It will create GameStateManager and MemoryManager as root GameObjects if they don't exist.
/// </summary>
[DefaultExecutionOrder(-200)]
public class PersistentManagersSetup : MonoBehaviour
{
    [Header("Auto-Setup")]
    [SerializeField] private bool autoCreateManagers = true;

    private void Awake()
    {
        if (!autoCreateManagers) return;

        // Setup GameStateManager
        if (GameStateManager.Instance == null)
        {
            GameObject go = new GameObject("GameStateManager");
            go.AddComponent<GameStateManager>();
            Debug.Log("[PersistentManagersSetup] Created GameStateManager");
        }

        // Setup MemoryManager
        if (MemoryManager.Instance == null)
        {
            GameObject go = new GameObject("MemoryManager");
            go.AddComponent<MemoryManager>();
            Debug.Log("[PersistentManagersSetup] Created MemoryManager");
        }

        // This setup script can be destroyed after creating the managers
        Destroy(gameObject);
    }
}
