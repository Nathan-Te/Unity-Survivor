using UnityEngine;

/// <summary>
/// Singleton registry for spell prefab mappings.
/// Provides global access to Form-Effect prefab combinations.
/// </summary>
public class SpellPrefabRegistry : MonoBehaviour
{
    private static SpellPrefabRegistry _instance;
    public static SpellPrefabRegistry Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SpellPrefabRegistry>();
                if (_instance == null)
                {
                    Debug.LogWarning("SpellPrefabRegistry not found in scene. Creating default instance.");
                    GameObject go = new GameObject("SpellPrefabRegistry");
                    _instance = go.AddComponent<SpellPrefabRegistry>();
                }
            }
            return _instance;
        }
    }

    [Header("Prefab Mapping")]
    [SerializeField] private FormEffectPrefabMapping prefabMapping;
    [SerializeField] private GameObject defaultFallbackPrefab;

    public FormEffectPrefabMapping PrefabMapping => prefabMapping;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Gets the prefab for a (form, effect) combination
    /// </summary>
    public GameObject GetPrefab(SpellForm form, SpellEffect effect)
    {
        GameObject result = null;

        // Try mapping first
        if (prefabMapping != null)
        {
            result = prefabMapping.GetPrefab(form, effect);
        }

        // If mapping didn't provide a prefab, try form's default
        if (result == null && form != null)
        {
            result = form.prefab;
        }

        // Last resort: use global fallback
        if (result == null)
        {
            result = defaultFallbackPrefab;
            if (result == null)
            {
                Debug.LogError($"[SpellPrefabRegistry] No prefab found for ({form?.runeName}, {effect?.runeName}) and no fallback configured!");
            }
        }

        return result;
    }

    /// <summary>
    /// Gets the impact VFX prefab for a (form, effect) combination
    /// </summary>
    public GameObject GetImpactVfx(SpellForm form, SpellEffect effect)
    {
        if (prefabMapping == null)
            return null;

        return prefabMapping.GetImpactVfx(form, effect);
    }

    /// <summary>
    /// Gets the Smite timing configuration for a (form, effect) combination
    /// Returns (impactDelay, vfxSpawnDelay, lifetime)
    /// </summary>
    public (float impactDelay, float vfxSpawnDelay, float lifetime) GetSmiteTiming(SpellForm form, SpellEffect effect)
    {
        if (prefabMapping == null)
            return (0f, 0f, 2f);

        return prefabMapping.GetSmiteTiming(form, effect);
    }

    /// <summary>
    /// Checks if a (form, effect) combination is compatible.
    /// Compatibility is determined solely by the prefab mapping.
    /// </summary>
    public bool IsCompatible(SpellForm form, SpellEffect effect)
    {
        if (form == null || effect == null)
            return false;

        // Use the mapping as the single source of truth
        if (prefabMapping != null)
        {
            return prefabMapping.IsCompatible(form, effect);
        }

        // If no mapping configured, log warning and return false
        Debug.LogWarning("[SpellPrefabRegistry] No prefab mapping configured! Cannot validate compatibility.");
        return false;
    }
}
