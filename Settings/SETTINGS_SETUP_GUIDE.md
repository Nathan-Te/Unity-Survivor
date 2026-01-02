# Settings System - Setup Guide

## Critical Setup Requirements

### MainMenu Scene Setup

For the settings system to work correctly, the **MainMenu scene** MUST have a `SimpleLocalizationManager` GameObject.

#### Why is this required?

1. `GameSettingsManager` loads `settings.json` on startup
2. It tries to apply the saved language to `SimpleLocalizationManager`
3. If `SimpleLocalizationManager` doesn't exist, the language won't be applied
4. This results in the UI displaying in the wrong language (default instead of saved)

### Setup Steps

1. **Open your MainMenu scene**

2. **Create a SimpleLocalizationManager GameObject** (if it doesn't exist):
   - Right-click in Hierarchy → Create Empty
   - Name it: `SimpleLocalizationManager`
   - Add Component → `SimpleLocalizationManager` script
   - Configure in Inspector:
     - Default Language: English (or your preference)
     - Load From Resources: ✓ (checked)

3. **Verify the setup**:
   - Launch the game from MainMenu scene
   - Check Console for: `[MainMenuUI] SimpleLocalizationManager is missing!`
   - If you see this error, go back to step 2

### Initialization Order

The `MainMenuUI` script ensures the correct initialization order:

```
1. MainMenuUI.Start()
2. InitializeGameSettings()
   ├─ Checks SimpleLocalizationManager exists (CRITICAL!)
   └─ Creates GameSettingsManager
3. GameSettingsManager.Awake()
   ├─ LoadSettings() (reads settings.json)
   └─ ApplySettings()
       └─ SimpleLocalizationManager.SetLanguage(saved language)
4. UI refreshes with correct language
```

## Testing the System

### Test Language Persistence

1. Launch game from MainMenu
2. Go to Settings
3. Change language to French
4. Click Save button
5. Exit game completely
6. Relaunch game from MainMenu
7. **Expected**: UI should be in French immediately

### Troubleshooting

#### Problem: Language always defaults to English/French on restart

**Cause**: `SimpleLocalizationManager` is missing from MainMenu scene

**Fix**: Follow "MainMenu Scene Setup" steps above

**Verification**: Check Console for error message about missing SimpleLocalizationManager

#### Problem: Popup doesn't show after clicking Cancel

**Cause**: Fixed in latest version

**Verification**:
1. Change language (don't save)
2. Click Back
3. Popup appears
4. Click Cancel
5. Click Back again
6. **Expected**: Popup appears again (was broken before fix)

## File Locations

### Windows
```
C:\Users\[Username]\AppData\LocalLow\[CompanyName]\[GameName]\settings.json
C:\Users\[Username]\AppData\LocalLow\[CompanyName]\[GameName]\progression.json
```

### Unity Menu Debug Tools

Access via `Tools > Settings System`:
- Show Settings File Location
- Open Settings Folder
- View Settings File Content
- Delete Settings File
- Create Test Settings

## Settings.json Format

```json
{
    "masterVolume": 1.0,
    "musicVolume": 0.7,
    "sfxVolume": 0.8,
    "muteAll": false,
    "languageCode": "en",
    "qualityLevel": 2,
    "fullscreen": true,
    "targetFrameRate": 60,
    "vsync": true,
    "showDamageNumbers": true,
    "screenShake": true,
    "screenShakeIntensity": 1.0,
    "colorBlindMode": false,
    "uiScale": 1.0
}
```

## DontDestroyOnLoad Singletons

Both of these persist across scene loads:

- **GameSettingsManager** - Created automatically by MainMenuUI if missing
- **SimpleLocalizationManager** - MUST be manually placed in MainMenu scene

## Common Mistakes

❌ **DON'T**: Forget to add SimpleLocalizationManager to MainMenu scene
✅ **DO**: Add it to MainMenu scene and verify it exists

❌ **DON'T**: Modify settings.json fields manually while game is running
✅ **DO**: Use the in-game Settings menu or Unity debug tools

❌ **DON'T**: Call `SimpleLocalizationManager.SetLanguage()` directly
✅ **DO**: Call `GameSettingsManager.SetLanguage()` (saves to file)

## Summary

- Settings saved in separate `settings.json` file
- Language persistence requires SimpleLocalizationManager in MainMenu scene
- Manual save mode with unsaved changes detection
- Popup confirmation when exiting with unsaved changes
- Popup correctly re-shows after Cancel button
