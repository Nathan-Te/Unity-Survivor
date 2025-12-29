# Automatic Description Generation Guide

## Overview

The `RuneDescriptionGenerator` automatically creates localized descriptions for rune upgrades based on their `RuneStats`. **No more manual description writing!**

## Key Principle

**Only stats > 0 are displayed**

This creates clean, focused descriptions that show exactly what the upgrade does, without cluttering the UI with zero values.

## How It Works

### Automatic Generation

```csharp
using SurvivorGame.Localization;

// Simple generation
string description = RuneDescriptionGenerator.GenerateDescription(runeDefinition);

// Compact (single-line) for cards
string compact = RuneDescriptionGenerator.GenerateCompactDescription(runeDefinition);

// Rich text (with colors)
string rich = RuneDescriptionGenerator.GenerateRichDescription(runeDefinition);
```

### Example Output

Given a `RuneDefinition` with these stats:
```csharp
RuneStats {
    DamageMult = 0.20f,      // +20% Damage
    FlatCount = 2,           // +2 Projectiles
    FlatCritChance = 0.10f   // +10% Crit Chance
    // All other stats = 0 (NOT DISPLAYED)
}
```

**Generated Description** (English):
```
+20% Damage
+2 Count
+10% Crit Chance
```

**Generated Description** (French):
```
+20% Dégâts
+2 Nombre de projectiles
+10% Chance Crit
```

## Stat Categories

### Multiplicative Bonuses (Displayed as %)
- `DamageMult` → "Damage"
- `CooldownMult` → "Cooldown"
- `SizeMult` → "Size"
- `SpeedMult` → "Speed"
- `DurationMult` → "Duration"

### Flat Bonuses (Displayed as +X)
- `FlatCount` → "Count"
- `FlatPierce` → "Pierce"
- `FlatSpread` → "Spread°"
- `FlatRange` → "Range m"
- `FlatKnockback` → "Knockback"
- `FlatChainCount` → "Chain"
- `FlatMulticast` → "Multicast"

### Special Bonuses
- `FlatCooldown` → "Cooldown s" (can be negative = reduction)
- `FlatBurnDamage` → "Burn Damage"
- `FlatBurnDuration` → "Burn Duration s"
- `FlatCritChance` → "Crit Chance %" (gold color)
- `FlatCritDamage` → "Crit Damage %" (gold color)

### Stat Upgrades (StatUpgradeSO)
**NOT auto-generated** - StatUpgrade runes must use their `Description` field manually.
- Reason: Requires `StatUpgradeSO.targetStat` which is not available in `RuneStats`
- `StatValue` is stored in `RuneStats`, but `TargetStat` is in `StatUpgradeSO`
- Use `LocalizedString` for `Description` field to manually describe stat upgrades

## Integration Points

### 1. Upgrade Cards (Already Integrated!)

`UpgradeCard.cs` now auto-generates descriptions:

```csharp
// In UpgradeCard.Initialize()
if (data.UpgradeDefinition != null)
{
    descriptionText.text = RuneDescriptionGenerator.GenerateDescription(data.UpgradeDefinition);
}
```

### 2. Manual Usage

```csharp
using SurvivorGame.Localization;

// Basic
RuneDefinition def = ...;
string desc = RuneDescriptionGenerator.GenerateDescription(def);
myText.text = desc;

// Compact (for small cards)
string compact = RuneDescriptionGenerator.GenerateCompactDescription(def);
// Output: "+20% Damage, +2 Count, +10% Crit Chance"

// Rich (with color coding)
string rich = RuneDescriptionGenerator.GenerateRichDescription(def);
// Output with <color> tags
```

## Benefits

### ✅ No Manual Writing
- Stats automatically become descriptions
- Change stats → description updates instantly
- No need to maintain separate Description fields

### ✅ Always Localized
- Uses `SimpleLocalizationHelper` for all labels
- Automatically translates to current language
- Add new language → descriptions auto-translate

### ✅ Clean & Focused
- Only shows stats > 0
- No clutter from unused stats
- Easy to read at a glance

### ✅ Consistent Format
- Same format everywhere
- Predictable structure
- Professional appearance

## Advanced Features

### Color-Coded Descriptions

`GenerateRichDescription()` adds color coding:

- **Green**: Positive bonuses (damage, count, etc.)
- **Red**: Negative effects (if any)
- **Gold**: Crit stats
- **Orange**: Burn effects (in `GenerateDescription`)

### Compact Descriptions

For small UI cards:
```csharp
string compact = RuneDescriptionGenerator.GenerateCompactDescription(definition);
// Output: "+20% Damage, +2 Count, +10% Crit"
```

### Fallback Handling

If a `RuneDefinition` has no stats:
```csharp
// Returns localized "Special upgrade" message
string desc = RuneDescriptionGenerator.GenerateDescription(emptyDef);
```

## Migration Guide

### Before (Manual Descriptions)

```csharp
// Each RuneDefinition needed a manual Description field
RuneDefinition {
    Stats = ...,
    Description = "Augmente les dégâts de 20% et ajoute 2 projectiles"  // ❌ Manual
}
```

### After (Auto-Generated)

```csharp
// Just set the stats, description generates automatically
RuneDefinition {
    Stats = new RuneStats {
        DamageMult = 0.20f,
        FlatCount = 2
    }
    // ✅ Description auto-generated from stats!
}
```

## Code Examples

### Example 1: Simple Damage Boost

