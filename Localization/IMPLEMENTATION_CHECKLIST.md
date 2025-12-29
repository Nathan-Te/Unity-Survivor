# Localization Implementation Checklist

Use this checklist to track your progress implementing the localization system.

## Phase 1: Initial Setup ✅

### Unity Setup
- [ ] Open Unity project
- [ ] Verify all localization scripts are imported (no compile errors)
- [ ] Check that namespace `SurvivorGame.Localization` is recognized

### Create LocalizationManager
- [ ] Create empty GameObject in main scene
- [ ] Name it "LocalizationManager"
- [ ] Add `LocalizationManager` component
- [ ] Set Default Language to `English`
- [ ] Verify DontDestroyOnLoad is enabled

### Create ScriptableObjects Directory
- [ ] Create folder: `Assets/ScriptableObjects/Localization`
- [ ] Create subfolder: `Tables`
- [ ] Create subfolder: `Strings`

## Phase 2: Create Localization Tables 📋

### Create UI Table
- [ ] Right-click → Create → Localization → Localization Table
- [ ] Name: `UILocalizationTable`
- [ ] Save in `ScriptableObjects/Localization/Tables/`
- [ ] Set Table Name field to: `UI`
- [ ] Populate with UI entries (see LOCALIZATION_SETUP.md)
  - [ ] ENEMIES
  - [ ] KILLS
  - [ ] SCORE
  - [ ] COMBO
  - [ ] MULTIPLIER
  - [ ] LEVEL
  - [ ] HEALTH
  - [ ] LEVELUP_TITLE
  - [ ] LEVELUP_SPECIAL
  - [ ] LEVELUP_CHOOSE
  - [ ] BAN_TITLE
  - [ ] BAN_CHOOSE
  - [ ] REROLL_COST
  - [ ] BAN_STOCK
  - [ ] APPLY_ON
  - [ ] INCOMPATIBLE_FORM
  - [ ] ERROR_ADD_MODIFIER
  - [ ] REPLACE_MODIFIER
  - [ ] LEVEL_LABEL
  - [ ] MAX_LEVEL
  - [ ] TYPE_LABEL
  - [ ] TARGET_LABEL
  - [ ] TYPE_STAT_UPGRADE

### Create Stats Table
- [ ] Create new Localization Table
- [ ] Name: `StatsLocalizationTable`
- [ ] Save in `ScriptableObjects/Localization/Tables/`
- [ ] Set Table Name field to: `Stats`
- [ ] Populate with stat entries
  - [ ] STAT_MOVE_SPEED
  - [ ] STAT_MAX_HEALTH
  - [ ] STAT_HEALTH_REGEN
  - [ ] STAT_ARMOR
  - [ ] STAT_MAGNET_AREA
  - [ ] STAT_EXPERIENCE_GAIN
  - [ ] STAT_GLOBAL_DAMAGE
  - [ ] STAT_GLOBAL_COOLDOWN
  - [ ] STAT_GLOBAL_AREA
  - [ ] STAT_GLOBAL_SPEED
  - [ ] STAT_GLOBAL_COUNT
  - [ ] STAT_CRIT_CHANCE
  - [ ] STAT_CRIT_DAMAGE

### Create Combat Table
- [ ] Create new Localization Table
- [ ] Name: `CombatLocalizationTable`
- [ ] Save in `ScriptableObjects/Localization/Tables/`
- [ ] Set Table Name field to: `Combat`
- [ ] Populate with combat entries
  - [ ] Elements: ELEMENT_PHYSICAL, ELEMENT_FIRE, ELEMENT_ICE, ELEMENT_LIGHTNING, ELEMENT_NECROTIC
  - [ ] Rarities: RARITY_COMMON, RARITY_RARE, RARITY_EPIC, RARITY_LEGENDARY
  - [ ] Effects: BURN, SLOW, CHAIN, AOE, SUMMON, HOMING
  - [ ] Stats: DAMAGE, COOLDOWN, COUNT, PIERCE, SPREAD, RANGE, CRIT_CHANCE, CRIT_DAMAGE, SIZE, SPEED, DURATION, KNOCKBACK, MULTICAST

