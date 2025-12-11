using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    [Header("Cible")]
    [SerializeField] private Transform playerTarget;

    [Header("Réglages")]
    [SerializeField] private float height = 50f; // Altitude de la caméra

    private void LateUpdate()
    {
        if (playerTarget == null)
        {
            if (PlayerController.Instance != null)
                playerTarget = PlayerController.Instance.transform;
            else
                return;
        }

        // 1. Suivre la Position (X, Z)
        Vector3 newPos = playerTarget.position;
        newPos.y = height; // On fixe la hauteur
        transform.position = newPos;

        // 2. Fixer la Rotation (Regarder vers le bas, Nord en haut)
        // 90° sur X pour regarder le sol. 0° sur Y pour que le haut soit le Nord (Z+).
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}