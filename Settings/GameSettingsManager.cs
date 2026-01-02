using UnityEngine;
using SurvivorGame.Localization;

namespace SurvivorGame.Settings
{
    /// <summary>
    /// DontDestroyOnLoad singleton that manages game settings.
    /// Loads settings on startup and applies them to the game.
    /// Supports both auto-save and manual save modes.
    /// </summary>
    public class GameSettingsManager : Singleton<GameSettingsManager>
    {
        [Header("Settings")]
        [SerializeField] private bool autoSaveOnChange = false; // Changed to false for manual save mode
        [SerializeField] private bool verboseLogging = true;

        private GameSettings _currentSettings;
        private GameSettings _lastSavedSettings; // Backup for detecting changes

        public GameSettings CurrentSettings => _currentSettings;

        /// <summary>
        /// Returns true if there are unsaved changes
        /// </summary>
        public bool HasUnsavedChanges => !AreSettingsEqual(_currentSettings, _lastSavedSettings);

        // Events for UI to listen to
        public event System.Action<GameSettings> OnSettingsLoaded;
        public event System.Action<GameSettings> OnSettingsChanged;

        protected override void Awake()
        {
            base.Awake();

            if (Instance == this)
            {
                // Ensure this GameObject is at root level for DontDestroyOnLoad
                if (transform.parent != null)
                {
                    Debug.LogWarning("[GameSettingsManager] Must be on a root GameObject. Moving to root.");
                    transform.SetParent(null);
                }

                DontDestroyOnLoad(gameObject);

                // Delay loading settings to ensure SimpleLocalizationManager is initialized
                // (SimpleLocalizationManager.Awake() might not have been called yet if we're created dynamically)
                StartCoroutine(LoadSettingsDelayed());
            }
        }

        private System.Collections.IEnumerator LoadSettingsDelayed()
        {
            // Wait for end of frame to ensure all Awake() calls are complete
            yield return new UnityEngine.WaitForEndOfFrame();

            // Load settings now that all managers are initialized
            LoadSettings();
        }

        /// <summary>
        /// Loads settings from disk and applies them
        /// </summary>
        public void LoadSettings()
        {
            _currentSettings = SettingsSaveSystem.LoadSettings();
            _lastSavedSettings = CloneSettings(_currentSettings); // Backup for change detection

            if (verboseLogging)
                Debug.Log($"[GameSettingsManager] Loaded settings: Language={_currentSettings.languageCode}, MasterVol={_currentSettings.masterVolume}");

            // Apply settings to game systems
            ApplySettings();

            OnSettingsLoaded?.Invoke(_currentSettings);
        }

        /// <summary>
        /// Saves settings to disk
        /// </summary>
        public void SaveSettings()
        {
            if (_currentSettings == null)
            {
                Debug.LogWarning("[GameSettingsManager] Cannot save null settings.");
                return;
            }

            SettingsSaveSystem.SaveSettings(_currentSettings);
            _lastSavedSettings = CloneSettings(_currentSettings); // Update backup

            if (verboseLogging)
                Debug.Log($"[GameSettingsManager] Saved settings: Language={_currentSettings.languageCode}, HasUnsavedChanges={HasUnsavedChanges}");
        }

        /// <summary>
        /// Discards unsaved changes and reverts to last saved settings
        /// </summary>
        public void DiscardChanges()
        {
            if (_lastSavedSettings == null)
            {
                Debug.LogWarning("[GameSettingsManager] No saved settings to revert to.");
                return;
            }

            if (verboseLogging)
                Debug.Log($"[GameSettingsManager] Discarding changes - Current language: {_currentSettings.languageCode}, Saved language: {_lastSavedSettings.languageCode}");

            _currentSettings = CloneSettings(_lastSavedSettings);
            ApplySettings();

            if (verboseLogging)
                Debug.Log($"[GameSettingsManager] Discarded changes, reverted to last saved settings - Language now: {_currentSettings.languageCode}, HasUnsavedChanges={HasUnsavedChanges}");

            OnSettingsChanged?.Invoke(_currentSettings);
        }

