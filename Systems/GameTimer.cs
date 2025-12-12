using System;
using UnityEngine;

public class GameTimer : Singleton<GameTimer>
{
    // --- EVENTS ---
    public event Action<float> OnTimeChanged;

    // --- STATE ---
    private float _elapsedTime = 0f;
    private bool _isPaused = false;

    public float ElapsedTime => _elapsedTime;
    public bool IsPaused => _isPaused;

    private void Update()
    {
        if (!_isPaused)
        {
            _elapsedTime += Time.deltaTime;
            OnTimeChanged?.Invoke(_elapsedTime);
        }
    }

    /// <summary>
    /// Formate le temps écoulé en string avec format HH:MM:SS ou MM:SS
    /// </summary>
    /// <param name="hideHoursIfZero">Si true, cache les heures si moins d'1 heure</param>
    /// <returns>Temps formaté</returns>
    public string GetFormattedTime(bool hideHoursIfZero = true)
    {
        return FormatTime(_elapsedTime, hideHoursIfZero);
    }

    /// <summary>
    /// Formate un temps donné en string
    /// </summary>
    public static string FormatTime(float timeInSeconds, bool hideHoursIfZero = true)
    {
        int totalSeconds = Mathf.FloorToInt(timeInSeconds);
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;

        if (hideHoursIfZero && hours == 0)
        {
            return $"{minutes:D2}:{seconds:D2}";
        }
        else
        {
            return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }
    }

    public void PauseTimer()
    {
        _isPaused = true;
    }

    public void ResumeTimer()
    {
        _isPaused = false;
    }

    public void ResetTimer()
    {
        _elapsedTime = 0f;
        OnTimeChanged?.Invoke(_elapsedTime);
    }

}
