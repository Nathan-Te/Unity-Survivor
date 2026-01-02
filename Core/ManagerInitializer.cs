using UnityEngine;
using SurvivorGame.Localization;

/// <summary>
/// Ensures critical managers are initialized in the correct order.
/// This script should have a Script Execution Order of -100 (very early).
/// </summary>
[DefaultExecutionOrder(-100)]
public class ManagerInitializer : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("[ManagerInitializer] Initializing critical managers...");

        // 1. SimpleLocalizationManager MUST exist first (required by UI)
        if (FindFirstObjectByType<SimpleLocalizationManager>() == null)
        {
            GameObject localizationObj = new GameObject("SimpleLocalizationManager");
            localizationObj.AddComponent<SimpleLocalizationManager>();
            Debug.Log("[ManagerInitializer] Created SimpleLocalizationManager");
        }

        Debug.Log("[ManagerInitializer] Critical managers initialized");
    }
}
