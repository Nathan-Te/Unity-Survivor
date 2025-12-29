# Rune Localization Guide

## Overview

This system consolidates all localization strings for a rune into a single `RuneLocalizationData` asset instead of creating separate `LocalizedString` assets for each name and description.

## Structure

```
RuneLocalizationData (ScriptableObject)
├─ runeName: LocalizedString
└─ Descriptions by Rarity:
   ├─ commonDescriptions: LocalizedString[]
   ├─ rareDescriptions: LocalizedString[]
   ├─ epicDescriptions: LocalizedString[]
   └─ legendaryDescriptions: LocalizedString[]
```

## Benefits

✅ **Single Asset Per Rune**: All localization strings in one place
✅ **Better Organization**: Descriptions grouped by rarity
✅ **Easy to Edit**: Visual editor with color-coded rarities
✅ **Batch Creation**: Create localization data for all runes at once
✅ **Bidirectional Sync**: Import from and export to RuneSO

## Workflow

### Option 1: Batch Creation (Recommended)

1. **Open the Batch Creator**:
   - Menu: `Tools → Localization → Rune Localization Batch Creator`

2. **Configure Folders**:
   - Source Folder: Where your RuneSO assets are (e.g., `Assets/ScriptableObjects/Runes`)
   - Output Folder: Where to create localization assets (e.g., `Assets/ScriptableObjects/Localization/Runes`)

3. **Scan and Create**:
   - Click "Scan For Runes"
   - Select which runes to create localization data for
   - Click "Create Selected"

4. **Edit Descriptions**:
   - Open each `RuneLocalizationData` asset
   - Edit the descriptions in the Inspector
   - Click "Export To RuneSO" to apply changes

### Option 2: Manual Creation

1. **Create Asset**:
   - Right-click in Project → `Create → Localization → Rune Localization Data`

2. **Link RuneSO**:
   - Drag your RuneSO into the "Linked RuneSO" field
   - Click "Import From Linked RuneSO"

3. **Edit Strings**:
   - Edit the rune name
   - Edit descriptions for each rarity
   - Add/remove descriptions as needed

4. **Apply Changes**:
   - Click "Export To RuneSO" to push changes back to the RuneSO

## Editor Features

### Custom Inspector

The `RuneLocalizationData` inspector provides:

- **Quick Import/Export Buttons**: Sync with linked RuneSO
- **Color-Coded Rarity Sections**: Visual distinction between rarities
- **Array Management**: Add/remove descriptions easily
- **Validation**: Warns if linked RuneSO is missing

### Batch Creator

The batch creator tool (`Tools → Localization → Rune Localization Batch Creator`) offers:

- **Folder Scanning**: Automatically finds all RuneSO assets
- **Bulk Selection**: Select all/none, or pick individually
- **Duplicate Detection**: Shows which runes already have localization data
- **Progress Feedback**: Reports created/skipped assets

## Integration with RuneSO

### Current Setup

Your `RuneSO` already has:
```csharp
public LocalizedString runeName;
```

And your `RuneDefinition` has:
```csharp
public LocalizedString Description;
```

### How It Works

1. **Import**: `RuneLocalizationData` reads from `RuneSO`:
   - Copies `runeName`
   - Copies all `RuneDefinition.Description` strings organized by rarity

2. **Export**: `RuneLocalizationData` writes to `RuneSO`:
   - Updates `runeName`
   - Updates all `RuneDefinition.Description` strings

3. **Game Runtime**: No changes needed!
   - Your existing code continues to use `runeSO.runeName` and `definition.Description`
   - Everything works exactly as before

## Best Practices

### Recommended Folder Structure

```
Assets/
├─ ScriptableObjects/
│  ├─ Runes/              # Your RuneSO assets
│  │  ├─ Forms/
│  │  ├─ Effects/
│  │  └─ Modifiers/
│  └─ Localization/
│     └─ Runes/           # RuneLocalizationData assets
│        ├─ RuneLoc_Fireball.asset
│        ├─ RuneLoc_IceShard.asset
│        └─ ...
```

### Naming Convention

- **RuneLocalizationData**: `RuneLoc_<RuneName>.asset`
- Example: `RuneLoc_Fireball.asset` for Fireball rune

### Workflow Tips

1. **Create Once**: Use batch creator to generate all localization assets
2. **Edit in Bulk**: Edit all descriptions in one session
3. **Export All**: Export changes to RuneSO assets when done
4. **Version Control**: Commit both RuneSO and RuneLocalizationData together

## Code Examples

### Accessing Localized Strings (No Changes Needed!)

```csharp
// These work exactly as before:
string name = myRune.runeName;  // Auto-converts LocalizedString
string desc = upgrade.Description;  // Auto-converts LocalizedString
```

### Using RuneLocalizationData (If Needed)

```csharp
RuneLocalizationData locData = ...; // Your localization asset

// Get rune name
string name = locData.runeName;

// Get specific description
LocalizedString commonDesc = locData.GetDescription(Rarity.Common, 0);

// Get all descriptions for a rarity
LocalizedString[] rareDescs = locData.GetDescriptions(Rarity.Rare);
```

## Migration from Individual LocalizedStrings

If you already have individual `LocalizedString` assets:

1. **Use Batch Creator**: It will import existing strings from RuneSO
2. **Verify Data**: Check that descriptions imported correctly
3. **Export to RuneSO**: Update RuneSO with consolidated data
4. **Delete Old Assets**: Remove individual LocalizedString assets (optional)

## Troubleshooting

### Import/Export Not Working

**Problem**: "Import From RuneSO" button does nothing
**Solution**: Make sure "Linked RuneSO" field is filled

### Descriptions Mismatch

**Problem**: Wrong number of descriptions after import
**Solution**: RuneSO might have been updated. Re-import to sync

### Changes Not Saving

**Problem**: Edits disappear after closing Inspector
**Solution**: Click "Export To RuneSO" before closing Unity

## Advanced Usage

### Custom Editor Extensions

You can extend `RuneLocalizationDataEditor` to add:
- Bulk find/replace for descriptions
- Translation export/import
- Preview in current language

### Scripted Batch Operations

```csharp
// Find all RuneLocalizationData assets
string[] guids = AssetDatabase.FindAssets("t:RuneLocalizationData");
foreach (string guid in guids)
{
    string path = AssetDatabase.GUIDToAssetPath(guid);
    RuneLocalizationData data = AssetDatabase.LoadAssetAtPath<RuneLocalizationData>(path);

    // Perform bulk operations
    // ...
}
```

## Summary

This system provides a **centralized, organized approach** to rune localization:

- **Before**: 1 name + N descriptions = N+1 individual assets per rune
- **After**: 1 consolidated asset per rune with all strings

It's **backwards compatible** with your existing code and provides powerful **batch editing** tools for managing translations at scale.