### Link Tables to Manager
- [ ] Select LocalizationManager GameObject
- [ ] Expand "Localization Tables" array
- [ ] Set Size to `3`
- [ ] Assign `UILocalizationTable` to element 0
- [ ] Assign `StatsLocalizationTable` to element 1
- [ ] Assign `CombatLocalizationTable` to element 2
- [ ] Save scene

## Phase 3: Test Basic Functionality 🧪

### Test in Play Mode
- [ ] Enter Play Mode
- [ ] Check Console for errors
- [ ] Verify LocalizationManager.Instance is not null
- [ ] Test getting a string (use Debug.Log):
  ```csharp
  Debug.Log(LocalizationManager.Instance.GetString("UI", "LEVELUP_TITLE"));
  ```

### Test Language Switching
- [ ] Add test script or use Console window
- [ ] Switch to French: `LocalizationManager.Instance.SetLanguage(Language.French);`
- [ ] Get same string again - should return French version
- [ ] Switch back to English and verify

## Phase 4: Migrate Existing Content 🔄

### Migrate RuneSO Assets
For each existing Rune ScriptableObject:
- [ ] Create LocalizedString for rune name
  - [ ] Right-click → Create → Localization → Localized String
  - [ ] Name: `RuneName_[RuneName]`
  - [ ] Set English text
  - [ ] Set French text
  - [ ] Assign to rune's `runeName` field
- [ ] Create LocalizedString for each RuneDefinition description
  - [ ] Name: `RuneDesc_[Description]`
  - [ ] Set English text
  - [ ] Set French text
  - [ ] Assign to RuneDefinition's `Description` field

**Estimated Runes to Migrate:** ~20-30 runes × ~4 upgrades each = ~80-120 LocalizedStrings

### Migrate EnemyData Assets
For each existing Enemy ScriptableObject:
- [ ] Create LocalizedString for enemy name
  - [ ] Name: `EnemyName_[EnemyName]`
  - [ ] Set English text
  - [ ] Set French text
  - [ ] Assign to enemy's `enemyName` field

**Estimated Enemies to Migrate:** ~5-10 enemies

## Phase 5: Migrate UI Scripts 🎨

### High Priority Scripts

#### PlayerHUD.cs
- [ ] Open file
- [ ] Add `using SurvivorGame.Localization;`
- [ ] Add caching fields (see EXAMPLE_PlayerHUD_Refactored.cs)
- [ ] Subscribe to `LocalizationManager.OnLanguageChanged` in Start()
- [ ] Add `RefreshAllText()` method
- [ ] Replace all hardcoded strings:
  - [ ] `"Ennemis : {count}"` → `LocalizationHelper.FormatEnemyCount(count)`
  - [ ] `"Kills : {count}"` → `LocalizationHelper.FormatKillCount(count)`
  - [ ] `"Score : {score:N0}"` → `LocalizationHelper.FormatScore(score)`
  - [ ] `"Combo x{combo}"` → `LocalizationHelper.FormatCombo(combo)`
  - [ ] `"x{multiplier:F1}"` → `LocalizationHelper.FormatMultiplier(multiplier)`
  - [ ] `"LVL {level}"` → `LocalizationHelper.FormatLevel(level)`
  - [ ] `"{current} / {max}"` → `LocalizationHelper.FormatHealth(current, max)`
- [ ] Unsubscribe in OnDestroy()
- [ ] Test in Play Mode

#### StatIconUI.cs
- [ ] Open file
- [ ] Add `using SurvivorGame.Localization;`
- [ ] Replace switch statement with `EnumLocalizer.GetStatName(statType)`
- [ ] Subscribe to language changes if needed
- [ ] Test in Play Mode

