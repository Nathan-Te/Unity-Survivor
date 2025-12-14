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
    /// Checks if a (form, effect) combination is compatible
    /// </summary>
    public bool IsCompatible(SpellForm form, SpellEffect effect)
    {
        // First check tag-based compatibility
        if (!CompatibilityValidator.IsCompatible(form, effect))
            return false;

        // Then check prefab mapping compatibility if configured
        if (prefabMapping != null)
        {
            return prefabMapping.IsCompatible(form, effect);
        }

        // If no mapping, fall back to checking if form has a prefab
        return form != null && form.prefab != null;
    }
}
