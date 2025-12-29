# GameAudioSettings Editor Guide

Guide for using the custom editor for GameAudioSettings with per-sound volume control.

## 🎯 Overview

The GameAudioSettings asset uses a custom editor that allows you to:
- ✅ Assign audio clips for all game events and background music
- ✅ Adjust volume per sound (0-100%)
- ✅ Preview sounds directly in the editor with [▶] buttons
- ✅ View statistics on how many sounds are configured

## 📁 Location

The GameAudioSettings asset should be located in:
```
Assets/ScriptableObjects/Audio/GameAudioSettings.asset
```

## 📋 Interface Overview

```
┌─────────────────────────────────────────┐
│ Game Audio Settings                     │
│ ℹ️  Configure all game audio clips...   │
├─────────────────────────────────────────┤
│ Statistics                              │
│ Assigned: 8/12 sounds                   │
│ Completion: 67%                         │
├─────────────────────────────────────────┤
│ Background Music                        │
│ ┌─────────────────────────────────────┐ │
│ │ Default BGM  [clip] Vol: [━━━] 80% ▶││
│ │ Menu BGM     [clip] Vol: [━━━] 70% ▶││
│ │ Game Over BGM [clip] Vol: [━━━] 90%▶││
│ └─────────────────────────────────────┘ │
├─────────────────────────────────────────┤
│ Damage Sounds                           │
│ ┌─────────────────────────────────────┐ │
│ │ Enemy Hit    [clip] Vol: [━━━] 100%▶││
│ │ Crit Hit     [clip] Vol: [━━━] 110%▶││
│ │ Player Hit   [clip] Vol: [━━━] 90% ▶││
│ │ Area Explosion [clip] Vol: [━━] 95%▶││
│ └─────────────────────────────────────┘ │
├─────────────────────────────────────────┤
│ Event Sounds                            │
│ ┌─────────────────────────────────────┐ │
│ │ Enemy Death  [clip] Vol: [━━━] 80% ▶││
│ │ Level Up     [clip] Vol: [━━━] 100%▶││
│ │ Game Over    [clip] Vol: [━━━] 90% ▶││
│ │ Pause        [clip] Vol: [━━━] 70% ▶││
│ │ Resume       [clip] Vol: [━━━] 70% ▶││
│ └─────────────────────────────────────┘ │
└─────────────────────────────────────────┘
```

## 🎵 Sound Categories

### Background Music
- **Default BGM**: Music played during normal gameplay
- **Menu BGM**: Music for main menu (if applicable)
- **Game Over BGM**: Music when player dies

### Damage Sounds
- **Enemy Hit**: Generic hit sound when damaging enemies
- **Crit Hit**: Special sound for critical hits (higher pitch)
- **Player Hit**: Sound when player takes damage (throttled 0.3s)
- **Area Explosion**: Sound for AoE explosions

### Event Sounds
- **Enemy Death**: Sound when enemy dies
- **Level Up**: Sound when player levels up
- **Game Over**: Sound when game ends
- **Pause**: Sound when pausing game (optional)
- **Resume**: Sound when resuming game (optional)

## 🔧 How to Use

### 1. Assigning a Sound

1. Click on the clip field next to the sound name
2. Select an AudioClip from your project
3. The volume slider appears automatically
4. Adjust volume (0-100%, default 100%)
5. Click [▶] to preview the sound

**Example**:
```
Enemy Hit Sound:
1. Click clip field
2. Select "enemy_hurt.wav"
3. Set volume to 80%
4. Preview with [▶]
```

### 2. Adjusting Volume

The volume slider controls the **individual sound multiplier**.

**Final Volume Formula**:
```
Final Volume = Global × Category × Sound Multiplier
```

**Example**:
- Global Volume = 0.8 (80%)
- SFX Volume = 1.0 (100%)
- Enemy Hit Volume = 0.9 (90% in editor)
- **Final = 0.8 × 1.0 × 0.9 = 0.72 (72%)**

