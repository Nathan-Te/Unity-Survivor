using UnityEngine;

/// <summary>
/// Manages ZonePOI difficulty scaling over time.
/// Increases chargeSpeed and chargePerKill requirements as the game progresses
/// to make zones easier early game and harder in late game.
/// </summary>
public class ZonePOIScalingManager : Singleton<ZonePOIScalingManager>
{
    [Header("Scaling Curves")]
    [Tooltip("ChargeSpeed multiplier over time (X-axis = seconds, Y-axis = multiplier)\nHigher = faster charge = easier")]
    [SerializeField] private AnimationCurve chargeSpeedScalingCurve = AnimationCurve.Linear(0, 1, 600, 0.5f);

    [Tooltip("ChargePerKill multiplier over time (X-axis = seconds, Y-axis = multiplier)\nHigher = more charge per kill = easier")]
    [SerializeField] private AnimationCurve chargePerKillScalingCurve = AnimationCurve.Linear(0, 1, 600, 0.5f);

    [Header("Infinite Scaling (Beyond Curve)")]
    [Tooltip("Continue scaling after the curve ends using linear extrapolation")]
    [SerializeField] private bool enableInfiniteScaling = true;

    [Tooltip("ChargeSpeed decrease per minute after curve ends (e.g., -0.05 = -5% per minute)")]
    [SerializeField] private float infiniteChargeSpeedGrowthPerMinute = -0.05f;

    [Tooltip("ChargePerKill decrease per minute after curve ends (e.g., -0.05 = -5% per minute)")]
    [SerializeField] private float infiniteChargePerKillGrowthPerMinute = -0.05f;

    [Header("Clamping (Min/Max Difficulty)")]
    [Tooltip("Minimum multiplier allowed (prevents zones from becoming too hard)")]
    [SerializeField, Range(0.1f, 1f)] private float minMultiplier = 0.3f;

    [Tooltip("Maximum multiplier allowed (prevents zones from being too easy)")]
    [SerializeField, Range(1f, 5f)] private float maxMultiplier = 2f;

    [Header("Manual Multipliers (Optional Override)")]
    [SerializeField] private bool useManualMultipliers = false;
    [SerializeField, Range(0.1f, 5f)] private float manualChargeSpeedMultiplier = 1f;
    [SerializeField, Range(0.1f, 5f)] private float manualChargePerKillMultiplier = 1f;

    [Header("Debug Info")]
    [SerializeField] private float currentChargeSpeedMultiplier = 1f;
    [SerializeField] private float currentChargePerKillMultiplier = 1f;

    /// <summary>
    /// Returns the current ChargeSpeed multiplier based on game time
    /// </summary>
    public float ChargeSpeedMultiplier
    {
        get
        {
            if (useManualMultipliers)
                return manualChargeSpeedMultiplier;

            if (GameTimer.Instance == null)
                return 1f;

            float elapsedTime = GameTimer.Instance.ElapsedTime;
            currentChargeSpeedMultiplier = EvaluateCurveWithInfiniteScaling(
                chargeSpeedScalingCurve,
                elapsedTime,
                infiniteChargeSpeedGrowthPerMinute
            );
            return Mathf.Clamp(currentChargeSpeedMultiplier, minMultiplier, maxMultiplier);
        }
    }

    /// <summary>
    /// Returns the current ChargePerKill multiplier based on game time
    /// </summary>
    public float ChargePerKillMultiplier
    {
        get
        {
            if (useManualMultipliers)
                return manualChargePerKillMultiplier;

            if (GameTimer.Instance == null)
                return 1f;

            float elapsedTime = GameTimer.Instance.ElapsedTime;
            currentChargePerKillMultiplier = EvaluateCurveWithInfiniteScaling(
                chargePerKillScalingCurve,
                elapsedTime,
                infiniteChargePerKillGrowthPerMinute
            );
            return Mathf.Clamp(currentChargePerKillMultiplier, minMultiplier, maxMultiplier);
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
    /// Calculates scaled chargeSpeed for a ZonePOI
    /// </summary>
    public float GetScaledChargeSpeed(float baseChargeSpeed)
    {
        return baseChargeSpeed * ChargeSpeedMultiplier;
    }

    /// <summary>
    /// Calculates scaled chargePerKill for a ZonePOI
    /// </summary>
    public float GetScaledChargePerKill(float baseChargePerKill)
    {
        return baseChargePerKill * ChargePerKillMultiplier;
    }

    private void OnValidate()
    {
        // Update debug values in editor
        if (Application.isPlaying)
        {
            currentChargeSpeedMultiplier = ChargeSpeedMultiplier;
            currentChargePerKillMultiplier = ChargePerKillMultiplier;
        }
    }
}
