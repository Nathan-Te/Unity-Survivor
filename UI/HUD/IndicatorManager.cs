using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class IndicatorManager : MonoBehaviour
{
    [SerializeField] private GameObject indicatorPrefab; // Une image de flèche
    [SerializeField] private float edgeBuffer = 50f; // Marge du bord de l'écran

    private Dictionary<PointOfInterest, GameObject> _indicators = new Dictionary<PointOfInterest, GameObject>();
    private Camera _cam;

    private void Start()
    {
        _cam = Camera.main;
    }

    private void LateUpdate()
    {
        // Nettoyage des indicateurs orphelins (POI détruits)
        List<PointOfInterest> toRemove = new List<PointOfInterest>();
        foreach (var kvp in _indicators)
        {
            if (kvp.Key == null || !kvp.Key.gameObject.activeInHierarchy)
                toRemove.Add(kvp.Key);
        }
        foreach (var poi in toRemove)
        {
            Destroy(_indicators[poi]);
            _indicators.Remove(poi);
        }

        // Mise à jour ou Création
        foreach (var poi in PointOfInterest.ActivePOIs)
        {
            if (poi == null) continue;

            // 1. Calculer position écran
            Vector3 screenPos = _cam.WorldToScreenPoint(poi.transform.position);
            bool isOffScreen = screenPos.x <= 0 || screenPos.x >= Screen.width ||
                               screenPos.y <= 0 || screenPos.y >= Screen.height || screenPos.z < 0;

            if (isOffScreen)
            {
                // Si pas d'indicateur, on en crée un
                if (!_indicators.ContainsKey(poi))
                {
                    GameObject ind = Instantiate(indicatorPrefab, transform);
                    // Si vous avez mis une icône dans POI, on pourrait l'assigner ici
                    _indicators[poi] = ind;
                }

                UpdateIndicatorPosition(_indicators[poi], poi.transform.position, screenPos);
            }
            else
            {
                // Si visible à l'écran, on cache l'indicateur
                if (_indicators.ContainsKey(poi))
                {
                    Destroy(_indicators[poi]);
                    _indicators.Remove(poi);
                }
            }
        }
    }

    private void UpdateIndicatorPosition(GameObject indicator, Vector3 targetWorldPos, Vector3 screenPos)
    {
        // Clamp au bord de l'écran
        Vector3 center = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);

        // Si l'objet est derrière la caméra (z < 0), on inverse la direction
        if (screenPos.z < 0) screenPos *= -1;

        Vector3 dir = (screenPos - center).normalized;

        // Calcul de la position sur le bord
        Vector2 targetPos = center + dir * 10000f; // Loin

        // Clamp manuel simple (Rectangle)
        targetPos.x = Mathf.Clamp(targetPos.x, edgeBuffer, Screen.width - edgeBuffer);
        targetPos.y = Mathf.Clamp(targetPos.y, edgeBuffer, Screen.height - edgeBuffer);

        indicator.transform.position = targetPos;

        // Rotation de la flèche
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        indicator.transform.rotation = Quaternion.Euler(0, 0, angle - 90); // -90 si votre sprite pointe vers le haut
    }
}