        /// <summary>
        /// Applies current settings to game systems
        /// </summary>
        private void ApplySettings()
        {
            if (_currentSettings == null)
            {
                Debug.LogWarning("[GameSettingsManager] Cannot apply null settings!");
                return;
            }

            // Apply language (force reload to ensure UI updates)
            if (SimpleLocalizationManager.Instance != null)
            {
                Language targetLanguage = _currentSettings.GetLanguageEnum();

                if (verboseLogging)
                    Debug.Log($"[GameSettingsManager] Applying language: {targetLanguage} (code: {_currentSettings.languageCode})");

                SimpleLocalizationManager.Instance.ForceSetLanguage(targetLanguage);

                if (verboseLogging)
                    Debug.Log($"[GameSettingsManager] Language applied successfully");
            }
            else
            {
                Debug.LogError("[GameSettingsManager] SimpleLocalizationManager.Instance is NULL! Cannot apply language settings. " +
                               "Make sure SimpleLocalizationManager exists in the scene and is initialized before GameSettingsManager.");
            }

            // Apply audio settings
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetGlobalVolume(_currentSettings.masterVolume);
                AudioManager.Instance.SetMusicVolume(_currentSettings.musicVolume);
                AudioManager.Instance.SetSFXVolume(_currentSettings.sfxVolume);
            }

            // Apply graphics settings
            QualitySettings.SetQualityLevel(_currentSettings.qualityLevel);
            Screen.fullScreen = _currentSettings.fullscreen;
            Application.targetFrameRate = _currentSettings.targetFrameRate;
            QualitySettings.vSyncCount = _currentSettings.vsync ? 1 : 0;

            if (verboseLogging)
                Debug.Log("[GameSettingsManager] Settings applied to game systems");
        }

        // ===== Audio Settings =====

        public void SetMasterVolume(float volume)
        {
            if (_currentSettings == null) return;
            _currentSettings.masterVolume = Mathf.Clamp01(volume);

            if (AudioManager.Instance != null)
                AudioManager.Instance.SetGlobalVolume(_currentSettings.masterVolume);

            OnSettingsChanged?.Invoke(_currentSettings);
            if (autoSaveOnChange) SaveSettings();
        }

        public void SetMusicVolume(float volume)
        {
            if (_currentSettings == null) return;
            _currentSettings.musicVolume = Mathf.Clamp01(volume);

            if (AudioManager.Instance != null)
                AudioManager.Instance.SetMusicVolume(_currentSettings.musicVolume);

            OnSettingsChanged?.Invoke(_currentSettings);
            if (autoSaveOnChange) SaveSettings();
        }

        public void SetSFXVolume(float volume)
        {
            if (_currentSettings == null) return;
            _currentSettings.sfxVolume = Mathf.Clamp01(volume);

            if (AudioManager.Instance != null)
                AudioManager.Instance.SetSFXVolume(_currentSettings.sfxVolume);

            OnSettingsChanged?.Invoke(_currentSettings);
            if (autoSaveOnChange) SaveSettings();
        }

        public void SetMuteAll(bool mute)
        {
            if (_currentSettings == null) return;
            _currentSettings.muteAll = mute;

            // Apply mute logic here (AudioManager needs mute support)

            OnSettingsChanged?.Invoke(_currentSettings);
            if (autoSaveOnChange) SaveSettings();
        }

        // ===== Localization =====

        public void SetLanguage(Language language)
        {
            if (_currentSettings == null) return;

            string previousLanguage = _currentSettings.languageCode;
            _currentSettings.SetLanguage(language);

            if (verboseLogging)
                Debug.Log($"[GameSettingsManager] SetLanguage called: {previousLanguage} → {_currentSettings.languageCode}");

            if (SimpleLocalizationManager.Instance != null)
                SimpleLocalizationManager.Instance.SetLanguage(language);

            OnSettingsChanged?.Invoke(_currentSettings);
            if (autoSaveOnChange) SaveSettings();

            if (verboseLogging)
                Debug.Log($"[GameSettingsManager] Language changed to: {_currentSettings.languageCode}, HasUnsavedChanges={HasUnsavedChanges}");
        }

        // ===== Graphics Settings =====

        public void SetQualityLevel(int level)
        {
            if (_currentSettings == null) return;
            _currentSettings.qualityLevel = Mathf.Clamp(level, 0, QualitySettings.names.Length - 1);
            QualitySettings.SetQualityLevel(_currentSettings.qualityLevel);

            OnSettingsChanged?.Invoke(_currentSettings);
            if (autoSaveOnChange) SaveSettings();
        }

        public void SetFullscreen(bool fullscreen)
        {
            if (_currentSettings == null) return;
            _currentSettings.fullscreen = fullscreen;
            Screen.fullScreen = fullscreen;

            OnSettingsChanged?.Invoke(_currentSettings);
            if (autoSaveOnChange) SaveSettings();
        }

