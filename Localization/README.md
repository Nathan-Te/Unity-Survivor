# Localization System for Survivor Game

A complete, data-driven localization system for Unity that supports multiple languages with automatic UI updates.

## 📁 System Components

| File | Purpose |
|------|---------|
| `Language.cs` | Enum defining available languages |
| `LocalizationManager.cs` | Singleton managing language switching and string retrieval |
| `LocalizationTable.cs` | ScriptableObject storing UI strings by key |
| `LocalizedString.cs` | ScriptableObject for data-driven text (rune names, descriptions) |
| `LocalizedTextMeshPro.cs` | Component for auto-updating TextMeshPro elements |
| `LocalizationKeys.cs` | Centralized constants for all localization keys |
| `EnumLocalizer.cs` | Utility for getting localized enum names |
| `LocalizationHelper.cs` | Shortcuts for common localization operations |
| `LanguageSelectorUI.cs` | Example UI component for language selection |

## 🚀 Quick Start

### 1. Setup LocalizationManager

1. Create an empty GameObject in your scene: `LocalizationManager`
2. Add the `LocalizationManager` component
3. Set **Default Language** to `English`
4. This GameObject is marked as **DontDestroyOnLoad** (persists across scenes)

### 2. Create Localization Tables

Create 3 ScriptableObjects:

**Right-click in Project** → **Create** → **Localization** → **Localization Table**

#### Table 1: UILocalizationTable
- **Table Name:** `UI`
- Contains: Level-up messages, button labels, HUD text

#### Table 2: StatsLocalizationTable
- **Table Name:** `Stats`
- Contains: Stat type names (Speed, Health, Damage, etc.)

#### Table 3: CombatLocalizationTable
- **Table Name:** `Combat`
- Contains: Elements, rarities, combat labels

### 3. Populate Tables

See [LOCALIZATION_SETUP.md](LOCALIZATION_SETUP.md) for the complete list of entries.

### 4. Assign Tables to Manager

Select the `LocalizationManager` GameObject:
1. Expand **Localization Tables** array
2. Set **Size** to `3`
3. Drag your 3 tables into the slots

## 💡 Usage Examples

### Simple String Lookup

```csharp
using SurvivorGame.Localization;

string text = LocalizationManager.Instance.GetString(
    LocalizationKeys.TABLE_UI,
    LocalizationKeys.UI_LEVELUP_TITLE
);
```

### Formatted Strings (with parameters)

```csharp
using SurvivorGame.Localization;

// Example: "Enemies: {0}" → "Enemies: 42"
string text = LocalizationManager.Instance.GetFormattedString(
    LocalizationKeys.TABLE_UI,
    LocalizationKeys.UI_ENEMIES,
    42
);
```

### Using LocalizationHelper (Recommended)

```csharp
using SurvivorGame.Localization;

enemyCountText.text = LocalizationHelper.FormatEnemyCount(count);
killCountText.text = LocalizationHelper.FormatKillCount(count);
scoreText.text = LocalizationHelper.FormatScore(score);
levelText.text = LocalizationHelper.FormatLevel(level);
```

### Enum Localization

```csharp
using SurvivorGame.Localization;

// StatType enum → Localized name
string statName = EnumLocalizer.GetStatName(StatType.MoveSpeed);
// Returns: "Speed" (English) or "Vitesse" (French)

string elementName = EnumLocalizer.GetElementName(ElementType.Fire);
// Returns: "Fire" (English) or "Feu" (French)

string rarityName = EnumLocalizer.GetRarityName(Rarity.Legendary);
// Returns: "Legendary" (English) or "Légendaire" (French)
```

### Auto-Updating UI Components

For static text that updates on language change:

1. Add `LocalizedTextMeshPro` component to your TextMeshPro object
2. Set **Table Name** (e.g., "UI")
3. Set **Key** (e.g., "LEVELUP_TITLE")
4. Text automatically updates when language changes!

For dynamic text with parameters:

```csharp
using SurvivorGame.Localization;

var localizedText = GetComponent<LocalizedTextMeshPro>();
localizedText.SetKey(LocalizationKeys.TABLE_UI, LocalizationKeys.UI_LEVEL);
localizedText.SetFormatArgs(playerLevel); // Updates {0} placeholder
```

### LocalizedString for Data

For rune names, descriptions, enemy names:

1. **Right-click** → **Create** → **Localization** → **Localized String**
2. Name it descriptively (e.g., "RuneName_Fireball")
3. Fill in **English** and **French** translations
4. Assign to your ScriptableObject's field

**Updated ScriptableObjects:**
- `RuneSO.runeName` → Now `LocalizedString` (was `string`)
- `RuneDefinition.Description` → Now `LocalizedString` (was `string`)
- `EnemyData.enemyName` → Now `LocalizedString` (was `string`)

**Usage in code:**

```csharp
// LocalizedString has implicit conversion to string
string name = myRune.Data.runeName; // Automatically gets current language
```

## 🔄 Language Switching

Add a language selector to your settings menu:

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

