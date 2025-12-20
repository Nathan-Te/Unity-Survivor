# Enemy Scaling System - Setup Guide

## Quick Setup (3 steps)

### 1. Create the GameObject in your scene
1. In your main scene, create an empty GameObject: `GameObject > Create Empty`
2. Rename it to `EnemyScalingManager`
3. Add the component: `Add Component > EnemyScalingManager`

### 2. Configure the scaling curves (Inspector)
The manager has two AnimationCurves to configure:

**HP Scaling Curve** (default: 1x → 3x over 600 seconds)
- Start: (0s, 1.0) = Normal HP at start
- End: (600s, 3.0) = 3x HP after 10 minutes
- Adjust the curve shape for exponential/linear/custom scaling
- **X-axis = seconds of gameplay** (not a ratio!)

**Damage Scaling Curve** (default: 1x → 2x over 600 seconds)
- Start: (0s, 1.0) = Normal damage at start
- End: (600s, 2.0) = 2x damage after 10 minutes
- Adjust the curve shape for exponential/linear/custom scaling
- **X-axis = seconds of gameplay** (not a ratio!)

### 2b. Configure infinite scaling (for endless mode)
**Enable Infinite Scaling** (default: true)
- When enabled, scaling continues beyond the last keyframe using linear growth
- When disabled, multipliers plateau at the last keyframe value

**Infinite HP Growth Per Minute** (default: 0.5)
- Additional HP multiplier per minute after curve ends
- Example: If curve ends at x3 HP, and growth = 0.5, after 2 more minutes = 3 + (0.5 × 2) = 4.0x HP

**Infinite Damage Growth Per Minute** (default: 0.3)
- Additional damage multiplier per minute after curve ends
- Example: If curve ends at x2 DMG, and growth = 0.3, after 3 more minutes = 2 + (0.3 × 3) = 2.9x DMG

### 3. Test the system
- Press Play
- Open the GameDirector debug window (press `` ` `` key)
- Watch the "Enemy Scaling" section to see live multipliers
- Use Time Scale controls to speed up time and see scaling in action

## Advanced: Manual Override
If you want to test specific multipliers without waiting:
1. Check "Use Manual Multipliers" in Inspector
2. Adjust "Manual Hp Multiplier" and "Manual Damage Multiplier" sliders
3. Enemies spawned will use these fixed values instead of time-based curves

## How it works
1. `EnemyScalingManager` is a Singleton that calculates multipliers based on `GameTimer.ElapsedTime`
2. When enemies spawn, `EnemyController.InitializeStats()` queries the manager
3. Base HP and Damage from `EnemyData` are multiplied by current scaling values
4. **Within curve range:** Evaluates the curve directly at elapsed time
5. **Beyond curve range (infinite mode):** Adds linear growth per minute
6. Example:
   - `EnemyData.baseHp = 50`
   - At 5 minutes (within curve): `HpMultiplier = 2.0` → Enemy HP = `100`
   - At 15 minutes (5 min past curve end at x3): `HpMultiplier = 3.0 + (0.5 × 5) = 5.5` → Enemy HP = `275`

## Curve Design Tips

**Linear Scaling** (Constant increase)
```
AnimationCurve.Linear(0, 1, 600, 3)
```

**Exponential Scaling** (Accelerating difficulty)
```
Create a curve with:
- (0, 1.0)
- (300, 1.5) - Gentle at first
- (600, 4.0) - Steep at the end
```

**Plateau Scaling** (Early spike, then stable)
```
Create a curve with:
- (0, 1.0)
- (120, 2.5) - Quick ramp
- (600, 2.5) - Stays constant
```

## Integration with Game Loop
The system is fully automatic once set up:
- No code changes needed in `WaveManager`
- No code changes needed in `EnemyData` ScriptableObjects
- All enemies automatically scale (Melee, Ranged, Bosses)
- Boss HP bars reflect scaled values correctly

## Debug Commands (GameDirector)
- `Toggle God Mode` - Survive high damage enemies
- `KILL ALL ENEMIES` - Clear the screen to test respawn
- `2x / 5x Time Scale` - Speed up time to see scaling faster
- Enemy Scaling display shows live multipliers

## Performance
- Zero runtime overhead (properties, no Update loop)
- Curve evaluation is O(1) via Unity's AnimationCurve
- No garbage collection (value types only)