#### BossHealthBarUI.cs
- [ ] Open file
- [ ] Verify boss name uses `boss.Data.enemyName` (already LocalizedString)
- [ ] Subscribe to language changes to refresh boss name
- [ ] Test with boss spawn

#### LevelUpDraftController.cs
- [ ] Open file
- [ ] Add `using SurvivorGame.Localization;`
- [ ] Replace all hardcoded French strings:
  - [ ] `"RÉCOMPENSE SPÉCIALE !"` → `LocalizationHelper.GetUIString(LocalizationKeys.UI_LEVELUP_SPECIAL)`
  - [ ] `"LEVEL UP ! Choisissez une récompense"` → `LocalizationHelper.GetUIString(LocalizationKeys.UI_LEVELUP_TITLE)`
  - [ ] `"BANNISSEMENT : Cliquez sur une carte"` → `LocalizationHelper.GetUIString(LocalizationKeys.UI_BAN_TITLE)`
  - [ ] `"CHOISISSEZ UNE RÉCOMPENSE"` → `LocalizationHelper.GetUIString(LocalizationKeys.UI_LEVELUP_CHOOSE)`
  - [ ] `"Reroll ({availableRerolls})"` → `LocalizationHelper.FormatRerollCost(availableRerolls)`
  - [ ] `"Ban ({availableBans})"` → `LocalizationHelper.FormatBanStock(availableBans)`
- [ ] Subscribe to language changes
- [ ] Test level-up flow

#### LevelUpInventoryController.cs
- [ ] Open file
- [ ] Add `using SurvivorGame.Localization;`
- [ ] Replace all hardcoded French strings:
  - [ ] `"Appliquer {pendingUpgrade.Name} sur ?"` → Use LocalizationManager with `UI_APPLY_ON` key
  - [ ] `"Incompatible avec cette forme !"` → `LocalizationHelper.GetUIString(LocalizationKeys.UI_INCOMPATIBLE_FORM)`
  - [ ] `"Erreur lors de l'ajout du modificateur"` → `LocalizationHelper.GetUIString(LocalizationKeys.UI_ERROR_ADD_MODIFIER)`
  - [ ] `"Remplacer quel Modificateur ?"` → `LocalizationHelper.GetUIString(LocalizationKeys.UI_REPLACE_MODIFIER)`
- [ ] Subscribe to language changes
- [ ] Test modifier application flow

#### UpgradeCard.cs
- [ ] Open file
- [ ] Verify uses `rune.runeName` (already LocalizedString)
- [ ] Verify uses `upgrade.Description` (already LocalizedString)
- [ ] Replace rarity display with `EnumLocalizer.GetRarityName(rarity)`
- [ ] Subscribe to language changes
- [ ] Test upgrade card display

#### RuneTooltip.cs
- [ ] Open file
- [ ] Add `using SurvivorGame.Localization;`
- [ ] Replace hardcoded labels:
  - [ ] `"LvL {level}/{maxLevel}"` → `LocalizationHelper.FormatLevelWithMax(level, maxLevel)`
  - [ ] `"Type: Player Stat Upgrade"` → `LocalizationHelper.GetUIString(LocalizationKeys.UI_TYPE_STAT_UPGRADE)`
  - [ ] Element names → `EnumLocalizer.GetElementName(element)`
  - [ ] Stat labels → Use `LocalizationHelper.GetDamageLabel()`, etc.
  - [ ] Effect descriptions → Use localization keys
- [ ] Subscribe to language changes
- [ ] Clear cache on language change
- [ ] Test tooltip display

### Medium Priority Scripts

#### ModifierReplaceButton.cs
- [ ] Open file
- [ ] Add `using SurvivorGame.Localization;`
- [ ] Replace `"Lvl {modifierRune.Level}"` → `LocalizationHelper.FormatLevel(modifierRune.Level)`
- [ ] Subscribe to language changes
- [ ] Test modifier replacement UI

## Phase 6: Add Language Selector 🌍

