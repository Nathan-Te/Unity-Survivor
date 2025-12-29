# Audio Mapping System - Form/Effect Based

Complete guide for the audio system integrated with `FormEffectPrefabMapping`.

## 🎯 Overview

Le système audio utilise le même système de mapping que les prefabs/VFX : **`FormEffectPrefabMapping`**.

Chaque combinaison (Form, Effect) peut avoir :
- ✅ **Cast Sound** - Son joué au lancement du sort
- ✅ **Impact Sound** - Son joué à l'impact/explosion
- ✅ **Volume personnalisé** - Volume ajustable par son (0-1)

## 📁 Architecture

```
FormEffectPrefabMapping (ScriptableObject)
├─ List<PrefabEntry>
   ├─ SpellForm form
   ├─ SpellEffect effect
   ├─ GameObject prefab
   ├─ GameObject impactVfxPrefab
   ├─ Smite Timing (impactDelay, vfxSpawnDelay, lifetime)
   └─ Audio Settings ⭐ NEW
      ├─ AudioClip castSound
      ├─ float castVolume (0-1)
      ├─ AudioClip impactSound
      └─ float impactVolume (0-1)
```

## ⚙️ Configuration

### 1. Ouvrir FormEffectPrefabMapping

Ouvrez votre asset `FormEffectPrefabMapping` existant dans l'Inspector.

### 2. Configurer les Sons par Combinaison

Pour chaque entrée (Form + Effect) :

```
Example: Linear + Fire Bolt
├─ Form: LinearForm
├─ Effect: FireEffect
├─ Prefab: FireBoltPrefab
├─ Impact VFX: FireExplosionVFX
└─ Audio Settings:
   ├─ Cast Sound: fire_woosh.wav
   ├─ Cast Volume: 0.8 (80%)
   ├─ Impact Sound: fire_explosion.wav
   └─ Impact Volume: 1.0 (100%)
```

**Avantages** :
- ✅ Un seul asset à gérer (prefabs + VFX + audio)
- ✅ Volume ajustable par combinaison
- ✅ Pas besoin de scripter - tout dans l'Inspector
- ✅ Sons différents pour chaque variante (Fire Bolt vs Ice Bolt)

## 🎮 Utilisation

Le système est **entièrement automatique** ! Les sons sont joués automatiquement via :

### Cast Sounds
**Trigger** : `SpellCaster.Fire()` (ligne 257)
```csharp
AudioManager.Instance.PlaySpellCastSound(def.Form, def.Effect, position);
```

### Impact Sounds
**Trigger** : `ProjectileDamageHandler.ApplyHit()` (ligne 35)
```csharp
AudioManager.Instance.PlaySpellImpactSound(def.Form, def.Effect, position);
```

## 🔊 Volume System

Le volume final d'un son est calculé comme suit :

```
Final Volume = Global Volume × SFX Volume × Spell Volume
```

**Exemple** :
- Global Volume = 0.8 (80%)
- SFX Volume = 1.0 (100%)
- Spell Cast Volume = 0.6 (60% dans mapping)
- **Final = 0.8 × 1.0 × 0.6 = 0.48 (48%)**

Cela permet de :
- Ajuster le volume global via UI
- Ajuster tous les SFX ensemble
- Équilibrer chaque sort individuellement

## 📝 API Reference

### SpellPrefabRegistry

```csharp
// Get cast sound and volume for a Form/Effect combo
var (clip, volume) = SpellPrefabRegistry.Instance.GetCastSound(form, effect);

// Get impact sound and volume for a Form/Effect combo
var (clip, volume) = SpellPrefabRegistry.Instance.GetImpactSound(form, effect);
```

### AudioManager

```csharp
// Play cast sound (automatic volume from mapping)
AudioManager.Instance.PlaySpellCastSound(form, effect, position);

// Play impact sound (automatic volume from mapping)
AudioManager.Instance.PlaySpellImpactSound(form, effect, position);
```

## 🎯 Examples

### Example 1: Fire Bolt

```
FormEffectPrefabMapping Entry:
├─ Form: LinearForm
├─ Effect: FireEffect
├─ Cast Sound: fire_cast.wav
├─ Cast Volume: 0.7
├─ Impact Sound: fire_impact.wav
└─ Impact Volume: 1.0
```

**Behavior** :
- Player casts Fire Bolt → plays `fire_cast.wav` at 70% volume
- Bolt hits enemy → plays `fire_impact.wav` at 100% volume

