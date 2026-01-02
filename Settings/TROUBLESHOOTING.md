# Settings System - Troubleshooting Guide

## Recent Fixes Applied

### Fix 1: Language not applying on startup

**Problem**: Settings.json contains `"languageCode": "fr"` but UI shows English on restart.

**Root Cause**: `SimpleLocalizationManager.SetLanguage()` has early return if language is already set:
```csharp
if (_currentLanguage == language) return; // Skips reload!
```

**Solution**: Added `ForceSetLanguage()` method to `SimpleLocalizationManager`:
```csharp
public void ForceSetLanguage(Language language)
{
    _currentLanguage = language;
    LoadLanguage(_currentLanguage);
    OnLanguageChanged?.Invoke();
}
```

Modified `GameSettingsManager.ApplySettings()` to use `ForceSetLanguage()` instead of `SetLanguage()`.

**Files Modified**:
- `SimpleLocalizationManager.cs` - Added `ForceSetLanguage()` method
- `GameSettingsManager.cs` - Changed `ApplySettings()` to call `ForceSetLanguage()`

### Fix 2: Popup shows again after Cancel button

**Problem**: Click Back → Popup shows → Click Cancel → Click Back again → Popup doesn't show

**Root Cause**: No check to see if popup is already visible before showing it again.

**Solution**: Added check in `SettingsMenuUI.OnBackPressed()`:
```csharp
if (confirmationPopup != null && !confirmationPopup.activeSelf)
{
    ShowConfirmationPopup();
}
```

**File Modified**: `SettingsMenuUI.cs`

### Fix 3: Enhanced Logging for Debugging

Added verbose logging to track settings changes:

**GameSettingsManager**:
- `SaveSettings()` - Logs `HasUnsavedChanges` after save
- `DiscardChanges()` - Logs current vs saved language before/after discard
- `ApplySettings()` - Logs applied language code

**SettingsMenuUI**:
- `OnBackPressed()` - Logs `HasUnsavedChanges` status

## Common Issues

### Issue: "Language still not applying on startup"

**Check**:
1. Does `SimpleLocalizationManager` GameObject exist in MainMenu scene?
2. Check Console for: `[MainMenuUI] SimpleLocalizationManager is missing!`
3. Check Console for: `[GameSettingsManager] Applied language: French (code: fr)`

**Expected logs on startup**:
```
[GameSettingsManager] Loaded settings: Language=fr, MasterVol=1
[GameSettingsManager] Applied language: French (code: fr)
[GameSettingsManager] Settings applied to game systems
```

### Issue: "Popup still shows after clicking Save"

**Debug steps**:
1. Change language (don't save)
2. Click Save button
3. Check Console for: `[GameSettingsManager] Saved settings: Language=fr, HasUnsavedChanges=False`
4. Click Back button
5. Check Console for: `[SettingsMenuUI] OnBackPressed - HasUnsavedChanges: False`

**Expected**: If `HasUnsavedChanges=False`, popup should NOT show.

**If popup still shows**: Something is modifying settings AFTER the save. Check for:
- Other scripts calling `GameSettingsManager.SetXXX()` methods
- Event listeners modifying settings

### Issue: "Discard saves changes anyway"

**Debug steps**:
1. Change language from EN to FR (don't save)
2. Click Back
3. Click Discard in popup
4. Check Console logs:

**Expected logs**:
```
[GameSettingsManager] Discarding changes - Current language: fr, Saved language: en
[GameSettingsManager] Applied language: English (code: en)
[GameSettingsManager] Discarded changes, reverted to last saved settings - Language now: en, HasUnsavedChanges=False
```

5. Check settings.json file - should still have `"languageCode": "en"`

**If settings.json changed to "fr"**:
- Check if `autoSaveOnChange` is accidentally set to `true` in GameSettingsManager Inspector
- Check if any script is listening to `OnSettingsChanged` event and calling `SaveSettings()`

## Verification Checklist

Use these steps to verify the system works correctly:

### Test 1: Language Persistence
- [ ] Launch game from MainMenu
- [ ] Go to Settings
- [ ] Change language to French
- [ ] Click Save button
- [ ] Check Console: `HasUnsavedChanges=False`
- [ ] Click Back (should return to main menu, NO popup)
- [ ] Exit game completely
- [ ] Relaunch game
- [ ] Check Console: `Applied language: French (code: fr)`
- [ ] Verify UI is in French

### Test 2: Unsaved Changes Popup
- [ ] Go to Settings
- [ ] Change language to English
- [ ] DON'T click Save
- [ ] Click Back
- [ ] Check Console: `HasUnsavedChanges=True`
- [ ] Popup should show
- [ ] Click Cancel
- [ ] Still in Settings menu
- [ ] Click Back again
- [ ] Popup should show AGAIN

### Test 3: Discard Changes
- [ ] Change language to French (don't save)
- [ ] Click Back
- [ ] Click Discard
- [ ] Check Console: `reverted to last saved settings - Language now: en`
- [ ] UI should switch back to English
- [ ] Check settings.json: should still be `"languageCode": "en"`

### Test 4: Save Changes
- [ ] Change language to French
- [ ] Click Save
- [ ] Check Console: `HasUnsavedChanges=False`
- [ ] Click Back
- [ ] Should return to main menu (NO popup)
- [ ] Check settings.json: should be `"languageCode": "fr"`

## Debug Commands

### View Current Settings State

Add this temporary code to `SettingsMenuUI.Update()` for real-time monitoring:
```csharp
private void Update()
{
    if (Input.GetKeyDown(KeyCode.F1))
    {
        if (GameSettingsManager.Instance != null)
        {
            var current = GameSettingsManager.Instance.CurrentSettings;
            Debug.Log($"[DEBUG] Current language: {current.languageCode}, HasUnsavedChanges: {GameSettingsManager.Instance.HasUnsavedChanges}");
        }
    }
}
```

Press F1 in-game to see current state.

### Force Reset Settings

If settings get corrupted:
1. Use Unity menu: `Tools > Settings System > Delete Settings File`
2. Restart game
3. Default settings will be created

## File Locations

- Settings: `C:\Users\[Username]\AppData\LocalLow\DefaultCompany\Survivor\settings.json`
- Progression: `C:\Users\[Username]\AppData\LocalLow\DefaultCompany\Survivor\progression.json`

## Key Settings

### GameSettingsManager Inspector
- **Auto Save On Change**: FALSE (manual save mode)
- **Verbose Logging**: TRUE (for debugging)

### SimpleLocalizationManager Inspector (MainMenu scene)
- **Default Language**: English (or your preference)
- **Load From Resources**: TRUE

## Summary of Changes

1. ✅ Added `ForceSetLanguage()` to ensure language applies on startup
2. ✅ Fixed popup not re-showing after Cancel
3. ✅ Added extensive debug logging
4. ✅ Improved MainMenuUI to validate SimpleLocalizationManager exists

Next steps if issues persist:
- Run through Verification Checklist
- Check Console logs match expected output
- Verify Inspector settings are correct
- Check settings.json file directly
