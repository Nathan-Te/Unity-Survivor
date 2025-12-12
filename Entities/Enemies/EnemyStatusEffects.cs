using UnityEngine;

/// <summary>
/// Handles status effects (Burn, Slow) for enemies.
/// </summary>
[RequireComponent(typeof(EnemyController))]
public class EnemyStatusEffects : MonoBehaviour
{
    private EnemyController _controller;

    // Burn state
    private float _burnTimer;
    private float _burnDamagePerSec;
    private float _burnTickTimer;

    // Slow state
    private float _slowTimer;
    private float _originalSpeed;
    private bool _isSlowed;

    private void Awake()
    {
        _controller = GetComponent<EnemyController>();
    }

    /// <summary>
    /// Updates all active status effects. Called from time-sliced logic.
    /// </summary>
    public void UpdateStatusEffects(float deltaTime)
    {
        HandleBurn(deltaTime);
        HandleSlow(deltaTime);
    }

    /// <summary>
    /// Applies burn damage over time
    /// </summary>
    public void ApplyBurn(float dps, float duration)
    {
        // If no burn active, or this burn is stronger, apply it
        if (_burnTimer <= 0 || dps > _burnDamagePerSec)
        {
            _burnDamagePerSec = dps;
        }

        _burnTimer = duration;
    }

    /// <summary>
    /// Applies slow effect that reduces movement speed
    /// </summary>
    public void ApplySlow(float factor, float duration)
    {
        if (!_isSlowed)
        {
            _originalSpeed = _controller.currentSpeed;
            _controller.currentSpeed *= factor;
            _isSlowed = true;
        }

        _slowTimer = duration;
    }

    /// <summary>
    /// Resets all status effects (called when enemy is pooled)
    /// </summary>
    public void ResetStatusEffects()
    {
        _burnTimer = 0f;
        _burnDamagePerSec = 0f;
        _burnTickTimer = 0f;
        _slowTimer = 0f;
        _isSlowed = false;
    }

    private void HandleBurn(float deltaTime)
    {
        if (_burnTimer <= 0) return;

        _burnTimer -= deltaTime;
        _burnTickTimer += deltaTime;

        // Tick every second
        if (_burnTickTimer >= 1.0f)
        {
            _controller.TakeDamage(_burnDamagePerSec);
            _burnTickTimer -= 1.0f; // Preserve precision
        }
    }

    private void HandleSlow(float deltaTime)
    {
        if (_slowTimer <= 0) return;

        _slowTimer -= deltaTime;

        if (_slowTimer <= 0)
        {
            _controller.currentSpeed = _originalSpeed;
            _isSlowed = false;
        }
    }
}
