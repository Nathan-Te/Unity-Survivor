using UnityEngine;

/// <summary>
/// Simple initializer to start background music when the game starts.
/// Add this to a GameObject in the game scene (not DontDestroyOnLoad).
/// </summary>
public class AudioInitializer : MonoBehaviour
{
    [Header("Auto-Play BGM on Start")]
    [SerializeField] private bool _playBGMOnStart = true;

    private void Start()
    {
        if (_playBGMOnStart && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDefaultBGM();
        }
    }
}
