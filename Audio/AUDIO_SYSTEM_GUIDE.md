# Audio System Guide

Complete audio system for Survivor game with volume control, pooling, and game state integration.

## 📁 System Architecture

```
Audio System
├── AudioManager (Singleton) - Central audio controller
├── AudioPool (Singleton) - AudioSource pooling for performance
├── AudioSettings (ScriptableObject) - Audio clip database
└── AudioInitializer - Auto-start BGM on scene load
```

## 🎵 Volume Categories

The system supports 3 volume categories:
- **Global Volume** (0-1) - Master volume for all audio
- **Music Volume** (0-1) - BGM volume multiplier
- **SFX Volume** (0-1) - Sound effects volume multiplier

**Effective Volume Formula:**
- BGM: `Global * Music`
- SFX: `Global * SFX`

## 🔧 Setup Instructions

### 1. Create Audio Settings Asset

1. Right-click in Project window → `Create → SurvivorGame/Audio/Audio Settings`
2. Name it `AudioSettings`
3. Assign audio clips to appropriate fields:
   - **BGM**: Background music tracks
   - **Spell Casts**: Sounds for Linear, Smite, Orbit, Nova spell types
   - **Spell Impacts**: Sounds for Fire, Ice, Lightning, Physical elements
   - **Damage**: Hit sounds for enemy, crit, player, explosions
   - **Events**: Level up, enemy death, game over, pause/resume

### 2. Setup AudioManager in Scene

1. Create empty GameObject in scene: `AudioManager`
2. Add component: `AudioManager`
3. Assign the `AudioSettings` asset you created
4. (Optional) Adjust default volumes in inspector

### 3. Setup AudioPool in Scene

1. Create empty GameObject in scene: `AudioPool`
2. Add component: `AudioPool`
3. Configure pool settings:
   - **Initial Pool Size**: 20 (default)
   - **Max Pool Size**: 50 (default)

### 4. Auto-Start BGM (Optional)

1. Select any GameObject in the game scene (e.g., GameDirector)
2. Add component: `AudioInitializer`
3. Enable "Play BGM On Start" checkbox

**Note:** Both `AudioManager` and `AudioPool` are scene-based singletons. They will auto-reinitialize on scene restart.

## 🎮 Current Sound Triggers

The system is already integrated into the following game events:

### ✅ Spell Casting
- **Location**: `SpellCaster.cs`
- **Trigger**: When `Fire()` is called
- **Sound Selection**: Based on `SpellForm` type (Linear, Smite, Orbit, Nova)

### ✅ Projectile Impacts
- **Location**: `ProjectileDamageHandler.cs`
- **Single Hit**: Based on spell's `ElementType` (Fire, Ice, Lightning, Physical)
- **Area Explosion**: Plays explosion sound for AoE spells
- **Crit Hit**: Special sound for critical hits

### ✅ Player Damage
- **Location**: `PlayerController.cs`
- **Trigger**: When player takes damage
- **Throttling**: 0.3s cooldown to avoid spam

### ✅ Enemy Death
- **Location**: `EnemyEventBroadcaster.cs`
- **Trigger**: When enemy dies
- **Positioned**: Plays at enemy death position

### ✅ Level Up
- **Location**: `LevelManager.cs`
- **Trigger**: When player levels up

### ✅ Game Over
- **Location**: `PlayerController.cs`
- **Trigger**: When player HP reaches 0

### ✅ Pause/Resume
- **Location**: `AudioManager.cs` (subscribed to `GameStateController`)
- **Auto-pauses**: BGM pauses when game is paused
- **Auto-resumes**: BGM resumes when game continues

## 📝 API Reference

### AudioManager Methods

```csharp
// Volume Control
AudioManager.Instance.SetGlobalVolume(float volume); // 0-1
AudioManager.Instance.SetMusicVolume(float volume);  // 0-1
AudioManager.Instance.SetSFXVolume(float volume);    // 0-1

// Background Music
AudioManager.Instance.PlayDefaultBGM();
AudioManager.Instance.PlayBGM(AudioClip clip);
AudioManager.Instance.StopBGM();
AudioManager.Instance.PauseBGM();
AudioManager.Instance.ResumeBGM();

// Spell Sounds
AudioManager.Instance.PlaySpellCastSound(SpellForm form, Vector3 position);
AudioManager.Instance.PlaySpellImpactSound(SpellDefinition spell, Vector3 position);

// Damage Sounds
AudioManager.Instance.PlayEnemyHitSound(Vector3 position, bool isCrit = false);
AudioManager.Instance.PlayPlayerHitSound();
AudioManager.Instance.PlayAreaExplosionSound(Vector3 position);

// Event Sounds
AudioManager.Instance.PlayEnemyDeathSound(Vector3 position);
AudioManager.Instance.PlayLevelUpSound();
AudioManager.Instance.PlayGameOverSound();
```

