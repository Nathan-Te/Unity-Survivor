using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Nécessaire pour le tri (OrderBy)

public class MinimapIndicatorManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private RectTransform minimapRect;
    [SerializeField] private GameObject arrowPrefab;

    [Header("Réglages Visuels")]
    [SerializeField] private float radius = 100f;       // Distance des flèches par rapport au centre (en pixels)
    [SerializeField] private float iconScale = 0.8f;

    [Header("Logique")]
    [Tooltip("Rayon du cercle de visibilité (0.5 = Bord exact). Réduire un peu (ex: 0.45) pour créer un chevauchement fluide.")]
    [SerializeField] private float visibleRadiusThreshold = 0.45f;

    [Tooltip("Nombre maximum de flèches affichées simultanément")]
    [SerializeField] private int maxIndicators = 3;

    private Dictionary<PointOfInterest, GameObject> _indicators = new Dictionary<PointOfInterest, GameObject>();
    private Transform _playerTransform;

    private void Start()
    {
        if (PlayerController.Instance != null)
            _playerTransform = PlayerController.Instance.transform;
    }

    private void LateUpdate()
    {
        CleanUp(); // Nettoyage des POI détruits/nulls

        if (_playerTransform == null) return;

        // 1. Identifier les candidats (POI hors du cercle visible)
        List<PointOfInterest> candidates = new List<PointOfInterest>();

        foreach (var poi in PointOfInterest.ActivePOIs)
        {
            if (poi == null) continue;

            // Calcul de la position relative au centre du viewport (-0.5 à +0.5)
            Vector3 viewportPos = minimapCamera.WorldToViewportPoint(poi.transform.position);
            Vector2 posFromCenter = new Vector2(viewportPos.x - 0.5f, viewportPos.y - 0.5f);

            // LA CORRECTION EST ICI : On vérifie la DISTANCE (Cercle) et non les coord X/Y (Carré)
            // Si la distance est > au seuil, c'est que l'icône est masquée par le rond ou hors champ
            if (posFromCenter.magnitude > visibleRadiusThreshold)
            {
                candidates.Add(poi);
            }
            else
            {
                // Le POI est bien visible dans le cercle, on retire sa flèche
                RemoveIndicator(poi);
            }
        }

        // 2. Trier et Limiter (Logique "Les plus proches")
        // On trie les candidats par distance réelle au joueur
        var priorityList = candidates
            .OrderBy(p => Vector3.SqrMagnitude(p.transform.position - _playerTransform.position))
            .Take(maxIndicators) // On ne garde que les N premiers
            .ToList();

        // 3. Mise à jour de l'affichage

        // A. Supprimer les flèches des candidats non retenus (trop loin ou devenus visibles)
        List<PointOfInterest> toRemove = new List<PointOfInterest>();
        foreach (var kvp in _indicators)
        {
            if (!priorityList.Contains(kvp.Key)) toRemove.Add(kvp.Key);
        }
        foreach (var poi in toRemove) RemoveIndicator(poi);

        // B. Afficher/Mettre à jour les flèches des élus
        foreach (var poi in priorityList)
        {
            UpdateIndicatorPosition(poi);
        }
    }

    private void UpdateIndicatorPosition(PointOfInterest poi)
    {
        // Création si n'existe pas
        if (!_indicators.ContainsKey(poi))
        {
            GameObject arrow = Instantiate(arrowPrefab, minimapRect.transform);
            arrow.transform.localScale = Vector3.one * iconScale;
            _indicators[poi] = arrow;
        }

        GameObject arrowObj = _indicators[poi];
        RectTransform arrowRect = arrowObj.GetComponent<RectTransform>();

        // Calcul position
        Vector3 viewportPos = minimapCamera.WorldToViewportPoint(poi.transform.position);
        Vector2 posFromCenter = new Vector2(viewportPos.x - 0.5f, viewportPos.y - 0.5f);

        // Direction
        Vector2 dir = posFromCenter.normalized;

        // On colle la flèche sur le cercle défini par 'radius'
        arrowRect.anchoredPosition = dir * radius;

        // Rotation
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        arrowRect.localRotation = Quaternion.Euler(0, 0, angle - 90);
    }

    private void RemoveIndicator(PointOfInterest poi)
    {
        if (_indicators.ContainsKey(poi))
        {
            Destroy(_indicators[poi]);
            _indicators.Remove(poi);
        }
    }

    private void CleanUp()
    {
        // Supprime les entrées du dictionnaire dont les objets Unity sont détruits
        List<PointOfInterest> deadKeys = new List<PointOfInterest>();
        foreach (var kvp in _indicators)
        {
            if (kvp.Key == null || kvp.Value == null) deadKeys.Add(kvp.Key);
        }
        foreach (var k in deadKeys)
        {
            if (_indicators.ContainsKey(k) && _indicators[k] != null) Destroy(_indicators[k]);
            _indicators.Remove(k);
        }
    }
}