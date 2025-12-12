using UnityEngine;

/// <summary>
/// Handles visual effects for enemies (hit flash, material management).
/// </summary>
[RequireComponent(typeof(EnemyController))]
public class EnemyVisuals : MonoBehaviour
{
    [Header("Visual Effects")]
    [SerializeField] private float hitFlashDuration = 0.1f;
    [SerializeField] private Material hitFlashMaterial; // Optional custom flash material

    private Renderer _renderer;
    private Material[] _originalMaterials;
    private Material[] _flashMaterialsArray;
    private float _flashTimer;

    private void Awake()
    {
        InitializeFlashEffect();
    }

    private void Update()
    {
        UpdateFlashEffect();
    }

    /// <summary>
    /// Initializes the hit flash effect materials
    /// </summary>
    private void InitializeFlashEffect()
    {
        _renderer = GetComponentInChildren<Renderer>();
        if (_renderer == null) return;

        // Save all original materials (support multi-material meshes)
        _originalMaterials = _renderer.sharedMaterials;

        // Create flash material array (one per material slot)
        _flashMaterialsArray = new Material[_originalMaterials.Length];

        // Create or use provided flash material
        Material flashMat = CreateFlashMaterial();

        // Fill all slots with the same flash material
        for (int i = 0; i < _flashMaterialsArray.Length; i++)
        {
            _flashMaterialsArray[i] = flashMat;
        }
    }

    /// <summary>
    /// Creates the flash material (white unlit)
    /// </summary>
    private Material CreateFlashMaterial()
    {
        if (hitFlashMaterial != null)
        {
            return new Material(hitFlashMaterial);
        }

        // Create default white flash material
        Shader flashShader = Shader.Find("Universal Render Pipeline/Unlit");
        if (flashShader == null) flashShader = Shader.Find("Unlit/Color"); // Built-in fallback
        if (flashShader == null) flashShader = Shader.Find("Standard"); // Standard fallback

        Material flashMat = new Material(flashShader);
        flashMat.color = Color.white;

        return flashMat;
    }

    /// <summary>
    /// Updates the flash effect timer
    /// </summary>
    private void UpdateFlashEffect()
    {
        if (_flashTimer <= 0) return;

        _flashTimer -= Time.deltaTime;

        if (_flashTimer <= 0)
        {
            RestoreOriginalMaterials();
        }
    }

    /// <summary>
    /// Triggers the hit flash effect
    /// </summary>
    public void TriggerHitFlash()
    {
        if (_renderer == null || _flashMaterialsArray == null) return;

        _flashTimer = hitFlashDuration;
        _renderer.materials = _flashMaterialsArray;
    }

    /// <summary>
    /// Restores original materials (called when enemy is pooled)
    /// </summary>
    public void RestoreOriginalMaterials()
    {
        if (_renderer == null || _originalMaterials == null) return;

        _renderer.materials = _originalMaterials;
        _flashTimer = 0f;
    }
}
