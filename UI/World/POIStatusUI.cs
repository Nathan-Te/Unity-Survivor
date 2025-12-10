using UnityEngine;
using UnityEngine.UI;

public class POIStatusUI : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private GameObject uiContainer; // Le Canvas ou Panel parent
    [SerializeField] private Image fillImage;        // L'image "Fill" du slider

    [Header("Config")]
    [SerializeField] private bool hideWhenFull = true; // Pour les destructibles (cache la barre si 100%)
    [SerializeField] private bool showOnlyWhenActive = false; // Pour les zones (montre seulement si > 0%)

    private Transform _mainCameraTransform;
    private ZonePOI _zonePOI;
    private DestructiblePOI _destructiblePOI;

    private void Awake()
    {
        if (Camera.main != null) _mainCameraTransform = Camera.main.transform;

        // Détection automatique du type de POI sur le même objet
        _zonePOI = GetComponentInParent<ZonePOI>();
        _destructiblePOI = GetComponentInParent<DestructiblePOI>();
    }

    private void Start()
    {
        // Abonnements
        if (_zonePOI != null)
        {
            _zonePOI.OnProgressChanged += UpdateBar;
            UpdateBar(0); // Init
        }
        else if (_destructiblePOI != null)
        {
            _destructiblePOI.OnHealthChanged += UpdateBar;
            UpdateBar(1); // Init
            if (hideWhenFull) uiContainer.SetActive(false); // Cache au début pour les coffres
        }
    }

    private void OnDestroy()
    {
        if (_zonePOI != null) _zonePOI.OnProgressChanged -= UpdateBar;
        if (_destructiblePOI != null) _destructiblePOI.OnHealthChanged -= UpdateBar;
    }

    private void LateUpdate()
    {
        // Billboard : Toujours faire face à la caméra
        if (_mainCameraTransform != null && uiContainer.activeSelf)
        {
            uiContainer.transform.rotation = _mainCameraTransform.rotation;
        }
    }

    private void UpdateBar(float ratio)
    {
        if (fillImage != null) fillImage.fillAmount = ratio;

        // Gestion Visibilité
        if (_zonePOI != null)
        {
            // Pour une zone : on affiche si ça a commencé à charger
            bool shouldShow = ratio > 0f && ratio < 1f;
            if (showOnlyWhenActive) uiContainer.SetActive(shouldShow);
            else uiContainer.SetActive(ratio < 1f);
        }
        else if (_destructiblePOI != null)
        {
            // Pour un destructible : on affiche dès qu'on prend un coup
            if (hideWhenFull)
            {
                if (ratio < 1f && !uiContainer.activeSelf) uiContainer.SetActive(true);
            }
        }
    }
}