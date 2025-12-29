# Localization System - File Index

## 📖 Documentation (Start Here!)

| File | Purpose | When to Read |
|------|---------|--------------|
| [SIMPLE_SETUP.md](SIMPLE_SETUP.md) | **⭐ START HERE** - Simple JSON-based localization | First time setup |
| [RUNE_LOCALIZATION_GUIDE.md](RUNE_LOCALIZATION_GUIDE.md) | **⭐ RUNE SETUP** - Centralized rune localization | Managing rune translations |
| [STAT_UPGRADE_DESC_GUIDE.md](STAT_UPGRADE_DESC_GUIDE.md) | **StatUpgrade** - Random description generation | Stat upgrade variety |
| [AUTO_DESCRIPTION_GUIDE.md](AUTO_DESCRIPTION_GUIDE.md) | Auto-description system | Auto-generating rune descriptions |
| [README.md](README.md) | Complete system overview | Understanding the system |
| [QUICK_REFERENCE.md](QUICK_REFERENCE.md) | Copy-paste code examples | While coding |
| [LOCALIZATION_SETUP.md](LOCALIZATION_SETUP.md) | All table entries to create | Populating tables |
| [IMPLEMENTATION_CHECKLIST.md](IMPLEMENTATION_CHECKLIST.md) | Step-by-step migration plan | Full implementation |
| [ARCHITECTURE.md](ARCHITECTURE.md) | Technical deep-dive | Advanced understanding |

## 💻 Core System Files

### Simple System (JSON-Based)
| File | Type | Purpose |
|------|------|---------|
| [Language.cs](Language.cs) | Enum | Defines available languages |
| [SimpleLocalizationManager.cs](SimpleLocalizationManager.cs) | Singleton | Loads localization from JSON files |
| [SimpleLocalizationHelper.cs](SimpleLocalizationHelper.cs) | Static | Shortcuts for common UI strings |
| [LocalizedString.cs](LocalizedString.cs) | ScriptableObject | Stores individual localized strings (for ScriptableObjects) |

### Advanced System (Table-Based)
| File | Type | Purpose |
|------|------|---------|
| [LocalizationManager.cs](LocalizationManager.cs) | Singleton | Table-based localization manager |
| [LocalizationTable.cs](LocalizationTable.cs) | ScriptableObject | Stores UI strings by key |
| [LocalizationKeys.cs](LocalizationKeys.cs) | Constants | Centralized key definitions |

### Rune Localization
| File | Type | Purpose |
|------|------|---------|
| [RuneLocalizationData.cs](RuneLocalizationData.cs) | ScriptableObject | Centralized localization for one rune |
| [RuneDescriptionGenerator.cs](RuneDescriptionGenerator.cs) | Static | Auto-generates rune descriptions from stats |
| [StatUpgradeDescriptionGenerator.cs](StatUpgradeDescriptionGenerator.cs) | Static | Random localized descriptions for stat upgrades |
| [Editor/RuneLocalizationDataEditor.cs](Editor/RuneLocalizationDataEditor.cs) | Editor | Custom inspector for RuneLocalizationData |
| [Editor/RuneLocalizationBatchCreator.cs](Editor/RuneLocalizationBatchCreator.cs) | Editor | Batch creation tool for rune localization |

## 🛠️ Helper & Utility Files

| File | Type | Purpose |
|------|------|---------|
| [LocalizationHelper.cs](LocalizationHelper.cs) | Static | Shortcuts for common operations |
| [EnumLocalizer.cs](EnumLocalizer.cs) | Static | Convert enums to localized names |
| [LocalizedTextMeshPro.cs](LocalizedTextMeshPro.cs) | Component | Auto-update TextMeshPro on language change |
| [LanguageSelectorUI.cs](LanguageSelectorUI.cs) | Component | UI for language selection |

## 🧪 Testing & Tools

| File | Type | Purpose |
|------|------|---------|
| [LocalizationTester.cs](LocalizationTester.cs) | Component | Quick test script for verification |
| [Editor/LocalizationEditorTools.cs](Editor/LocalizationEditorTools.cs) | Editor | Unity Editor tools window |

## 📚 Examples

| File | Purpose |
|------|---------|
| [EXAMPLE_PlayerHUD_Refactored.cs](EXAMPLE_PlayerHUD_Refactored.cs) | Complete UI script migration example |

## 🗂️ File Organization

```
Localization/
├── Documentation/
│   ├── INDEX.md (this file)
│   ├── GET_STARTED.md ⭐ Start here
│   ├── README.md
│   ├── QUICK_REFERENCE.md
│   ├── LOCALIZATION_SETUP.md
│   ├── IMPLEMENTATION_CHECKLIST.md
│   └── ARCHITECTURE.md
│
├── Core/
│   ├── Language.cs
│   ├── LocalizationManager.cs
│   ├── LocalizationTable.cs
│   ├── LocalizedString.cs
│   └── LocalizationKeys.cs
│
├── Helpers/
│   ├── LocalizationHelper.cs
│   ├── EnumLocalizer.cs
│   ├── LocalizedTextMeshPro.cs
│   └── LanguageSelectorUI.cs
│
├── Examples/
│   ├── LocalizationTester.cs
│   └── EXAMPLE_PlayerHUD_Refactored.cs
│
└── Editor/
    └── LocalizationEditorTools.cs
```

