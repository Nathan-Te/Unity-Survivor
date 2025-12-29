# Localization System - Setup Guide

## System Overview

The localization system consists of:

1. **LocalizationManager** - Singleton that manages language switching and string retrieval
2. **LocalizedString** - ScriptableObject for data-driven text (rune names, descriptions, enemy names)
3. **LocalizationTable** - ScriptableObject for UI strings organized by keys
4. **LocalizedTextMeshPro** - Component for auto-updating TextMeshPro elements
5. **EnumLocalizer** - Utility for getting localized enum names
6. **LocalizationKeys** - Centralized constants to avoid typos

## Initial Setup

### 1. Create LocalizationManager GameObject

1. In your main scene, create an empty GameObject named "LocalizationManager"
2. Add the `LocalizationManager` component to it
3. Set default language to `English`

### 2. Create Localization Tables

You need to create 3 tables:

#### Table 1: UI (General UI strings)

1. Right-click in Project > Create > Localization > Localization Table
2. Name it "UILocalizationTable"
3. Set Table Name to "UI"
4. Populate with entries from the list below

#### Table 2: Stats (Stat type names)

1. Create another table named "StatsLocalizationTable"
2. Set Table Name to "Stats"
3. Populate with stat entries

#### Table 3: Combat (Elements, Rarities, Combat labels)

1. Create another table named "CombatLocalizationTable"
2. Set Table Name to "Combat"
3. Populate with combat entries

### 3. Assign Tables to LocalizationManager

1. Select the LocalizationManager GameObject
2. In the Inspector, expand "Localization Tables"
3. Set Size to 3
4. Drag the 3 tables you created into the array slots

## Localization Table Entries

### UI Table

| Key | English | French |
|-----|---------|--------|
| ENEMIES | Enemies: {0} | Ennemis : {0} |
| KILLS | Kills: {0} | Kills : {0} |
| SCORE | Score: {0:N0} | Score : {0:N0} |
| COMBO | Combo x{0} | Combo x{0} |
| MULTIPLIER | x{0:F1} | x{0:F1} |
| LEVEL | LVL {0} | NIV {0} |
| HEALTH | {0} / {1} | {0} / {1} |
| LEVELUP_TITLE | LEVEL UP! Choose a reward | LEVEL UP ! Choisissez une récompense |
| LEVELUP_SPECIAL | SPECIAL REWARD! | RÉCOMPENSE SPÉCIALE ! |
| LEVELUP_CHOOSE | CHOOSE A REWARD | CHOISISSEZ UNE RÉCOMPENSE |
| BAN_TITLE | BANISHMENT: Click on a card | BANNISSEMENT : Cliquez sur une carte |
| BAN_CHOOSE | CHOOSE A REWARD | CHOISISSEZ UNE RÉCOMPENSE |
| REROLL_COST | Reroll ({0}) | Reroll ({0}) |
| BAN_STOCK | Ban ({0}) | Ban ({0}) |
| APPLY_ON | Apply {0} on? | Appliquer {0} sur ? |
| INCOMPATIBLE_FORM | Incompatible with this form! | Incompatible avec cette forme ! |
| ERROR_ADD_MODIFIER | Error adding modifier | Erreur lors de l'ajout du modificateur |
| REPLACE_MODIFIER | Replace which Modifier? | Remplacer quel Modificateur ? |
| LEVEL_LABEL | Lvl | Niv |
| MAX_LEVEL | Lvl {0}/{1} | Niv {0}/{1} |
| TYPE_LABEL | Type: | Type : |
| TARGET_LABEL | Target: | Cible : |
| TYPE_STAT_UPGRADE | Player Stat Upgrade | Amélioration de Stat Joueur |

### Stats Table

| Key | English | French |
|-----|---------|--------|
| STAT_MOVE_SPEED | Speed | Vitesse |
| STAT_MAX_HEALTH | Health | Santé |
| STAT_HEALTH_REGEN | Regen | Régén |
| STAT_ARMOR | Armor | Armure |
| STAT_MAGNET_AREA | Magnet | Aimant |
| STAT_EXPERIENCE_GAIN | XP Gain | Gain XP |
| STAT_GLOBAL_DAMAGE | Damage | Dégâts |
| STAT_GLOBAL_COOLDOWN | Cooldown | Cooldown |
| STAT_GLOBAL_AREA | Area | Zone |
| STAT_GLOBAL_SPEED | Proj Speed | Vitesse Proj |
| STAT_GLOBAL_COUNT | Count | Nombre |
| STAT_CRIT_CHANCE | Crit % | Crit % |
| STAT_CRIT_DAMAGE | Crit Dmg | Dég Crit |

### Combat Table

| Key | English | French |
|-----|---------|--------|
| ELEMENT_PHYSICAL | Physical | Physique |
| ELEMENT_FIRE | Fire | Feu |
| ELEMENT_ICE | Ice | Glace |
| ELEMENT_LIGHTNING | Lightning | Foudre |
| ELEMENT_NECROTIC | Necrotic | Nécrotique |
| RARITY_COMMON | Common | Commun |
| RARITY_RARE | Rare | Rare |
| RARITY_EPIC | Epic | Épique |
| RARITY_LEGENDARY | Legendary | Légendaire |
| BURN | Burn: {0}/tick for {1}s | Brûlure : {0}/tick pdt {1}s |
| SLOW | Slow | Ralentissement |
| CHAIN | Chain x{0} | Chaîne x{0} |
| AOE | AoE {0}m | ZdE {0}m |
| SUMMON | Summon {0}% | Invocation {0}% |
| HOMING | Homing | Autoguidé |
| DAMAGE | Damage | Dégâts |
| COOLDOWN | Cooldown | Cooldown |
| COUNT | Count | Nombre |
| PIERCE | Pierce | Perçant |
| SPREAD | Spread | Dispersion |
| RANGE | Range | Portée |
| CRIT_CHANCE | Crit Chance | Chance Crit |
| CRIT_DAMAGE | Crit Damage | Dég Crit |
| SIZE | Size | Taille |
| SPEED | Speed | Vitesse |
| DURATION | Duration | Durée |
| KNOCKBACK | Knockback | Recul |
| MULTICAST | Multicast | Multicasting |