        public void SetVSync(bool enabled)
        {
            if (_currentSettings == null) return;
            _currentSettings.vsync = enabled;
            QualitySettings.vSyncCount = enabled ? 1 : 0;

            OnSettingsChanged?.Invoke(_currentSettings);
            if (autoSaveOnChange) SaveSettings();
        }

        // ===== Gameplay Settings =====

        public void SetShowDamageNumbers(bool show)
        {
            if (_currentSettings == null) return;
            _currentSettings.showDamageNumbers = show;

            OnSettingsChanged?.Invoke(_currentSettings);
            if (autoSaveOnChange) SaveSettings();
        }

        public void SetScreenShake(bool enabled)
        {
            if (_currentSettings == null) return;
            _currentSettings.screenShake = enabled;

            OnSettingsChanged?.Invoke(_currentSettings);
            if (autoSaveOnChange) SaveSettings();
        }

        public void SetScreenShakeIntensity(float intensity)
        {
            if (_currentSettings == null) return;
            _currentSettings.screenShakeIntensity = Mathf.Clamp01(intensity);

            OnSettingsChanged?.Invoke(_currentSettings);
            if (autoSaveOnChange) SaveSettings();
        }

        // ===== Accessibility =====

        public void SetColorBlindMode(bool enabled)
        {
            if (_currentSettings == null) return;
            _currentSettings.colorBlindMode = enabled;

            OnSettingsChanged?.Invoke(_currentSettings);
            if (autoSaveOnChange) SaveSettings();
        }

        public void SetUIScale(float scale)
        {
            if (_currentSettings == null) return;
            _currentSettings.uiScale = Mathf.Clamp(scale, 0.5f, 2.0f);

            OnSettingsChanged?.Invoke(_currentSettings);
            if (autoSaveOnChange) SaveSettings();
        }

        /// <summary>
        /// Resets all settings to default
        /// </summary>
        public void ResetToDefault()
        {
            _currentSettings = GameSettings.CreateDefault();
            ApplySettings();
            SaveSettings();

            if (verboseLogging)
                Debug.Log("[GameSettingsManager] Settings reset to default");

            OnSettingsChanged?.Invoke(_currentSettings);
        }

        /// <summary>
        /// Creates a deep copy of GameSettings
        /// </summary>
        private GameSettings CloneSettings(GameSettings original)
        {
            if (original == null) return null;

            string json = JsonUtility.ToJson(original);
            return JsonUtility.FromJson<GameSettings>(json);
        }

        /// <summary>
        /// Compares two GameSettings for equality
        /// </summary>
        private bool AreSettingsEqual(GameSettings a, GameSettings b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null)
            {
                if (verboseLogging)
                    Debug.Log($"[GameSettingsManager] AreSettingsEqual: One is null (a={a != null}, b={b != null})");
                return false;
            }

            // Compare all fields
            bool areEqual = a.masterVolume == b.masterVolume &&
                   a.musicVolume == b.musicVolume &&
                   a.sfxVolume == b.sfxVolume &&
                   a.muteAll == b.muteAll &&
                   a.languageCode == b.languageCode &&
                   a.qualityLevel == b.qualityLevel &&
                   a.fullscreen == b.fullscreen &&
                   a.targetFrameRate == b.targetFrameRate &&
                   a.vsync == b.vsync &&
                   a.showDamageNumbers == b.showDamageNumbers &&
                   a.screenShake == b.screenShake &&
                   a.screenShakeIntensity == b.screenShakeIntensity &&
                   a.colorBlindMode == b.colorBlindMode &&
                   a.uiScale == b.uiScale;

            // Debug: Log differences if not equal
            if (!areEqual && verboseLogging)
            {
                Debug.Log($"[GameSettingsManager] Settings differ:");
                if (a.languageCode != b.languageCode)
                    Debug.Log($"  - Language: current={a.languageCode}, saved={b.languageCode}");
                if (a.masterVolume != b.masterVolume)
                    Debug.Log($"  - MasterVolume: current={a.masterVolume}, saved={b.masterVolume}");
                if (a.musicVolume != b.musicVolume)
                    Debug.Log($"  - MusicVolume: current={a.musicVolume}, saved={b.musicVolume}");
                if (a.sfxVolume != b.sfxVolume)
                    Debug.Log($"  - SFXVolume: current={a.sfxVolume}, saved={b.sfxVolume}");
                // Add more fields if needed for debugging
            }

            return areEqual;
        }
    }
}
