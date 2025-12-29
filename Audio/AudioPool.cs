using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Object pool for AudioSource components to avoid runtime allocation.
/// Automatically returns sources to pool after playback completes.
/// </summary>
public class AudioPool : Singleton<AudioPool>
{
    [Header("Pool Settings")]
    [SerializeField] private int _initialPoolSize = 20;
    [SerializeField] private int _maxPoolSize = 50;

    private Queue<AudioSource> _availableSources = new Queue<AudioSource>();
    private List<AudioSource> _allSources = new List<AudioSource>();
    private Dictionary<AudioSource, float> _activeSourceTimers = new Dictionary<AudioSource, float>();

    protected override void Awake()
    {
        base.Awake();

        // Always initialize (critical for restart support)
        _availableSources.Clear();
        _allSources.Clear();
        _activeSourceTimers.Clear();

        // Pre-populate pool
        for (int i = 0; i < _initialPoolSize; i++)
        {
            CreateNewAudioSource();
        }
    }

    private void Update()
    {
        // Check for sources that have finished playing and return them to pool
        List<AudioSource> toReturn = new List<AudioSource>();

        foreach (var kvp in _activeSourceTimers)
        {
            if (Time.time >= kvp.Value)
            {
                toReturn.Add(kvp.Key);
            }
        }

        foreach (var source in toReturn)
        {
            ReturnSourceToPool(source);
        }
    }

    /// <summary>
    /// Get an AudioSource from the pool. Creates new one if pool is empty.
    /// </summary>
    public AudioSource GetAudioSource()
    {
        AudioSource source;

        if (_availableSources.Count > 0)
        {
            source = _availableSources.Dequeue();
        }
        else
        {
            // Pool exhausted - create new source if below max size
            if (_allSources.Count < _maxPoolSize)
            {
                source = CreateNewAudioSource();
            }
            else
            {
                // Max pool size reached - reuse oldest active source
                Debug.LogWarning($"[AudioPool] Max pool size ({_maxPoolSize}) reached. Reusing oldest source.");
                source = _allSources[0];
                source.Stop();
                _activeSourceTimers.Remove(source);
            }
        }

        source.gameObject.SetActive(true);
        return source;
    }

    /// <summary>
    /// Schedule an AudioSource to return to pool after specified duration.
    /// </summary>
    public void ReturnToPool(AudioSource source, float delay)
    {
        if (source == null) return;

        float returnTime = Time.time + delay;

        if (_activeSourceTimers.ContainsKey(source))
        {
            _activeSourceTimers[source] = returnTime;
        }
        else
        {
            _activeSourceTimers.Add(source, returnTime);
        }
    }

    /// <summary>
    /// Immediately return an AudioSource to the pool.
    /// </summary>
    private void ReturnSourceToPool(AudioSource source)
    {
        if (source == null) return;

        source.Stop();
        source.clip = null;
        source.gameObject.SetActive(false);

        _activeSourceTimers.Remove(source);

        if (!_availableSources.Contains(source))
        {
            _availableSources.Enqueue(source);
        }
    }

    /// <summary>
    /// Create a new AudioSource and add it to the pool.
    /// </summary>
    private AudioSource CreateNewAudioSource()
    {
        GameObject sourceObj = new GameObject($"PooledAudioSource_{_allSources.Count}");
        sourceObj.transform.SetParent(transform);
        sourceObj.SetActive(false);

        AudioSource source = sourceObj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f; // 2D sound by default
        source.loop = false;

        _allSources.Add(source);
        _availableSources.Enqueue(source);

        return source;
    }

    /// <summary>
    /// Stop all currently playing audio and return all sources to pool.
    /// </summary>
    public void StopAllSounds()
    {
        foreach (var source in _allSources)
        {
            if (source != null && source.isPlaying)
            {
                source.Stop();
            }
        }

        _activeSourceTimers.Clear();
        _availableSources.Clear();

        foreach (var source in _allSources)
        {
            if (source != null)
            {
                source.clip = null;
                source.gameObject.SetActive(false);
                _availableSources.Enqueue(source);
            }
        }
    }

    #region Debug Info

    public int GetActiveSourceCount()
    {
        return _activeSourceTimers.Count;
    }

    public int GetAvailableSourceCount()
    {
        return _availableSources.Count;
    }

    public int GetTotalSourceCount()
    {
        return _allSources.Count;
    }

    #endregion
}
