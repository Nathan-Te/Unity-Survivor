# Audio System

Complete audio system for the Survivor game with integrated Form/Effect mapping, volume control, and object pooling.

## 🎯 Features

✅ **Integrated Mapping** - Spell sounds configured in `FormEffectPrefabMapping` (same place as prefabs/VFX)
✅ **Per-Spell Volume Control** - Adjust volume for each Form/Effect combination
✅ **3-Tier Volume System** (Global / Music / SFX)
✅ **Object Pooling** for performance (avoids runtime allocation)
✅ **Game State Integration** (auto-pause/resume with GameStateController)
✅ **Event-driven Architecture** (integrated with existing game events)

## 📁 Files

| File | Purpose |
|------|---------|
| `AudioManager.cs` | Central audio controller (singleton) |
| `AudioPool.cs` | AudioSource pooling system |
| `GameAudioSettings.cs` | BGM + event sounds (enemy death, level up, etc.) |
| `AudioInitializer.cs` | Auto-starts BGM on scene load |
| `AudioSettingsUI.cs` | Example UI for volume controls |
| `AUDIO_MAPPING_GUIDE.md` | ⭐ **Complete spell audio guide** |
| `AUDIO_SYSTEM_GUIDE.md` | General audio system guide |
| `GAME_AUDIO_SETTINGS_GUIDE.md` | ⭐ **GameAudioSettings editor guide** |
| `EDITOR_AUDIO_GUIDE.md` | FormEffectPrefabMapping editor guide |
| `README.md` | This file |

## 🚀 Quick Start

### 1. Setup Scene Objects

Create two GameObjects in your scene:
- **AudioManager** (add AudioManager component)
- **AudioPool** (add AudioPool component)

### 2. Configure GameAudioSettings

```
Right-click in Project → Create → SurvivorGame/Audio/Audio Settings
```

**Custom Editor with per-sound volume control!**

Assign clips and adjust volumes in the Inspector:
- **Background Music** : defaultBGM (70-80%), menuBGM, gameOverBGM
- **Event Sounds** : enemyDeathSound, levelUpSound, gameOverSound, playerHitSound
- **Generic SFX** : enemyHitSound (90-100%), critHitSound (100%+), areaExplosionSound

Each sound has:
- AudioClip field
- Volume slider (0-100%)
- Preview button [▶]

See [GAME_AUDIO_SETTINGS_GUIDE.md](GAME_AUDIO_SETTINGS_GUIDE.md) for detailed usage.

### 3. Configure Spell Sounds ⭐ NEW

Open your existing `FormEffectPrefabMapping` asset:

For each (Form, Effect) entry, assign:
- **Cast Sound** : AudioClip played when casting
- **Cast Volume** : 0-1 (default 1.0)
- **Impact Sound** : AudioClip played on hit
- **Impact Volume** : 0-1 (default 1.0)

**Example** :
```
Linear + Fire:
├─ Cast Sound: fire_woosh.wav
├─ Cast Volume: 0.8
├─ Impact Sound: fire_explosion.wav
└─ Impact Volume: 1.0

Smite + Ice:
├─ Cast Sound: ice_summon.wav
├─ Cast Volume: 0.9
├─ Impact Sound: ice_shatter.wav
└─ Impact Volume: 0.8
```

### 4. Auto-Start BGM (Optional)

Add `AudioInitializer` component to any GameObject in the scene.

### 5. Done!

All sounds are automatically integrated:
- ✅ Spell casting sounds
- ✅ Projectile impact sounds
- ✅ Player/enemy damage sounds
- ✅ Level up & game over sounds
- ✅ Background music with pause/resume

## 📖 Documentation

### For Spell Audio Configuration

See [AUDIO_MAPPING_GUIDE.md](AUDIO_MAPPING_GUIDE.md) for:
- How to configure spell sounds in FormEffectPrefabMapping
- Per-spell volume control
- Examples for different spell types
- Workflow and best practices

### For General Audio System

See [AUDIO_SYSTEM_GUIDE.md](AUDIO_SYSTEM_GUIDE.md) for:
- Complete API reference
- Volume system details
- Event integration points
- Troubleshooting guide

## 🎮 Example Usage

### Play Custom Sounds

```csharp
// Spell sounds (automatic from mapping)
AudioManager.Instance.PlaySpellCastSound(form, effect, position);
AudioManager.Instance.PlaySpellImpactSound(form, effect, position);

// Event sounds
AudioManager.Instance.PlayLevelUpSound();
AudioManager.Instance.PlayEnemyDeathSound(position);
AudioManager.Instance.PlayPlayerHitSound();

// Background music
AudioManager.Instance.PlayDefaultBGM();
AudioManager.Instance.PauseBGM();
AudioManager.Instance.ResumeBGM();

// Volume control
AudioManager.Instance.SetGlobalVolume(0.8f);
AudioManager.Instance.SetMusicVolume(0.5f);
AudioManager.Instance.SetSFXVolume(1.0f);
```

## 🔊 Volume System

```
Final Volume = Global × Category × Spell
```