**Recommendations**:
- **BGM**: 70-90% (background music shouldn't overpower gameplay)
- **Impact Sounds**: 90-100% (loud and impactful)
- **UI Sounds**: 70-80% (subtle feedback)
- **Critical/Special**: 100-110% (emphasize importance)

### 3. Previewing Sounds

The [▶] button lets you preview sounds in the Unity editor:
- ✅ Stops any currently playing preview
- ✅ Plays the selected clip
- ✅ Logs the in-game volume to console

**Note**: Unity's preview system doesn't support custom volume, but the console shows what volume will be used in-game.

**Console Output**:
```
[Audio Preview] Playing: enemy_hurt.wav (Volume in-game: 90%)
```

## 📊 Statistics Section

The statistics section shows:
- **Assigned**: How many sounds have clips assigned
- **Completion**: Percentage of sounds configured

**Example**:
```
Assigned: 8/12 sounds
Completion: 67%
```

This helps track progress when setting up audio for your game.

## 🎮 In-Game Behavior

Once configured, sounds play automatically:

| Event | When It Plays | Configuration Field |
|-------|--------------|---------------------|
| **BGM Start** | Game starts | Default BGM |
| **Enemy Hit** | Projectile hits enemy | Enemy Hit Sound |
| **Critical Hit** | Crit damage occurs | Crit Hit Sound |
| **Player Damage** | Player takes damage | Player Hit Sound (throttled) |
| **Area Explosion** | AoE effect triggers | Area Explosion Sound |
| **Enemy Death** | Enemy dies | Enemy Death Sound |
| **Level Up** | Player levels up | Level Up Sound |
| **Game Over** | Player dies | Game Over Sound |
| **Pause/Resume** | Menu toggles | Pause/Resume Sound |

## ⚠️ Important Notes

1. **Spell sounds** are NOT in GameAudioSettings - they're in FormEffectPrefabMapping
2. **Volume stacking**: Individual volumes multiply with Global/SFX/Music volumes
3. **Player Hit throttling**: Max 1 sound every 0.3s to prevent spam
4. **Preview limitations**: Unity preview doesn't support custom volume (but in-game does)
5. **Optional sounds**: Pause/Resume sounds are optional (can be left unassigned)

## 🛠️ Workflow

### Setting Up All Sounds

1. Open `GameAudioSettings` asset in Inspector
2. For each category:
   - Assign appropriate AudioClips
   - Adjust volumes based on importance
   - Preview to verify they sound good
3. Check statistics to ensure all critical sounds are assigned
4. Test in Play mode

### Adjusting Volumes After Playtest

If sounds are too loud/quiet during gameplay:

1. **Global adjustment**: Use AudioManager volume controls in-game
2. **Individual adjustment**: Open GameAudioSettings, tweak specific volumes
3. **Category adjustment**: Adjust all BGM or all SFX together via AudioManager

### Recommended Volume Settings

```
Background Music:
├─ Default BGM: 70-80%
├─ Menu BGM: 60-70%
└─ Game Over BGM: 80-90%

Damage Sounds:
├─ Enemy Hit: 90-100%
├─ Crit Hit: 100-110% (emphasize crits!)
├─ Player Hit: 85-95%
└─ Area Explosion: 95-100%

Event Sounds:
├─ Enemy Death: 70-80%
├─ Level Up: 90-100% (celebration!)
├─ Game Over: 90-100%
├─ Pause: 60-70%
└─ Resume: 60-70%
```

## 🔍 Troubleshooting

### Sound not playing in-game?

**Check**:
1. Is AudioClip assigned in GameAudioSettings?
2. Is volume > 0%?
3. Is Global/SFX/Music volume > 0 in AudioManager?
4. Is AudioManager in the scene?

### Sound too quiet?

**Adjust**:
1. Increase sound volume in GameAudioSettings
2. Increase SFX/Music volume in AudioManager
3. Increase Global volume in AudioManager

### Sound too loud?

**Adjust**:
1. Decrease sound volume in GameAudioSettings
2. Decrease SFX/Music volume in AudioManager

### Preview button doesn't work?

**Possible causes**:
- Unity version doesn't support AudioUtil reflection
- AudioClip is corrupted
- Editor is in Play mode (stop playback first)

**Solution**:
- Test the sound in normal Unity Inspector instead
- The sound will still work in-game even if preview fails

## 📚 Related Files

```
Audio System:
├─ GameAudioSettings.cs          # ScriptableObject data
├─ GameAudioSettingsEditor.cs    # Custom editor (this UI)
├─ AudioManager.cs                # Plays the sounds
├─ AudioPool.cs                   # AudioSource pooling
└─ GAME_AUDIO_SETTINGS_GUIDE.md  # This guide

Spell Audio (Separate):
├─ FormEffectPrefabMapping.cs     # Spell sounds (cast/impact)
├─ FormEffectPrefabMappingEditor.cs
└─ EDITOR_AUDIO_GUIDE.md
```

## 📝 Best Practices

1. **Start at 100%**: Assign clips at full volume, then adjust down if needed
2. **Playtest early**: Test sounds in-game, not just in editor preview
3. **Consistent naming**: Use clear names like `enemy_hurt.wav`, `level_up_jingle.wav`
4. **Balance categories**: BGM quieter than SFX, UI quieter than gameplay
5. **Emphasize importance**: Critical events (level up, game over) should be louder
6. **Avoid clipping**: Keep volumes under 100% if sounds are naturally loud

---

**Last Updated**: 2025-12-29
**Version**: 1.0 (Initial Custom Editor)
