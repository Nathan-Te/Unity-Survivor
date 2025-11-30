using UnityEngine;
using Unity.Cinemachine; // <--- ATTENTION : Nouveau Namespace Unity 6

public class CameraUtils : MonoBehaviour
{
    // On référence le nouveau type
    [SerializeField] private CinemachineCamera _playerCamera;

    public void ShakeCamera(float intensity)
    {
        // Exemple d'accès au component Impulse (si ajouté)
        if (_playerCamera.TryGetComponent<CinemachineImpulseSource>(out var impulse))
        {
            impulse.GenerateImpulse(intensity);
        }
    }

    public void TeleportCamera(Vector3 position)
    {
        // Avec Cinemachine 3, pour téléporter sans lisser (cut), on utilise souvent :
        _playerCamera.OnTargetObjectWarped(
            _playerCamera.Follow,
            position - _playerCamera.Follow.position
        );
    }
}