### Create Settings Menu (if not exists)
- [ ] Create Settings UI canvas
- [ ] Add Settings button to main menu

### Add Language Selector
Option A: Dropdown
- [ ] Add TMP_Dropdown to settings UI
- [ ] Add `LanguageSelectorUI` component
- [ ] Assign dropdown reference
- [ ] Test dropdown functionality

Option B: Buttons
- [ ] Add "English" button
- [ ] Add "Français" button
- [ ] Add `LanguageSelectorUI` component
- [ ] Assign button references
- [ ] Test button functionality

### Test Language Switching
- [ ] Enter Play Mode
- [ ] Open settings
- [ ] Switch to French
- [ ] Verify all UI updates
- [ ] Switch back to English
- [ ] Verify all UI updates
- [ ] Test during gameplay (HUD should update)

## Phase 7: Polish & Validation ✨

### Run Editor Tools
- [ ] Open Tools → Localization Tools
- [ ] Click "Find LocalizationManager in Scene" - should succeed
- [ ] Click "Validate Localization Tables" - check for warnings
- [ ] Click "Scan for Hardcoded French Strings" - should find minimal/none

### Full Gameplay Test
- [ ] Start new game
- [ ] Verify HUD displays correctly (English)
- [ ] Gain XP and level up
- [ ] Verify level-up UI displays correctly
- [ ] Select upgrade
- [ ] Verify upgrade card displays correctly
- [ ] Hover over rune
- [ ] Verify tooltip displays correctly
- [ ] Switch language to French
- [ ] Verify all UI updates immediately
- [ ] Continue playing in French
- [ ] Switch back to English
- [ ] Complete full run

### Edge Case Testing
- [ ] Test with missing translation (should fallback to English)
- [ ] Test with LocalizationManager disabled (should handle gracefully)
- [ ] Test scene reload (persistence of language setting)
- [ ] Test multiple language switches in rapid succession

## Phase 8: Documentation & Cleanup 📚

### Update Project Documentation
- [ ] Add localization section to main README
- [ ] Document how to add new languages
- [ ] Document how to add new keys

### Code Cleanup
- [ ] Remove old commented-out hardcoded strings
- [ ] Verify no console warnings related to localization
- [ ] Run code formatter if available

### Create Localization Guide for Team
- [ ] Share LOCALIZATION_SETUP.md with team
- [ ] Share QUICK_REFERENCE.md with team
- [ ] Demonstrate language switching in team meeting

## Completion Checklist ✅

- [ ] All ScriptableObjects migrated to LocalizedString
- [ ] All UI scripts use LocalizationManager/Helper
- [ ] Language selector implemented and tested
- [ ] Full gameplay tested in both languages
- [ ] No hardcoded user-facing strings remain
- [ ] Documentation complete
- [ ] Team trained on system

---

## Time Estimates

| Phase | Estimated Time |
|-------|---------------|
| Phase 1: Setup | 15-30 minutes |
| Phase 2: Tables | 30-60 minutes |
| Phase 3: Testing | 15 minutes |
| Phase 4: Content | 2-4 hours (depends on # of assets) |
| Phase 5: UI Scripts | 3-5 hours |
| Phase 6: Language Selector | 30 minutes |
| Phase 7: Testing | 1-2 hours |
| Phase 8: Documentation | 30 minutes |
| **Total** | **8-14 hours** |

**Note:** Time estimates are for one developer working solo. Can be parallelized by having different team members work on different UI scripts.

---

## Next Steps After Completion

1. **Add Spanish** - Following the guide in README.md
2. **Add More Languages** - German, Portuguese, etc.
3. **Voice-Over Localization** - Extend system for audio clips
4. **Right-to-Left Languages** - Add RTL text support if needed
5. **Dynamic Font Switching** - Per-language font support
6. **Localization Testing** - Automated tests for missing keys

---

**Last Updated:** 2025-12-28
**System Version:** 1.0
**Status:** Ready for implementation