**Example** :
- Global Volume = 0.8 (80%)
- SFX Volume = 1.0 (100%)
- Fire Bolt Cast Volume = 0.7 (70% in mapping)
- **Final = 0.8 × 1.0 × 0.7 = 0.56 (56%)**

This allows:
- User controls global/category volumes in settings
- Designers balance individual spells in mapping

## 🎯 Integrated Events

All game events automatically trigger appropriate sounds:

| Event | Sound Type | Configuration |
|-------|------------|---------------|
| **Spell Cast** | Form/Effect based | FormEffectPrefabMapping |
| **Spell Impact** | Form/Effect based | FormEffectPrefabMapping |
| **Critical Hit** | Crit sound | GameAudioSettings |
| **Area Explosion** | Explosion sound | GameAudioSettings |
| **Player Damage** | Hit sound (throttled) | GameAudioSettings |
| **Enemy Death** | Death sound | GameAudioSettings |
| **Level Up** | Level up jingle | GameAudioSettings |
| **Game Over** | Game over sound | GameAudioSettings |
| **Pause/Resume** | Auto BGM pause/resume | Automatic |

## ⚠️ Important Notes

1. **Spell sounds** are configured in `FormEffectPrefabMapping` (same asset as prefabs/VFX)
2. **Event sounds** are configured in `GameAudioSettings`
3. **AudioManager** and **AudioPool** are scene-based singletons (reinitialize on restart)
4. Player damage sounds are throttled (0.3s cooldown) to prevent spam
5. All sounds respect game state (no sounds during pause, UI sounds during level-up)

## 📊 Performance

- **Pooling** : 20 initial AudioSources, grows to 50 max
- **Auto-return** : Sources automatically return to pool after playback
- **No GC** : Reuses AudioSources instead of Instantiate/Destroy
- **Efficient Lookup** : O(n) mapping lookup where n is small (< 100 entries)

## 🛠️ Workflow

### Adding a New Spell

1. Open `FormEffectPrefabMapping`
2. Add entry for (Form, Effect) combination
3. Assign prefab, VFX (as usual)
4. Assign cast/impact sounds
5. Adjust volumes if needed
6. **Done** - sounds play automatically!

### Adjusting Volumes

**For all Fire spells** :
1. Open `FormEffectPrefabMapping`
2. Find all entries with `FireEffect`
3. Adjust `castVolume` and `impactVolume`

**For all SFX** :
```csharp
AudioManager.Instance.SetSFXVolume(0.7f); // 70%
```

**For everything** :
```csharp
AudioManager.Instance.SetGlobalVolume(0.5f); // 50%
```

## 🔧 Volume UI Example

Add `AudioSettingsUI` to a settings panel with 3 sliders:
- Global Volume Slider (0-1)
- Music Volume Slider (0-1)
- SFX Volume Slider (0-1)

See `AudioSettingsUI.cs` for implementation.

## 🐛 Troubleshooting

### No spell sounds?
- Check `FormEffectPrefabMapping` entry exists for (Form, Effect)
- Verify `castSound` or `impactSound` is assigned
- Ensure volume > 0

### No event sounds?
- Check `GameAudioSettings` is assigned to AudioManager
- Verify sound clips are assigned
- Check Global/SFX volumes > 0

### No BGM?
- Check `defaultBGM` is assigned in GameAudioSettings
- Verify `AudioInitializer` is in scene
- Check Global/Music volumes > 0

### Sounds too quiet/loud?
- Adjust Global Volume first
- Adjust SFX/Music Volume second
- Fine-tune individual spell volumes in mapping

## 📚 Architecture

```
┌─────────────────────────────────────────────────┐
│           FormEffectPrefabMapping               │
│  ┌───────────────────────────────────────────┐  │
│  │ Entry: Linear + Fire                      │  │
│  ├─ Prefab: FireBoltPrefab                   │  │
│  ├─ VFX: FireExplosionVFX                    │  │
│  ├─ Cast Sound: fire_cast.wav (vol 0.8) ⭐   │  │
│  └─ Impact Sound: fire_impact.wav (vol 1.0)⭐│  │
│  └───────────────────────────────────────────┘  │
└─────────────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────┐
│         SpellPrefabRegistry                     │
│  GetCastSound(form, effect) → (clip, volume)   │
│  GetImpactSound(form, effect) → (clip, volume) │
└─────────────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────┐
│              AudioManager                       │
│  PlaySpellCastSound(form, effect, pos)         │
│  PlaySpellImpactSound(form, effect, pos)       │
│  + Volume control (Global/Music/SFX)           │
└─────────────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────┐
│              AudioPool                          │
│  Manages 20-50 pooled AudioSources             │
│  Auto-returns after playback                   │
└─────────────────────────────────────────────────┘
```

---

**Version** : 2.0 (Integrated Mapping System)
**Last Updated** : 2025-12-29
**Dependencies** : GameStateController, SpellPrefabRegistry, FormEffectPrefabMapping, SpellCaster, ProjectileDamageHandler, PlayerController, EnemyEventBroadcaster, LevelManager
