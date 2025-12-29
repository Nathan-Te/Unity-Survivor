# StatUpgrade Random Description System

## Overview
The StatUpgrade description system generates **randomized, localized descriptions** for stat upgrade cards, providing variety and preventing repetitive text.

## How It Works

### 1. Description Variants
Each language has **5 different variants** for describing stat upgrades:

**English:**
- "Increases your {0}"
- "Boosts {0}"
- "Improves {0}"
- "Enhances {0}"
- "Strengthens {0}"

**French:**
- "Augmente votre {0}"
- "Booste {0}"
- "Améliore {0}"
- "Renforce {0}"
- "Accroît {0}"

### 2. Format
Each description follows this pattern:
```
[Random Variant] [Stat Name]
[Colored Value]
```

**Example Output:**
```
Boosts Speed
+10.0%
```

### 3. Value Formatting
Values are automatically formatted based on stat type:

- **Percentage stats** (Speed, Damage, etc.): `+10.0%`
- **Flat stats** (Max Health, Regen): `+50.0`
- **Integer stats** (Global Count): `+2`

All values are displayed in **green** color: `<color=green>+10.0%</color>`

## Usage

### In UpgradeCard
The system is automatically used when initializing upgrade cards:

```csharp
if (data.Type == UpgradeType.StatBoost && data.TargetStat != null)
{
    descriptionText.text = StatUpgradeDescriptionGenerator.GenerateRandomDescription(
        data.TargetStat.targetStat,
        data.UpgradeDefinition.Stats.StatValue
    );
}
```

### Manual Generation
You can also generate descriptions manually:

```csharp
// Random variant (picks 1-5)
string desc = StatUpgradeDescriptionGenerator.GenerateRandomDescription(
    StatType.MoveSpeed,
    0.1f  // +10%
);

// Specific variant (for testing)
string desc = StatUpgradeDescriptionGenerator.GenerateDescription(
    StatType.MoveSpeed,
    0.1f,
    3  // Use variant 3
);
```

## Adding New Variants

To add more variants, update **both** language files:

### en.json
```json
{ "key": "STAT_DESC_VAR6", "value": "Your new variant {0}" }
```

### fr.json
```json
{ "key": "STAT_DESC_VAR6", "value": "Votre nouvelle variante {0}" }
```

Then update `VARIANT_COUNT` in `StatUpgradeDescriptionGenerator.cs`:
```csharp
private const int VARIANT_COUNT = 6; // Changed from 5
```

## Benefits

✅ **Variety**: Each card shows a different description
✅ **Localized**: Works seamlessly with language switching
✅ **Consistent**: Same formatting as other upgrade descriptions
✅ **Color-coded**: Green values for easy reading
✅ **Automatic**: No manual configuration needed

## Files

- **Generator**: `StatUpgradeDescriptionGenerator.cs`
- **Integration**: `UpgradeCard.cs` (line 30-36)
- **Translations**:
  - `Assets/Resources/Localization/en.json` (STAT_DESC_VAR1-5)
  - `Assets/Resources/Localization/fr.json` (STAT_DESC_VAR1-5)