## 📋 Quick Navigation

### I want to...

**...get started quickly (SIMPLE system)**
→ [SIMPLE_SETUP.md](SIMPLE_SETUP.md)

**...manage rune translations**
→ [RUNE_LOCALIZATION_GUIDE.md](RUNE_LOCALIZATION_GUIDE.md)

**...see code examples**
→ [QUICK_REFERENCE.md](QUICK_REFERENCE.md)

**...know what strings to add**
→ [LOCALIZATION_SETUP.md](LOCALIZATION_SETUP.md)

**...migrate my UI scripts**
→ [EXAMPLE_PlayerHUD_Refactored.cs](EXAMPLE_PlayerHUD_Refactored.cs)

**...understand the architecture**
→ [ARCHITECTURE.md](ARCHITECTURE.md)

**...track my progress**
→ [IMPLEMENTATION_CHECKLIST.md](IMPLEMENTATION_CHECKLIST.md)

**...test the system**
→ [LocalizationTester.cs](LocalizationTester.cs)

**...use editor tools**
→ Tools → Localization → Rune Localization Batch Creator

## 🎯 Recommended Reading Order

### For First-Time Setup
1. [GET_STARTED.md](GET_STARTED.md) - Quick 5-minute setup
2. [LOCALIZATION_SETUP.md](LOCALIZATION_SETUP.md) - Populate tables
3. [LocalizationTester.cs](LocalizationTester.cs) - Verify it works

### For Implementation
1. [QUICK_REFERENCE.md](QUICK_REFERENCE.md) - Copy-paste examples
2. [EXAMPLE_PlayerHUD_Refactored.cs](EXAMPLE_PlayerHUD_Refactored.cs) - Full example
3. [IMPLEMENTATION_CHECKLIST.md](IMPLEMENTATION_CHECKLIST.md) - Track progress

### For Understanding
1. [README.md](README.md) - System overview
2. [ARCHITECTURE.md](ARCHITECTURE.md) - Technical details

## 🔧 Unity Editor Integration

### Menu Items
- **Tools → Localization → Rune Localization Batch Creator** - Batch create rune localization assets
- **Tools → Localization Tools** - Editor utilities window (table-based system)

### Create Assets
- **Right-click → Create → Localization → Rune Localization Data** - Centralized rune localization
- **Right-click → Create → Localization → Localized String** - Individual localized string
- **Right-click → Create → Localization → Localization Table** - Table of strings (advanced)

### Components
Add to GameObjects:
- `SimpleLocalizationManager` - Simple JSON-based manager (DontDestroyOnLoad)
- `LocalizationManager` - Table-based manager (advanced)
- `LocalizedTextMeshPro` - Auto-updating text
- `LanguageSelectorUI` - Language selector UI
- `LocalizationTester` - Testing component

## 📊 File Statistics

- **Total Files:** 24
- **Core Scripts (Simple):** 3
- **Core Scripts (Advanced):** 3
- **Rune Localization:** 3
- **Helper Scripts:** 4
- **Documentation:** 8
- **Examples:** 2
- **Editor Tools:** 3

## 🌍 Language Support

**Current:**
- ✅ English (Default)
- ✅ French

**Easy to Add:**
- Spanish, German, Portuguese, etc.

See [README.md](README.md) for instructions on adding new languages.

## 🔗 External Resources

**Unity Documentation:**
- TextMeshPro: https://docs.unity3d.com/Packages/com.unity.textmeshpro@latest
- ScriptableObjects: https://docs.unity3d.com/Manual/class-ScriptableObject.html

**Related Project Files:**
- [RuneSO.cs](../Combat/Spells/Data/RuneSO.cs) - Now uses LocalizedString
- [RuneStats.cs](../Combat/Spells/Data/RuneStats.cs) - RuneDefinition uses LocalizedString
- [EnemyData.cs](../Entities/Enemies/EnemyData.cs) - Now uses LocalizedString
- [StatType.cs](../Core/StatType.cs) - Localized via EnumLocalizer
- [ElementType.cs](../Core/ElementType.cs) - Localized via EnumLocalizer

## ✅ Implementation Status

Track your progress:

- [ ] Core system setup complete
- [ ] Tables created and populated
- [ ] ScriptableObjects migrated (runes, enemies)
- [ ] UI scripts migrated
- [ ] Language selector added
- [ ] Full gameplay tested in both languages
- [ ] Team trained on system

## 📞 Need Help?

1. Check [GET_STARTED.md](GET_STARTED.md)
2. Look at [QUICK_REFERENCE.md](QUICK_REFERENCE.md)
3. Review [EXAMPLE_PlayerHUD_Refactored.cs](EXAMPLE_PlayerHUD_Refactored.cs)
4. Read [README.md](README.md) troubleshooting section

---

**System Version:** 1.0
**Created:** 2025-12-28
**Status:** Ready for use
**Supported Languages:** English, French (extensible)
