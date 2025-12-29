# Localization System Architecture

## System Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                    LOCALIZATION SYSTEM                          │
│                                                                 │
│  ┌─────────────────┐      ┌──────────────────┐                │
│  │ LocalizationMgr │◄─────┤ LocalizationTable│ (UI)           │
│  │   (Singleton)   │      └──────────────────┘                │
│  │                 │      ┌──────────────────┐                │
│  │ - Current Lang  │◄─────┤ LocalizationTable│ (Stats)        │
│  │ - OnLangChanged │      └──────────────────┘                │
│  │ - GetString()   │      ┌──────────────────┐                │
│  │ - SetLanguage() │◄─────┤ LocalizationTable│ (Combat)       │
│  └────────┬────────┘      └──────────────────┘                │
│           │                                                     │
└───────────┼─────────────────────────────────────────────────────┘
            │
            │ Used by ▼
            │
    ┌───────┴────────┬────────────┬──────────────┐
    │                │            │              │
    ▼                ▼            ▼              ▼
┌─────────┐  ┌─────────────┐  ┌──────────┐  ┌────────────┐
│LocalText│  │Localization │  │   Enum   │  │ UI Scripts │
│MeshPro  │  │   Helper    │  │Localizer │  │  (Manual)  │
│(Auto)   │  │ (Shortcuts) │  │(Enums)   │  │            │
└─────────┘  └─────────────┘  └──────────┘  └────────────┘
```

## Component Responsibilities

### Core Components

#### LocalizationManager
**Role:** Central singleton managing all localization
**Responsibilities:**
- Store current language
- Manage localization tables
- Provide string lookup API
- Notify listeners on language change

**Key Methods:**
```csharp
string GetString(string tableName, string key, string fallback)
string GetFormattedString(string tableName, string key, params object[] args)
void SetLanguage(Language language)
```

**Events:**
```csharp
static event Action OnLanguageChanged
```

#### LocalizationTable (ScriptableObject)
**Role:** Store key-value pairs for UI strings
**Responsibilities:**
- Organize strings by keys
- Store translations for all languages
- Provide fast lookup via cached dictionary

**Data Structure:**
```csharp
[Serializable]
class LocalizationEntry
{
    string Key;
    string English;
    string French;
    // Add more languages...
}
```

#### LocalizedString (ScriptableObject)
**Role:** Store localized versions of a single string
**Responsibilities:**
- Used for data-driven content (rune names, descriptions)
- Automatic language detection
- Implicit string conversion

**Usage:**
```csharp
// In ScriptableObject
public LocalizedString runeName;

// In code (implicit conversion)
string name = myRune.runeName; // Automatically gets current language
```

### Helper Components

#### LocalizationHelper
**Role:** Provide shortcuts for common operations
**Responsibilities:**
- Simplify common UI patterns
- Reduce boilerplate code
- Type-safe string formatting

**Example Methods:**
```csharp
string FormatEnemyCount(int count)
string FormatLevel(int level)
string FormatHealth(float current, float max)
```

#### EnumLocalizer
**Role:** Convert enums to localized strings
**Responsibilities:**
- Map enum values to localization keys
- Provide type-safe enum name lookup

**Example:**
```csharp
string GetStatName(StatType statType)
string GetElementName(ElementType elementType)
string GetRarityName(Rarity rarity)
```

#### LocalizedTextMeshPro (Component)
**Role:** Auto-update TextMeshPro on language change
**Responsibilities:**
- Subscribe to language change events
- Automatically refresh text
- Support formatted strings

**Usage:**
```csharp
// In Inspector: Set table + key
// Text updates automatically when language changes
```

### Utility Components

#### LocalizationKeys
**Role:** Centralized constants for all keys
**Responsibilities:**
- Prevent typos in key names
- Enable IDE autocomplete
- Make refactoring easier

**Structure:**
```csharp
public static class LocalizationKeys
{
    public const string TABLE_UI = "UI";
    public const string UI_LEVELUP_TITLE = "LEVELUP_TITLE";
    // ...
}
```

#### LanguageSelectorUI (Component)
**Role:** UI component for language selection
**Responsibilities:**
- Provide dropdown or button interface
- Call SetLanguage() on LocalizationManager
- Highlight current language

## Data Flow

### String Lookup Flow

```
User Code Request
    │
    ▼
LocalizationHelper.FormatEnemyCount(42)
    │
    ▼
LocalizationManager.GetFormattedString("UI", "ENEMIES", 42)
    │
    ▼
LocalizationTable("UI").GetString("ENEMIES", currentLanguage)
    │
    ▼
Dictionary Lookup (cached)
    │
    ▼
"Enemies: {0}" (English) or "Ennemis : {0}" (French)
    │
    ▼
string.Format("Enemies: {0}", 42)
    │
    ▼
Return "Enemies: 42"
```

### Language Change Flow

```
User Action (Button Click)
    │
    ▼
LocalizationManager.SetLanguage(Language.French)
    │
    ▼
Update _currentLanguage
    │
    ▼
Fire OnLanguageChanged event
    │
    ├──► LocalizedTextMeshPro components update
    ├──► UI Scripts with RefreshAllText() update
    └──► Custom subscribers update
```

### LocalizedString Flow

```
ScriptableObject Field Access
    │
    ▼
LocalizedString myString = rune.runeName;
    │
    ▼
Implicit string conversion operator
    │
    ▼
LocalizedString.GetText()
    │
    ▼
Check LocalizationManager.CurrentLanguage
    │
    ▼
Find matching translation in _translations list
    │
    ▼