```csharp
RuneDefinition damageBoost = new RuneDefinition {
    Stats = new RuneStats { DamageMult = 0.25f }
};

string desc = RuneDescriptionGenerator.GenerateDescription(damageBoost);
// English: "+25% Damage"
// French: "+25% Dégâts"
```

### Example 2: Multi-Stat Upgrade

```csharp
RuneDefinition multiStat = new RuneDefinition {
    Stats = new RuneStats {
        DamageMult = 0.15f,
        FlatCount = 1,
        FlatPierce = 2,
        FlatCritChance = 0.05f
    }
};

string desc = RuneDescriptionGenerator.GenerateDescription(multiStat);
// English:
// +15% Damage
// +1 Count
// +2 Pierce
// +5% Crit Chance
```

### Example 3: Cooldown Reduction

```csharp
RuneDefinition cooldownRed = new RuneDefinition {
    Stats = new RuneStats {
        CooldownMult = -0.20f,  // -20% = faster cooldown
        FlatCount = 3
    }
};

string desc = RuneDescriptionGenerator.GenerateDescription(cooldownRed);
// English:
// -20% Cooldown
// +3 Count
```

### Example 4: Stat Upgrade (Not Auto-Generated)

```csharp
// StatUpgrade runes are NOT auto-generated
// They must use the Description field with a LocalizedString

StatUpgradeSO statUpgrade = CreateInstance<StatUpgradeSO>();
statUpgrade.targetStat = StatType.MoveSpeed;
statUpgrade.commonUpgrades[0].Stats.StatValue = 0.10f;
statUpgrade.commonUpgrades[0].Description = myLocalizedString; // Manual description required

// The auto-generator will skip StatValue since TargetStat isn't accessible
// You must set the Description field manually for StatUpgrade runes
```

## Best Practices

### 1. Use Stats, Not Descriptions (Except for StatUpgrade)
```csharp
// ✅ GOOD - Data-driven (for combat stats)
RuneDefinition upgrade = new RuneDefinition {
    Stats = new RuneStats { DamageMult = 0.30f }
};

// ❌ BAD - Manual (for combat stats)
RuneDefinition upgrade = new RuneDefinition {
    Description = "+30% Damage"  // Don't do this!
};

// ✅ EXCEPTION - StatUpgrade MUST use Description
StatUpgradeSO statUpgrade = CreateInstance<StatUpgradeSO>();
statUpgrade.targetStat = StatType.MoveSpeed;
statUpgrade.commonUpgrades[0].Stats.StatValue = 0.10f;
statUpgrade.commonUpgrades[0].Description = myLocalizedString; // Required!
```

### 2. Let Zero Values Hide
```csharp
// ✅ GOOD - Only set what you need
RuneStats stats = new RuneStats {
    DamageMult = 0.20f
    // All other fields default to 0 and won't show
};

// ❌ BAD - Explicitly setting zeros
RuneStats stats = new RuneStats {
    DamageMult = 0.20f,
    FlatCount = 0,      // Unnecessary
    FlatPierce = 0,     // Clutters code
    // ...
};
```

### 3. Trust the Generator
```csharp
// ✅ GOOD - Trust auto-generation
descriptionText.text = RuneDescriptionGenerator.GenerateDescription(def);

// ❌ BAD - Mixing manual and auto
if (def.DamageMult > 0) {
    descriptionText.text = $"+{def.DamageMult * 100}% Damage";  // Don't!
}
```

## Localization Integration

The generator uses `SimpleLocalizationHelper` for all labels:

```csharp
// Labels automatically localized
SimpleLocalizationHelper.GetDamageLabel()      // "Damage" or "Dégâts"
SimpleLocalizationHelper.GetCooldownLabel()   // "Cooldown" or "Cooldown"
SimpleLocalizationHelper.GetCritChanceLabel()  // "Crit Chance" or "Chance Crit"
// ... etc
```

Add a new language:
1. Create `Assets/Resources/Localization/es.json` (for Spanish)
2. Add translations for all `LABEL_*` keys
3. Descriptions auto-translate!

## Troubleshooting

### Description is Empty

**Problem**: Generated description is empty or shows "Special upgrade"

**Solution**: Check if all stats in `RuneStats` are 0. The generator only displays stats > 0.

```csharp
// This will show "Special upgrade"
RuneStats allZeros = new RuneStats();  // All fields = 0

// Add at least one stat > 0
RuneStats withStats = new RuneStats { DamageMult = 0.10f };
```

### Wrong Language

**Problem**: Descriptions show in wrong language

**Solution**: Check `SimpleLocalizationManager.CurrentLanguage`

```csharp
// Change language
SimpleLocalizationManager.Instance.SetLanguage(Language.French);

// Regenerate descriptions
descriptionText.text = RuneDescriptionGenerator.GenerateDescription(def);
```

### Missing Label

**Problem**: Error or key name appears in description (e.g., "LABEL_DAMAGE")

**Solution**: Add missing key to JSON files

```json
// In en.json and fr.json
{ "key": "LABEL_DAMAGE", "value": "Damage" }  // en
{ "key": "LABEL_DAMAGE", "value": "Dégâts" }  // fr
```

## Summary

The `RuneDescriptionGenerator` provides:

- ✅ **Automatic** description generation from stats
- ✅ **Localized** using current language
- ✅ **Clean** output (only stats > 0)
- ✅ **Consistent** formatting
- ✅ **Easy to use** - one function call
- ✅ **Backwards compatible** - still supports manual descriptions

**No more writing descriptions manually!** Just set the stats and let the system handle the rest.
