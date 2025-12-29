using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Example UI component for audio volume controls.
/// Attach to a settings panel with sliders for Global/Music/SFX volumes.
/// </summary>
public class AudioSettingsUI : MonoBehaviour
{
    [Header("Volume Sliders (0-1)")]
    [SerializeField] private Slider _globalVolumeSlider;
    [SerializeField] private Slider _musicVolumeSlider;
    [SerializeField] private Slider _sfxVolumeSlider;

    [Header("Optional: Volume Text Labels")]
    [SerializeField] private TMPro.TextMeshProUGUI _globalVolumeText;
    [SerializeField] private TMPro.TextMeshProUGUI _musicVolumeText;
    [SerializeField] private TMPro.TextMeshProUGUI _sfxVolumeText;

    private void Start()
    {
        // Initialize sliders with current volume values
        if (AudioManager.Instance != null)
        {
            if (_globalVolumeSlider != null)
            {
                _globalVolumeSlider.value = AudioManager.Instance.GetGlobalVolume();
                _globalVolumeSlider.onValueChanged.AddListener(OnGlobalVolumeChanged);
            }

            if (_musicVolumeSlider != null)
            {
                _musicVolumeSlider.value = AudioManager.Instance.GetMusicVolume();
                _musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (_sfxVolumeSlider != null)
            {
                _sfxVolumeSlider.value = AudioManager.Instance.GetSFXVolume();
                _sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
        }

        UpdateVolumeLabels();
    }

    private void OnDestroy()
    {
        // Unsubscribe from slider events
        if (_globalVolumeSlider != null)
            _globalVolumeSlider.onValueChanged.RemoveListener(OnGlobalVolumeChanged);

        if (_musicVolumeSlider != null)
            _musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);

        if (_sfxVolumeSlider != null)
            _sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
    }

    private void OnGlobalVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetGlobalVolume(value);
            UpdateVolumeLabels();
        }
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
            UpdateVolumeLabels();
        }
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
            UpdateVolumeLabels();
        }
    }

    private void UpdateVolumeLabels()
    {
        if (AudioManager.Instance == null) return;

        if (_globalVolumeText != null)
        {
            int percentage = Mathf.RoundToInt(AudioManager.Instance.GetGlobalVolume() * 100);
            _globalVolumeText.text = $"{percentage}%";
        }

        if (_musicVolumeText != null)
        {
            int percentage = Mathf.RoundToInt(AudioManager.Instance.GetMusicVolume() * 100);
            _musicVolumeText.text = $"{percentage}%";
        }

        if (_sfxVolumeText != null)
        {
            int percentage = Mathf.RoundToInt(AudioManager.Instance.GetSFXVolume() * 100);
            _sfxVolumeText.text = $"{percentage}%";
        }
    }
}