Or use the provided `LanguageSelectorUI` component with a dropdown/buttons.

## 📝 Migrating Existing Scripts

See [EXAMPLE_PlayerHUD_Refactored.cs](EXAMPLE_PlayerHUD_Refactored.cs) for a complete example.

**Quick migration checklist:**

1. ✅ Add `using SurvivorGame.Localization;`
2. ✅ Replace hardcoded strings with `LocalizationHelper` calls
3. ✅ Subscribe to `LocalizationManager.OnLanguageChanged` in `Start()`
4. ✅ Add a `RefreshAllText()` method to update on language change
5. ✅ Unsubscribe in `OnDestroy()`
6. ✅ Cache values needed for refresh

**Before:**
```csharp
enemyCountText.text = $"Ennemis : {count}";
```

**After:**
```csharp
enemyCountText.text = LocalizationHelper.FormatEnemyCount(count);
```

## 🌍 Adding New Languages

1. Add to `Language.cs` enum:
```csharp
public enum Language
{
    English,
    French,
    Spanish // ← Add here
}
```

2. Add translation field to `LocalizationTable.LocalizationEntry`:
```csharp
[TextArea(1, 3)]
public string Spanish;
```

3. Update `BuildCache()` in `LocalizationTable.cs`:
```csharp
_cache[Language.Spanish][entry.Key] = entry.Spanish ?? "";
```

4. Add translations to all 3 tables

5. Update language selector UI

## ✨ Features

- **Automatic UI Updates** - All text refreshes when language changes
- **Zero Allocations** - Uses StringBuilder and caching for performance
- **Data-Driven** - ScriptableObjects for rune/enemy names
- **Type-Safe** - Centralized keys prevent typos
- **Fallback System** - Falls back to English if translation missing
- **Easy to Extend** - Add languages without code changes
- **Editor-Friendly** - All setup via Unity Inspector

## 📚 Documentation

- [LOCALIZATION_SETUP.md](LOCALIZATION_SETUP.md) - Complete setup guide with all table entries
- [EXAMPLE_PlayerHUD_Refactored.cs](EXAMPLE_PlayerHUD_Refactored.cs) - Migration example

## 🎯 Scripts to Migrate

### High Priority (User-visible text)
- ✅ [PlayerHUD.cs](../UI/HUD/PlayerHUD.cs) - See example
- [ ] [LevelUpDraftController.cs](../Progression/LevelUpDraftController.cs)
- [ ] [LevelUpInventoryController.cs](../Progression/LevelUpInventoryController.cs)
- [ ] [UpgradeCard.cs](../UI/Menus/UpgradeCard.cs)
- [ ] [RuneTooltip.cs](../UI/Menus/RuneTooltip.cs)
- [ ] [StatIconUI.cs](../UI/HUD/StatIconUI.cs)
- [ ] [BossHealthBarUI.cs](../UI/HUD/BossHealthBarUI.cs)

### Medium Priority
- [ ] [ModifierReplaceButton.cs](../UI/Menus/ModifierReplaceButton.cs)

## 🛠️ Best Practices

1. **Always use `LocalizationKeys` constants** - Never hardcode key strings
2. **Use `LocalizationHelper`** - Provides shortcuts for common operations
3. **Test both languages** - Switch in-game to verify all text updates
4. **Use `{0}` placeholders** - For dynamic values in format strings
5. **Avoid concatenation** - Use formatted strings instead of `str1 + str2`
6. **Cache LocalizationManager.Instance** - If accessing frequently in Update()
7. **Subscribe to OnLanguageChanged** - For UI that needs to refresh

## ❓ Troubleshooting

**"LocalizationManager not found"**
- Ensure GameObject exists in scene with component attached
- Check DontDestroyOnLoad is working

**Text not updating on language change**
- Verify subscription to `OnLanguageChanged` event
- Check component is enabled
- Ensure key exists in table

**Key not found warnings**
- Double-check key spelling vs `LocalizationKeys` constants
- Verify table is assigned to LocalizationManager
- Check table's `TableName` matches query

**Fallback text showing**
- Translation missing for current language
- System auto-falls back to English
- Add missing translation to table

## 📦 File Structure

```
Assets/Scripts/Localization/
├── Language.cs                         # Language enum
├── LocalizationManager.cs              # Core manager (singleton)
├── LocalizationTable.cs                # ScriptableObject for UI strings
├── LocalizedString.cs                  # ScriptableObject for data text
├── LocalizedTextMeshPro.cs             # Auto-updating component
├── LocalizationKeys.cs                 # Centralized key constants
├── EnumLocalizer.cs                    # Enum → localized name
├── LocalizationHelper.cs               # Utility shortcuts
├── LanguageSelectorUI.cs               # Example language selector
├── README.md                           # This file
├── LOCALIZATION_SETUP.md               # Detailed setup guide
└── EXAMPLE_PlayerHUD_Refactored.cs     # Migration example
```

---

**Created for:** Survivor Game
**Version:** 1.0
**Default Language:** English
**Supported Languages:** English, French (extensible)
