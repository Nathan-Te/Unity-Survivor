using UnityEngine;

/// <summary>
/// Manages enemy stat scaling over time to increase difficulty as the game progresses.
/// Applies HP and damage multipliers based on elapsed game time.
/// </summary>
public class EnemyScalingManager : Singleton<EnemyScalingManager>
{
    [Header("Scaling Curves")]
    [Tooltip("HP multiplier over time (X-axis = seconds, Y-axis = multiplier)")]
    [SerializeField] private AnimationCurve hpScalingCurve = AnimationCurve.Linear(0, 1, 600, 3);
    [Tooltip("Damage multiplier over time (X-axis = seconds, Y-axis = multiplier)")]
    [SerializeField] private AnimationCurve damageScalingCurve = AnimationCurve.Linear(0, 1, 600, 2);

    [Header("Infinite Scaling (Beyond Curve)")]
    [Tooltip("Continue scaling after the curve ends using linear extrapolation")]
    [SerializeField] private bool enableInfiniteScaling = true;
    [Tooltip("HP increase per minute after curve ends (e.g., 0.5 = +50% per minute)")]
    [SerializeField] private float infiniteHpGrowthPerMinute = 0.5f;
    [Tooltip("Damage increase per minute after curve ends (e.g., 0.3 = +30% per minute)")]
    [SerializeField] private float infiniteDamageGrowthPerMinute = 0.3f;

    [Header("Manual Multipliers (Optional Override)")]
    [SerializeField] private bool useManualMultipliers = false;
    [SerializeField, Range(1f, 10f)] private float manualHpMultiplier = 1f;
    [SerializeField, Range(1f, 10f)] private float manualDamageMultiplier = 1f;

    [Header("Debug Info")]
    [SerializeField] private float currentHpMultiplier = 1f;
    [SerializeField] private float currentDamageMultiplier = 1f;

    /// <summary>
    /// Returns the current HP multiplier based on game time
    /// </summary>
    public float HpMultiplier
    {
        get
        {
            if (useManualMultipliers)
                return manualHpMultiplier;

            if (GameTimer.Instance == null)
                return 1f;

            float elapsedTime = GameTimer.Instance.ElapsedTime;
            currentHpMultiplier = EvaluateCurveWithInfiniteScaling(
                hpScalingCurve,
                elapsedTime,
                infiniteHpGrowthPerMinute
            );
            return currentHpMultiplier;
        }
    }

    /// <summary>
    /// Returns the current damage multiplier based on game time
    /// </summary>
    public float DamageMultiplier
    {
        get
        {
            if (useManualMultipliers)
                return manualDamageMultiplier;

            if (GameTimer.Instance == null)
                return 1f;

            float elapsedTime = GameTimer.Instance.ElapsedTime;
            currentDamageMultiplier = EvaluateCurveWithInfiniteScaling(
                damageScalingCurve,
                elapsedTime,
                infiniteDamageGrowthPerMinute
            );
            return currentDamageMultiplier;
        }
    }

    /// <summary>
    /// Evaluates a curve with optional infinite linear extrapolation beyond the last keyframe
    /// </summary>
    private float EvaluateCurveWithInfiniteScaling(AnimationCurve curve, float time, float growthPerMinute)
    {
        // If curve has no keys, return default
        if (curve.length == 0)
            return 1f;

        // Get the last keyframe time
        float lastKeyTime = curve.keys[curve.length - 1].time;

        // If we're within the curve range, use it directly
        if (time <= lastKeyTime)
        {
            return curve.Evaluate(time);
        }

        // If infinite scaling is disabled, clamp to last value
        if (!enableInfiniteScaling)
        {
            return curve.Evaluate(lastKeyTime);
        }

        // Beyond the curve: extrapolate linearly
        float baseValue = curve.Evaluate(lastKeyTime);
        float overtimeSeconds = time - lastKeyTime;
        float overtimeMinutes = overtimeSeconds / 60f;
        float extraGrowth = growthPerMinute * overtimeMinutes;

        return baseValue + extraGrowth;
    }

    /// <summary>
    /// Calculates scaled HP for an enemy
    /// </summary>
    public float GetScaledHp(float baseHp)
    {
        return baseHp * HpMultiplier;
    }

    /// <summary>
    /// Calculates scaled damage for an enemy
    /// </summary>
    public float GetScaledDamage(float baseDamage)
    {
        return baseDamage * DamageMultiplier;
    }

    private void OnValidate()
    {
        // Update debug values in editor
        if (Application.isPlaying)
        {
            currentHpMultiplier = HpMultiplier;
            currentDamageMultiplier = DamageMultiplier;
        }
    }
}
