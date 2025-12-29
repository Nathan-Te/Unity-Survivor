using UnityEngine;

/// <summary>
/// Central audio management system with pooling support and volume control.
/// Handles BGM, SFX playback with Global/Music/SFX volume categories.
/// </summary>
public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource _bgmSource;

    [Header("Audio Settings")]
    [SerializeField] private GameAudioSettings _audioSettings;

    [Header("Volume Settings (0-1)")]
    [SerializeField, Range(0f, 1f)] private float _globalVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float _musicVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float _sfxVolume = 1f;

    private AudioPool _audioPool;

    // Player damage sound throttling
    private float _lastPlayerHitTime;
    private const float PLAYER_HIT_COOLDOWN = 0.3f;

    protected override void Awake()
    {
        base.Awake();

        // Always initialize (critical for restart support)
        if (_bgmSource == null)
        {
            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;
        }

        UpdateBGMVolume();
    }

    private void Start()
    {
        // Initialize audio pool
        _audioPool = AudioPool.Instance;

        // Subscribe to game state events
        if (GameStateController.Instance != null)
        {
            GameStateController.Instance.OnGamePaused.AddListener(OnGamePaused);
            GameStateController.Instance.OnGameResumed.AddListener(OnGameResumed);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (GameStateController.Instance != null)
        {
            GameStateController.Instance.OnGamePaused.RemoveListener(OnGamePaused);
            GameStateController.Instance.OnGameResumed.RemoveListener(OnGameResumed);
        }
    }

    #region Volume Control

    public void SetGlobalVolume(float volume)
    {
        _globalVolume = Mathf.Clamp01(volume);
        UpdateBGMVolume();
    }

    public void SetMusicVolume(float volume)
    {
        _musicVolume = Mathf.Clamp01(volume);
        UpdateBGMVolume();
    }

    public void SetSFXVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
    }

    public float GetGlobalVolume() => _globalVolume;
    public float GetMusicVolume() => _musicVolume;
    public float GetSFXVolume() => _sfxVolume;

    private void UpdateBGMVolume()
    {
        if (_bgmSource != null)
        {
            _bgmSource.volume = _globalVolume * _musicVolume;
        }
    }

    private float GetEffectiveSFXVolume()
    {
        return _globalVolume * _sfxVolume;
    }

    #endregion

    #region Background Music

    public void PlayBGM(AudioClip bgmClip)
    {
        if (_bgmSource == null || bgmClip == null) return;

        if (_bgmSource.clip == bgmClip && _bgmSource.isPlaying)
            return; // Already playing this track

        _bgmSource.clip = bgmClip;
        _bgmSource.Play();
    }

    public void PlayDefaultBGM()
    {
        if (_audioSettings != null)
        {
            var (clip, volume) = _audioSettings.GetDefaultBGM();
            if (clip != null)
            {
                PlayBGM(clip);
                // Apply volume multiplier to BGM
                if (_bgmSource != null)
                {
                    _bgmSource.volume = _globalVolume * _musicVolume * volume;
                }
            }
        }
    }

    public void StopBGM()
    {
        if (_bgmSource != null)
        {
            _bgmSource.Stop();
        }
    }

    public void PauseBGM()
    {
        if (_bgmSource != null && _bgmSource.isPlaying)
        {
            _bgmSource.Pause();
        }
    }

    public void ResumeBGM()
    {
        if (_bgmSource != null && _bgmSource.clip != null)
        {
            _bgmSource.UnPause();
        }
    }

    #endregion

    #region Spell Sounds

    /// <summary>
    /// Plays spell cast sound based on Form/Effect combination from SpellPrefabRegistry.
    /// Uses custom volume per spell if specified.
    /// </summary>
    public void PlaySpellCastSound(SpellForm form, SpellEffect effect, Vector3 position)
    {
        if (form == null || effect == null) return;

        var registry = SpellPrefabRegistry.Instance;
        if (registry == null) return;

        var (clip, volume) = registry.GetCastSound(form, effect);
        if (clip != null)
        {
            PlaySFX(clip, position, 1f, volume);
        }
    }

    /// <summary>
    /// Plays spell impact sound based on Form/Effect combination from SpellPrefabRegistry.
    /// Uses custom volume per spell if specified.
    /// </summary>
    public void PlaySpellImpactSound(SpellForm form, SpellEffect effect, Vector3 position)
    {
        if (form == null || effect == null) return;

        var registry = SpellPrefabRegistry.Instance;
        if (registry == null) return;

        var (clip, volume) = registry.GetImpactSound(form, effect);
        if (clip != null)
        {
            PlaySFX(clip, position, 1f, volume);
        }
    }

    #endregion

    #region Damage Sounds

    public void PlayEnemyHitSound(Vector3 position, bool isCrit = false)
    {
        if (_audioSettings == null) return;

        var (clip, volume) = isCrit ? _audioSettings.GetCritHitSound() : _audioSettings.GetEnemyHitSound();
        if (clip != null)
        {
            PlaySFX(clip, position, isCrit ? 1.1f : 1f, volume); // Slightly higher pitch for crits
        }
    }

    public void PlayPlayerHitSound()
    {
        // Throttle player hit sounds to avoid spam
        if (Time.time - _lastPlayerHitTime < PLAYER_HIT_COOLDOWN)
            return;

        if (_audioSettings == null)
            return;

        var (clip, volume) = _audioSettings.GetPlayerHitSound();
        if (clip == null)
            return;

        _lastPlayerHitTime = Time.time;
        PlaySFX(clip, Vector3.zero, 1f, volume);
    }

    public void PlayAreaExplosionSound(Vector3 position)
    {
        if (_audioSettings == null)
            return;

        var (clip, volume) = _audioSettings.GetAreaExplosionSound();
        if (clip == null)
            return;

        PlaySFX(clip, position, 1f, volume);
    }

    #endregion

    #region Event Sounds

    public void PlayEnemyDeathSound(Vector3 position)
    {
        if (_audioSettings == null)
            return;

        var (clip, volume) = _audioSettings.GetEnemyDeathSound();
        if (clip == null)
            return;

        PlaySFX(clip, position, 1f, volume);
    }

    public void PlayLevelUpSound()
    {
        if (_audioSettings == null)
            return;

        var (clip, volume) = _audioSettings.GetLevelUpSound();
        if (clip == null)
            return;

        PlaySFX(clip, Vector3.zero, 1f, volume);
    }

    public void PlayGameOverSound()
    {
        if (_audioSettings == null)
            return;

        var (clip, volume) = _audioSettings.GetGameOverSound();
        if (clip == null)
            return;

        PlaySFX(clip, Vector3.zero, 1f, volume);
    }

    #endregion

    #region Core Playback

    /// <summary>
    /// Plays a sound effect with optional pitch and volume multiplier.
    /// </summary>
    /// <param name="clip">Audio clip to play</param>
    /// <param name="position">3D position for the sound</param>
    /// <param name="pitch">Pitch multiplier (default 1)</param>
    /// <param name="volumeMultiplier">Additional volume multiplier (0-1, default 1)</param>
    private void PlaySFX(AudioClip clip, Vector3 position, float pitch = 1f, float volumeMultiplier = 1f)
    {
        if (clip == null || _audioPool == null) return;

        // Respect game state
        if (GameStateController.Instance != null && !GameStateController.Instance.IsPlaying)
        {
            // Allow UI sounds during LevelUp state, but not during Pause
            if (GameStateController.Instance.CurrentState != GameStateController.GameState.LevelingUp)
                return;
        }

        AudioSource source = _audioPool.GetAudioSource();
        if (source == null) return;

        source.clip = clip;
        source.volume = GetEffectiveSFXVolume() * volumeMultiplier;
        source.pitch = pitch;
        source.transform.position = position;
        source.Play();

        // Return to pool after clip finishes
        _audioPool.ReturnToPool(source, clip.length / pitch);
    }

    #endregion

    #region Game State Callbacks

    private void OnGamePaused()
    {
        PauseBGM();
    }

    private void OnGameResumed()
    {
        ResumeBGM();
    }

    #endregion
}