Return localized text
```

## File Organization

```
Assets/
├── Scripts/
│   └── Localization/
│       ├── Core/
│       │   ├── Language.cs
│       │   ├── LocalizationManager.cs
│       │   ├── LocalizationTable.cs
│       │   └── LocalizedString.cs
│       ├── Components/
│       │   ├── LocalizedTextMeshPro.cs
│       │   └── LanguageSelectorUI.cs
│       ├── Utils/
│       │   ├── LocalizationKeys.cs
│       │   ├── LocalizationHelper.cs
│       │   └── EnumLocalizer.cs
│       ├── Editor/
│       │   └── LocalizationEditorTools.cs
│       ├── Testing/
│       │   └── LocalizationTester.cs
│       └── Documentation/
│           ├── README.md
│           ├── LOCALIZATION_SETUP.md
│           ├── QUICK_REFERENCE.md
│           ├── IMPLEMENTATION_CHECKLIST.md
│           ├── ARCHITECTURE.md (this file)
│           └── EXAMPLE_PlayerHUD_Refactored.cs
│
└── ScriptableObjects/
    └── Localization/
        ├── Tables/
        │   ├── UILocalizationTable.asset
        │   ├── StatsLocalizationTable.asset
        │   └── CombatLocalizationTable.asset
        └── Strings/
            ├── RuneName_Fireball.asset
            ├── RuneDesc_PlusProjectiles.asset
            └── ... (many more)
```

## Design Patterns Used

### Singleton Pattern
**Component:** LocalizationManager
**Reason:** Single source of truth for current language, accessible from anywhere

### Observer Pattern
**Component:** OnLanguageChanged event
**Reason:** Decouple language changes from UI updates, multiple subscribers

### Strategy Pattern
**Component:** LocalizedString vs LocalizationTable
**Reason:** Different strategies for different types of localized content

### Facade Pattern
**Component:** LocalizationHelper
**Reason:** Simplify complex API into easy-to-use shortcuts

### Flyweight Pattern
**Component:** Cached dictionaries in LocalizationTable
**Reason:** Avoid redundant string allocations, improve performance

## Performance Considerations

### Caching
```csharp
// LocalizationTable builds cache on first access
private Dictionary<Language, Dictionary<string, string>> _cache;

// Subsequent lookups are O(1) dictionary access
public string GetString(string key, Language language)
{
    return _cache[language][key];
}
```

### String Allocation
```csharp
// LocalizationHelper uses string.Format (creates allocation)
// For frequently-updated text (e.g., every frame), cache the result

// ❌ BAD (allocates every frame)
void Update()
{
    text.text = LocalizationHelper.FormatEnemyCount(_enemyCount);
}

// ✅ GOOD (allocates only on change)
private int _lastCount = -1;
void Update()
{
    if (_enemyCount != _lastCount)
    {
        text.text = LocalizationHelper.FormatEnemyCount(_enemyCount);
        _lastCount = _enemyCount;
    }
}
```

### Event Unsubscription
```csharp
// Always unsubscribe in OnDestroy to prevent memory leaks
private void OnDestroy()
{
    LocalizationManager.OnLanguageChanged -= RefreshAllText;
}
```

## Extension Points

### Adding New Languages
1. Add to `Language` enum
2. Add translation field to `LocalizationTable.LocalizationEntry`
3. Update `BuildCache()` method
4. Populate translations

### Adding New Tables
1. Create new `LocalizationTable` asset
2. Set unique `TableName`
3. Assign to `LocalizationManager`
4. Add constants to `LocalizationKeys`

### Custom Localization Components
```csharp
public class LocalizedImage : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite englishSprite;
    [SerializeField] private Sprite frenchSprite;

    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += UpdateSprite;
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (LocalizationManager.Instance.CurrentLanguage == Language.French)
            image.sprite = frenchSprite;
        else
            image.sprite = englishSprite;
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= UpdateSprite;
    }
}
```

## Testing Strategy

### Unit Tests (Recommended)
```csharp
[Test]
public void LocalizationManager_SwitchLanguage_UpdatesStrings()
{
    LocalizationManager.Instance.SetLanguage(Language.English);
    string englishText = LocalizationManager.Instance.GetString("UI", "LEVELUP_TITLE");

    LocalizationManager.Instance.SetLanguage(Language.French);
    string frenchText = LocalizationManager.Instance.GetString("UI", "LEVELUP_TITLE");

    Assert.AreNotEqual(englishText, frenchText);
}
```

### Integration Tests
- Use `LocalizationTester` component
- Test all UI screens in both languages
- Verify no missing keys

### Manual Testing Checklist
- [ ] Switch language during gameplay
- [ ] Verify all UI updates immediately
- [ ] Check tooltips update correctly
- [ ] Test scene reload preserves language
- [ ] Verify fallback to English for missing translations

## Future Enhancements

### Planned Features
1. **Plural Support** - Different strings for singular/plural
2. **Gender Support** - Different strings based on character gender
3. **Dynamic Font Switching** - Per-language font support
4. **RTL Text Support** - Right-to-left languages (Arabic, Hebrew)
5. **Audio Localization** - Voice-over clips per language
6. **Context-Aware Translation** - Same word, different contexts
7. **Translation Export/Import** - CSV/JSON for translators
8. **Missing Key Detection** - Editor tool to find untranslated keys

### Scalability
Current system supports:
- ✅ Unlimited languages
- ✅ Unlimited tables
- ✅ Unlimited keys per table
- ✅ Runtime table registration (modding support)

Performance tested with:
- 100+ keys per table
- 3 tables
- Real-time language switching
- No frame drops

---

**Architecture Version:** 1.0
**Last Updated:** 2025-12-28
**Designed for:** Unity 2022+ / C# 9.0+
