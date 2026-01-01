using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles status effects (Burn, Slow) for enemies.
/// </summary>
[RequireComponent(typeof(EnemyController))]
public class EnemyStatusEffects : MonoBehaviour
{
    private EnemyController _controller;

    // Burn state - support stacking multiple burns
    private List<BurnEffect> _activeBurns = new List<BurnEffect>();
    private float _burnTickTimer;

    // Slow state
    private float _slowTimer;
    private float _originalSpeed;
    private bool _isSlowed;

    // Necrotic marking state (for ghost minion spawning)
    private float _necroticMarkTime = -999f; // Time when last marked by Necrotic damage
    private const float NECROTIC_WINDOW = 3f; // Seconds after mark that minion can spawn
    private bool _hasSpawnedGhost = false; // Track if this enemy already spawned a ghost

    /// <summary>
    /// Returns true if the enemy is currently slowed
    /// </summary>
    public bool IsSlowed => _isSlowed;

    /// <summary>
    /// Returns true if the enemy was recently marked by Necrotic damage (within time window) AND hasn't spawned a ghost yet
    /// </summary>
    public bool IsMarkedByNecrotic => !_hasSpawnedGhost && (Time.time - _necroticMarkTime) <= NECROTIC_WINDOW;

    /// <summary>
    /// Represents a single burn effect instance
    /// </summary>
    private class BurnEffect
    {
        public float DamagePerSec;
        public float RemainingDuration;

        public BurnEffect(float dps, float duration)
        {
            DamagePerSec = dps;
            RemainingDuration = duration;
        }
    }

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
    /// Applies burn damage over time. Burns stack - each application adds a new burn instance.
    /// </summary>
    public void ApplyBurn(float dps, float duration)
    {
        // Add new burn to the stack
        _activeBurns.Add(new BurnEffect(dps, duration));
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
    /// Marks this enemy as hit by Necrotic damage (for ghost minion spawning)
    /// </summary>
    public void ApplyNecroticMark()
    {
        _necroticMarkTime = Time.time;
    }

    /// <summary>
    /// Marks this enemy as having spawned a ghost (prevents duplicate spawns)
    /// </summary>
    public void MarkGhostSpawned()
    {
        _hasSpawnedGhost = true;
    }

    /// <summary>
    /// Resets all status effects (called when enemy is pooled)
    /// </summary>
    public void ResetStatusEffects()
    {
        _activeBurns.Clear();
        _burnTickTimer = 0f;
        _slowTimer = 0f;
        _isSlowed = false;
        _necroticMarkTime = -999f;
        _hasSpawnedGhost = false;
    }

    private void HandleBurn(float deltaTime)
    {
        if (_activeBurns.Count == 0) return;

        // Update all burn durations and calculate total damage
        float totalDamagePerSec = 0f;
        for (int i = _activeBurns.Count - 1; i >= 0; i--)
        {
            _activeBurns[i].RemainingDuration -= deltaTime;

            // Remove expired burns
            if (_activeBurns[i].RemainingDuration <= 0)
            {
                _activeBurns.RemoveAt(i);
            }
            else
            {
                // Accumulate damage from active burns
                totalDamagePerSec += _activeBurns[i].DamagePerSec;
            }
        }

        // Apply accumulated damage every second
        if (totalDamagePerSec > 0)
        {
            _burnTickTimer += deltaTime;

            if (_burnTickTimer >= 1.0f)
            {
                _controller.TakeDamage(totalDamagePerSec, DamageType.Burn);
                _burnTickTimer -= 1.0f; // Preserve precision
            }
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