### AudioPool Methods

```csharp
// Get AudioSource from pool
AudioSource source = AudioPool.Instance.GetAudioSource();

// Return to pool after duration
AudioPool.Instance.ReturnToPool(AudioSource source, float delay);

// Emergency: Stop all sounds
AudioPool.Instance.StopAllSounds();

// Debug Info
int active = AudioPool.Instance.GetActiveSourceCount();
int available = AudioPool.Instance.GetAvailableSourceCount();
int total = AudioPool.Instance.GetTotalSourceCount();
```

## 🎯 Adding New Sounds

### Example: Add a new UI click sound

1. **Add field to AudioSettings.cs:**
```csharp
[Header("UI Sounds")]
public AudioClip uiClickSound;
```

2. **Add method to AudioManager.cs:**
```csharp
public void PlayUIClickSound()
{
    if (_audioSettings == null || _audioSettings.uiClickSound == null)
        return;

    PlaySFX(_audioSettings.uiClickSound, Vector3.zero);
}
```

3. **Call from UI code:**
```csharp
using SurvivorGame.Audio;

public void OnButtonClicked()
{
    AudioManager.Instance.PlayUIClickSound();
    // ... rest of button logic
}
```

## ⚡ Performance Considerations

### AudioPool Optimization
- **Pooling**: Avoids `Instantiate`/`Destroy` during gameplay
- **Auto-return**: Sources automatically return to pool after playback
- **Dynamic growth**: Pool grows up to `maxPoolSize` if needed
- **Reuse strategy**: If pool is full, oldest active source is reused

### Sound Throttling
- **Player damage**: 0.3s cooldown to prevent spam
- **Game state**: Sounds respect pause/level-up states

### Spatial Audio
- All SFX use 2D audio by default (`spatialBlend = 0`)
- Position parameter is available for future 3D audio upgrade

## 🔄 Game State Integration

The audio system automatically responds to game states:

| Game State | BGM Behavior | SFX Behavior |
|------------|--------------|--------------|
| **Playing** | Playing | Enabled |
| **Paused** | Paused | Disabled |
| **LevelingUp** | Paused | UI sounds only |
| **Restarting** | Continues | Disabled |

## 🐛 Troubleshooting

### No sound playing?
1. Check `AudioManager` and `AudioPool` exist in scene
2. Verify `AudioSettings` asset is assigned to AudioManager
3. Check Global/Music/SFX volumes are > 0
4. Ensure audio clips are assigned in AudioSettings

### BGM not starting?
1. Check `AudioInitializer` is present and enabled
2. Verify `defaultBGM` is assigned in AudioSettings
3. Check Music Volume > 0

### Sounds cutting off early?
1. Pool might be full - increase `maxPoolSize` in AudioPool
2. Check console for pool warnings

### Player hit sound spamming?
- This is throttled automatically (0.3s cooldown)
- Adjust `PLAYER_HIT_COOLDOWN` constant in AudioManager if needed

## 📊 Technical Details

### Singleton Pattern
- **AudioManager**: Scene-based singleton (destroyed on restart)
- **AudioPool**: Scene-based singleton (destroyed on restart)
- Both inherit from `Singleton<T>` and follow CLAUDE.md restart guidelines

### Event Subscriptions
- AudioManager subscribes to `GameStateController` events in `Start()`
- Unsubscribes in `OnDestroy()` to prevent memory leaks

### Audio Clip Selection
- **Spell casts**: Based on `SpellForm.tags` (Smite, Orbit, Nova, Linear)
- **Spell impacts**: Based on `SpellEffect.elementType` (Fire, Ice, Lightning, Physical)
- **Fallback**: Generic clips used if specific element sound is missing

## 🎨 Best Practices

1. **Always check Instance:** `if (AudioManager.Instance != null)`
2. **Use volume controls:** Don't set AudioSource.volume directly
3. **Respect game state:** Audio system handles this automatically
4. **Position matters:** Pass correct position for future 3D audio support
5. **Don't spam:** High-frequency sounds are automatically throttled
6. **Pool cleanup:** AudioPool auto-manages sources, no manual cleanup needed

## 📚 Related Files

```
c:\Users\miste\Unity Projects\Survivor\Assets\Scripts\Audio\
├── AudioManager.cs          # Main audio controller
├── AudioPool.cs             # AudioSource pooling
├── AudioSettings.cs         # Audio clip database (ScriptableObject)
├── AudioInitializer.cs      # Auto-start BGM helper
└── AUDIO_SYSTEM_GUIDE.md    # This file
```

---

**Last Updated:** 2025-12-29
**System Version:** 1.0