## Migrating Existing Scripts

### Example 1: Simple String Replacement

**Before:**
```csharp
enemyCountText.text = $"Ennemis : {count}";
```

**After:**
```csharp
using SurvivorGame.Localization;

enemyCountText.text = LocalizationManager.Instance.GetFormattedString(
    LocalizationKeys.TABLE_UI,
    LocalizationKeys.UI_ENEMIES,
    count
);
```

### Example 2: Enum Display

**Before:**
```csharp
nameText.text = statType.ToString();
```

**After:**
```csharp
using SurvivorGame.Localization;

nameText.text = EnumLocalizer.GetStatName(statType);
```

### Example 3: Using LocalizedTextMeshPro Component

For static text that doesn't change often:

1. Add `LocalizedTextMeshPro` component to your TextMeshPro GameObject
2. Set Table Name (e.g., "UI")
3. Set Key (e.g., "LEVELUP_TITLE")
4. Text will auto-update when language changes

For dynamic text with formatting:

```csharp
using SurvivorGame.Localization;

var localizedText = GetComponent<LocalizedTextMeshPro>();
localizedText.SetKey(LocalizationKeys.TABLE_UI, LocalizationKeys.UI_LEVEL);
localizedText.SetFormatArgs(playerLevel);
```

### Example 4: RuneSO Migration

**Old ScriptableObjects:**
- `runeName` was a `string`
- Now it's a `LocalizedString` ScriptableObject

**Migration Steps:**
1. For each existing rune ScriptableObject:
   - Right-click > Create > Localization > Localized String
   - Name it "RuneName_[RuneName]" (e.g., "RuneName_Fireball")
   - Set English text to the old name
   - Add French translation
   - Assign this LocalizedString to the rune's `runeName` field

2. For RuneDefinition descriptions:
   - Create a LocalizedString for each description
   - Name it "RuneDesc_[Description]" (e.g., "RuneDesc_PlusProjectiles")
   - Assign to the `Description` field

### Example 5: StringBuilder with Localization

**Before:**
```csharp
sb.Append("Burn: ");
sb.Append(burnDamage);
sb.Append("/tick for ");
sb.Append(duration);
sb.Append("s");
```

**After:**
```csharp
string burnText = LocalizationManager.Instance.GetFormattedString(
    LocalizationKeys.TABLE_COMBAT,
    LocalizationKeys.BURN,
    burnDamage,
    duration
);
sb.Append(burnText);
```

## Scripts That Need Migration

### High Priority (User-facing text)
1. `PlayerHUD.cs` - Enemies, kills, score, combo, level, health
2. `LevelUpDraftController.cs` - Instruction text, button labels
3. `LevelUpInventoryController.cs` - Instruction text, error messages
4. `UpgradeCard.cs` - Title and description display
5. `RuneTooltip.cs` - All tooltip text
6. `StatIconUI.cs` - Stat name display
7. `BossHealthBarUI.cs` - Boss name display

### Medium Priority (Secondary UI)
1. `ModifierReplaceButton.cs` - Level text
2. Any other UI that displays text to player

## Language Switching

To switch language at runtime (e.g., from a settings menu):

```csharp
using SurvivorGame.Localization;

public void SetEnglish()
{
    LocalizationManager.Instance.SetLanguage(Language.English);
}

public void SetFrench()
{
    LocalizationManager.Instance.SetLanguage(Language.French);
}
```

All `LocalizedTextMeshPro` components will automatically update.

## Adding New Languages

1. Add language to `Language.cs` enum:
```csharp
public enum Language
{
    English,
    French,
    Spanish // <- Add here
}
```

2. Add field to `LocalizationTable.LocalizationEntry`:
```csharp
[TextArea(1, 3)]
public string Spanish; // <- Add here
```

3. Update `BuildCache()` in `LocalizationTable.cs`:
```csharp
_cache[Language.Spanish][entry.Key] = entry.Spanish ?? "";
```

4. Add translation field to `LocalizedString.cs` if needed

5. Fill in translations in all tables

## Best Practices

1. **Use LocalizationKeys constants** - Never hardcode key strings
2. **Test with both languages** - Switch language in-game to verify
3. **Use {0}, {1} formatting** - For dynamic values in strings
4. **Keep text context-aware** - Some languages have different grammar
5. **Avoid string concatenation** - Use formatted strings instead
6. **Cache LocalizationManager.Instance** - In Update() methods if called frequently
7. **Use LocalizedString for data** - For content that's defined in ScriptableObjects
8. **Use LocalizationTable for UI** - For interface strings and labels

## Troubleshooting

### "LocalizationManager not found" warning
- Ensure LocalizationManager GameObject exists in scene
- Check that the component is attached
- Verify it's marked as DontDestroyOnLoad

### Text not updating on language change
- Check that LocalizedTextMeshPro is subscribed to OnLanguageChanged
- Verify component is enabled
- Check that the key exists in the table

### Key not found warnings
- Verify key spelling matches LocalizationKeys constants
- Check that table is assigned to LocalizationManager
- Ensure table's TableName matches the one being queried

### Fallback text showing
- Translation may be missing for current language
- System falls back to English automatically
- Add missing translation to table