### Example 2: Ice Smite

```
FormEffectPrefabMapping Entry:
├─ Form: SmiteForm
├─ Effect: IceEffect
├─ Cast Sound: ice_summon.wav
├─ Cast Volume: 0.9
├─ Impact Sound: ice_shatter.wav
└─ Impact Volume: 0.8
```

**Behavior** :
- Player casts Ice Smite → plays `ice_summon.wav` at 90% volume
- Smite explodes → plays `ice_shatter.wav` at 80% volume

### Example 3: No Sound Override

```
FormEffectPrefabMapping Entry:
├─ Form: LinearForm
├─ Effect: PhysicalEffect
├─ Cast Sound: (none)
├─ Cast Volume: 1.0
├─ Impact Sound: (none)
└─ Impact Volume: 1.0
```

**Behavior** :
- No sound plays (silent spell)
- Useful for subtle/stealthy spells

## ⚡ Performance

- ✅ **Pooling** : AudioSources are pooled (no runtime allocation)
- ✅ **Caching** : Mapping lookup is O(n) but n is small (< 100 entries typically)
- ✅ **No GC** : Tuple returns are value types (no heap allocation)

## 🔧 Workflow

### Adding a New Spell Combination

1. Open `FormEffectPrefabMapping` asset
2. Add new entry or find existing (Form + Effect)
3. Assign prefab/VFX as usual
4. Assign cast/impact sounds
5. Adjust volumes if needed (default 1.0)
6. Play test!

**That's it!** The audio system will automatically use these sounds.

### Batch Adjusting Volumes

If all Fire spells are too loud:
1. Open `FormEffectPrefabMapping`
2. Find all entries with `FireEffect`
3. Reduce `castVolume` and `impactVolume` for each
4. Save asset

No code changes needed!

## 🐛 Troubleshooting

### No sound playing?

**Check** :
1. Is `AudioManager` in scene?
2. Is `AudioPool` in scene?
3. Is `SpellPrefabRegistry` in scene?
4. Is `FormEffectPrefabMapping` assigned to SpellPrefabRegistry?
5. Does the (Form, Effect) entry exist in mapping?
6. Is `castSound` or `impactSound` assigned?
7. Is volume > 0?

### Sound too quiet?

**Adjust volumes** :
- Global Volume (AudioManager)
- SFX Volume (AudioManager)
- Spell Volume (FormEffectPrefabMapping entry)

### Wrong sound playing?

**Verify mapping** :
1. Check Form/Effect combination in mapping
2. Ensure correct AudioClip is assigned
3. Look for duplicate entries (first match wins)

## 📊 GameAudioSettings (Legacy)

`GameAudioSettings` est maintenant utilisé uniquement pour :
- ✅ Background Music (defaultBGM, menuBGM, gameOverBGM)
- ✅ Event Sounds (enemyDeathSound, levelUpSound, gameOverSound, playerHitSound)
- ✅ Generic Sounds (enemyHitSound, critHitSound, areaExplosionSound)

**Spell sounds** sont gérés par `FormEffectPrefabMapping`.

## 🎨 Best Practices

1. **Consistent Naming** : `fire_cast.wav`, `fire_impact.wav`, `ice_cast.wav`, `ice_impact.wav`
2. **Volume Balance** : Start at 1.0, adjust down if too loud
3. **One Mapping** : Use FormEffectPrefabMapping for everything (prefabs + VFX + audio)
4. **Test Early** : Assign sounds early, adjust volumes during playtesting
5. **Reuse Sounds** : Same sound can be used for multiple entries (e.g., all Physical spells use `physical_hit.wav`)

## 📚 Related Files

```
Audio System:
├─ AudioManager.cs               # Main audio controller
├─ AudioPool.cs                  # AudioSource pooling
├─ GameAudioSettings.cs          # BGM + event sounds
└─ AUDIO_MAPPING_GUIDE.md        # This file

Spell Mapping:
├─ FormEffectPrefabMapping.cs    # ⭐ Contains audio settings
├─ SpellPrefabRegistry.cs        # Exposes audio API
└─ SpellCaster.cs                # Triggers cast sounds
└─ ProjectileDamageHandler.cs    # Triggers impact sounds
```

---

**Last Updated** : 2025-12-29
**Version** : 2.0 (Integrated Mapping System)
