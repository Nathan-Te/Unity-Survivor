using UnityEngine;

namespace SurvivorGame.Visuals
{
    /// <summary>
    /// Anime les ombres de nuages projetées par une Directional Light avec Cookie.
    /// Déplace la position de la lumière pour créer un effet de vent.
    /// </summary>
    public class CloudShadowScroller : MonoBehaviour
    {
        [Header("Wind Settings")]
        [Tooltip("Vitesse et direction du vent (X, Y). Ex: (2, 1) pour un vent diagonal")]
        [SerializeField] private Vector2 windSpeed = new Vector2(2f, 1f);

        [Header("Performance")]
        [Tooltip("Distance maximale avant de reset la position (évite les erreurs de flottants)")]
        [SerializeField] private float resetDistance = 1000f;

        private void Update()
        {
            // Déplacer la lumière dans le plan XZ (horizontal)
            // Le déplacement de la Directional Light déplace la projection du Cookie
            Vector3 movement = new Vector3(windSpeed.x, 0f, windSpeed.y) * Time.deltaTime;
            transform.position += movement;

            // Reset de position pour éviter les floating point errors sur de longues sessions
            if (transform.position.magnitude > resetDistance)
            {
                // Ramener à zéro (invisible car la texture du cookie boucle/seamless)
                transform.position = Vector3.zero;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Avertissement si resetDistance est trop petite
            if (resetDistance < 100f)
            {
                Debug.LogWarning($"[CloudShadowScroller] resetDistance ({resetDistance}) est très petite. Recommandé: >100");
            }
        }
#endif
    }
}
