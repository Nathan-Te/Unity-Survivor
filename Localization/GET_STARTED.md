# Get Started with Localization (5 Minutes)

## What You Need to Know

✅ **Default language is English** - Write all your text in English
✅ **Easy to add translations** - Fill French column when ready
✅ **Automatic UI updates** - Switch language, everything updates instantly

## Quick Setup (Follow Along)

### Step 1: Create LocalizationManager (1 minute)

1. In your main scene, create empty GameObject
2. Name it: `LocalizationManager`
3. Add component: `LocalizationManager`
4. That's it! ✓

### Step 2: Create 3 Tables (2 minutes)

**Create each table:**
- Right-click in Project → Create → Localization → Localization Table

**Table 1:**
- Name: `UILocalizationTable`
- Field "Table Name": `UI`

**Table 2:**
- Name: `StatsLocalizationTable`
- Field "Table Name": `Stats`

**Table 3:**
- Name: `CombatLocalizationTable`
- Field "Table Name": `Combat`

### Step 3: Link Tables (1 minute)

1. Select `LocalizationManager` GameObject
2. In Inspector, find "Localization Tables" array
3. Set Size to `3`
4. Drag your 3 tables into slots 0, 1, 2

### Step 4: Add Test Entries (1 minute)

Open `UILocalizationTable`:
- Click "+" to add entry
- Key: `LEVELUP_TITLE`
- English: `LEVEL UP! Choose a reward`
- French: `LEVEL UP ! Choisissez une récompense`

Add a few more from [LOCALIZATION_SETUP.md](LOCALIZATION_SETUP.md)

### Step 5: Test It! (1 minute)

Create a test scene:

1. Add Canvas with TextMeshPro text
2. Add `LocalizationTester` component to canvas
3. Assign text reference
4. Press Play
5. Press `E` for English, `F` for French
6. Watch text update! 🎉

## Using It In Your Code

### Simple Example

```csharp
using SurvivorGame.Localization;

// Replace this:
myText.text = "Enemies: " + count;

// With this:
myText.text = LocalizationHelper.FormatEnemyCount(count);
```

That's it! When you switch language, the text updates automatically.

### Common Patterns

```csharp
using SurvivorGame.Localization;

// HUD updates
enemyCountText.text = LocalizationHelper.FormatEnemyCount(42);
killCountText.text = LocalizationHelper.FormatKillCount(100);
levelText.text = LocalizationHelper.FormatLevel(5);

// Enum names
string statName = EnumLocalizer.GetStatName(StatType.MoveSpeed);

// Custom strings
string text = LocalizationHelper.GetUIString(LocalizationKeys.UI_LEVELUP_TITLE);
```

## For ScriptableObjects (Rune Names, etc.)

### One-time setup per rune:

1. Right-click → Create → Localization → Localized String
2. Name: `RuneName_Fireball`
3. English: `Fireball`
4. French: `Boule de Feu`
5. Drag to your rune's `runeName` field

Now the rune name automatically shows in the correct language!

## Language Switching

Add to your settings menu:

```csharp
using SurvivorGame.Localization;

public void OnEnglishButton()
{
    LocalizationManager.Instance.SetLanguage(Language.English);
}

public void OnFrenchButton()
{
    LocalizationManager.Instance.SetLanguage(Language.French);
}
```

Or use the `LanguageSelectorUI` component on a dropdown.

## Making UI Auto-Update

Add this to any UI script that displays text:

```csharp
using SurvivorGame.Localization;

private void Start()
{
    // Subscribe to language changes
    LocalizationManager.OnLanguageChanged += RefreshText;
    RefreshText();
}

private void RefreshText()
{
    // Update your text here
    myText.text = LocalizationHelper.FormatEnemyCount(_enemyCount);
}

private void OnDestroy()
{
    // Don't forget to unsubscribe!
    LocalizationManager.OnLanguageChanged -= RefreshText;
}
```

## Complete Example

See [EXAMPLE_PlayerHUD_Refactored.cs](EXAMPLE_PlayerHUD_Refactored.cs) for a full working example.

## All Keys Available

See [LOCALIZATION_SETUP.md](LOCALIZATION_SETUP.md) for the complete list of keys to add to your tables.

Common ones:
- `UI_ENEMIES` - "Enemies: {0}"
- `UI_KILLS` - "Kills: {0}"
- `UI_LEVEL` - "LVL {0}"
- `UI_LEVELUP_TITLE` - "LEVEL UP! Choose a reward"

## Troubleshooting

**"LocalizationManager not found"**
→ Did you create the GameObject in Step 1?

**Text not updating on language switch**
→ Did you subscribe to `OnLanguageChanged`?

**Key not found warnings**
→ Did you add the entry to the correct table?

## Next Steps

1. ✅ Complete setup (5 minutes above)
2. 📋 Populate all table entries from [LOCALIZATION_SETUP.md](LOCALIZATION_SETUP.md)
3. 🔄 Migrate your UI scripts using [QUICK_REFERENCE.md](QUICK_REFERENCE.md)
4. 🎨 Create LocalizedString assets for all runes/enemies
5. ✨ Add language selector to settings menu
6. 🧪 Test full gameplay in both languages

## Full Documentation

- [README.md](README.md) - System overview
- [QUICK_REFERENCE.md](QUICK_REFERENCE.md) - Copy-paste examples
- [LOCALIZATION_SETUP.md](LOCALIZATION_SETUP.md) - Complete table entries
- [IMPLEMENTATION_CHECKLIST.md](IMPLEMENTATION_CHECKLIST.md) - Step-by-step migration
- [ARCHITECTURE.md](ARCHITECTURE.md) - Technical details

## Support

If you get stuck, check the example files:
- `LocalizationTester.cs` - Working test implementation
- `EXAMPLE_PlayerHUD_Refactored.cs` - Full UI script migration

---

**You're ready to go!** Start with the 5-minute setup above, then gradually migrate your UI scripts. 🚀
