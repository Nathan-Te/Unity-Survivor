# Localization Quick Reference

## 🔧 Setup Checklist

- [ ] Create `LocalizationManager` GameObject in scene
- [ ] Create 3 localization tables (UI, Stats, Combat)
- [ ] Populate tables with entries from LOCALIZATION_SETUP.md
- [ ] Assign tables to LocalizationManager component
- [ ] Test language switching works

## 📖 Common Operations

### Get Simple String

```csharp
using SurvivorGame.Localization;

string text = LocalizationHelper.GetUIString(LocalizationKeys.UI_LEVELUP_TITLE);
```

### Get Formatted String

```csharp
// "Enemies: {0}" → "Enemies: 42"
string text = LocalizationHelper.FormatEnemyCount(42);
```

### Localize Enum

```csharp
string statName = EnumLocalizer.GetStatName(StatType.MoveSpeed);
string element = EnumLocalizer.GetElementName(ElementType.Fire);
string rarity = EnumLocalizer.GetRarityName(Rarity.Legendary);
```

### Auto-Update TextMeshPro

1. Add `LocalizedTextMeshPro` component
2. Set Table Name: "UI"
3. Set Key: "LEVELUP_TITLE"
4. Done! Updates automatically on language change

### Switch Language

```csharp
LocalizationManager.Instance.SetLanguage(Language.English);
LocalizationManager.Instance.SetLanguage(Language.French);
```

## 🎨 LocalizationHelper Shortcuts

### HUD Text
```csharp
enemyCountText.text = LocalizationHelper.FormatEnemyCount(count);
killCountText.text = LocalizationHelper.FormatKillCount(count);
scoreText.text = LocalizationHelper.FormatScore(score);
comboText.text = LocalizationHelper.FormatCombo(combo);
multiplierText.text = LocalizationHelper.FormatMultiplier(multiplier);
levelText.text = LocalizationHelper.FormatLevel(level);
healthText.text = LocalizationHelper.FormatHealth(current, max);
```

### Combat Effects
```csharp
LocalizationHelper.FormatBurn(damagePerTick, duration);
LocalizationHelper.FormatChain(count);
LocalizationHelper.FormatAoE(radius);
LocalizationHelper.FormatSummon(chance);
LocalizationHelper.GetSlowLabel();
LocalizationHelper.GetHomingLabel();
```

### Stat Labels
```csharp
LocalizationHelper.GetDamageLabel();
LocalizationHelper.GetCooldownLabel();
LocalizationHelper.GetCountLabel();
LocalizationHelper.GetPierceLabel();
LocalizationHelper.GetRangeLabel();
LocalizationHelper.GetCritChanceLabel();
// ... and more (see LocalizationHelper.cs)
```

## 🔄 Making UI Scripts Language-Aware

### Template

```csharp
using SurvivorGame.Localization;

public class MyUIScript : MonoBehaviour
{
    // 1. Cache values for refresh
    private int _cachedValue = -1;

    private void Start()
    {
        // 2. Subscribe to language changes
        LocalizationManager.OnLanguageChanged += RefreshAllText;

        // Initial display
        UpdateDisplay(0);
    }

    // 3. Update method caches value and uses LocalizationHelper
    private void UpdateDisplay(int value)
    {
        _cachedValue = value; // Cache for language switch
        myText.text = LocalizationHelper.FormatEnemyCount(value);
    }

    // 4. Refresh on language change
    private void RefreshAllText()
    {
        if (_cachedValue >= 0)
            UpdateDisplay(_cachedValue);
    }

    // 5. Unsubscribe on destroy
    private void OnDestroy()
    {
        LocalizationManager.OnLanguageChanged -= RefreshAllText;
    }
}
```

## 📦 Creating LocalizedString Assets

For rune names, descriptions, enemy names:

1. Right-click in Project
2. Create → Localization → Localized String
3. Name it: `RuneName_Fireball` (descriptive)
4. Fill translations:
   - **English:** "Fireball"
   - **French:** "Boule de Feu"
5. Assign to ScriptableObject field

## 🔑 Key Naming Convention

| Category | Prefix | Example |
|----------|--------|---------|
| UI strings | `UI_` | `UI_LEVELUP_TITLE` |
| Stat names | `STAT_` | `STAT_MOVE_SPEED` |
| Elements | `ELEMENT_` | `ELEMENT_FIRE` |
| Rarities | `RARITY_` | `RARITY_LEGENDARY` |
| Combat labels | `COMBAT_` | `COMBAT_DAMAGE` |

## 📊 Table Reference

| Table Name | Contains |
|------------|----------|
| `UI` | HUD text, level-up messages, buttons |
| `Stats` | Stat type display names |
| `Combat` | Elements, rarities, combat effects |

## ⚠️ Common Mistakes

❌ **DON'T:**
```csharp
text.text = "Enemies: " + count; // Hardcoded string
text.text = $"Ennemis : {count}"; // Hardcoded French
```

✅ **DO:**
```csharp
text.text = LocalizationHelper.FormatEnemyCount(count);
```

---

❌ **DON'T:**
```csharp
text.text = statType.ToString(); // Non-localized enum
```

✅ **DO:**
```csharp
text.text = EnumLocalizer.GetStatName(statType);
```

---

❌ **DON'T:**
```csharp
// Hardcode key strings (typo risk)
GetString("UI", "LEVLUP_TITLE"); // Typo!
```

✅ **DO:**
```csharp
// Use constants (compile-time safety)
GetUIString(LocalizationKeys.UI_LEVELUP_TITLE);
```

## 🎯 Migration Priority

### Phase 1: High-Impact UI
1. PlayerHUD.cs ⭐ (See EXAMPLE_PlayerHUD_Refactored.cs)
2. LevelUpDraftController.cs
3. LevelUpInventoryController.cs
4. UpgradeCard.cs

### Phase 2: Tooltips & Details
5. RuneTooltip.cs
6. StatIconUI.cs
7. BossHealthBarUI.cs

### Phase 3: Polish
8. ModifierReplaceButton.cs
9. Any other UI scripts

## 🌍 Current Translation Status

| Language | Status | Completeness |
|----------|--------|--------------|
| English | ✅ Default | 100% |
| French | 🟡 Partial | ~60% (mixed in current code) |

**Next Steps:**
1. Complete French translations in tables
2. Migrate all UI scripts
3. Test full gameplay in both languages

## 📞 Need Help?

- Full setup guide: [LOCALIZATION_SETUP.md](LOCALIZATION_SETUP.md)
- Complete example: [EXAMPLE_PlayerHUD_Refactored.cs](EXAMPLE_PlayerHUD_Refactored.cs)
- System overview: [README.md](README.md)

---

**Remember:** Always test with both languages to ensure everything updates correctly!
