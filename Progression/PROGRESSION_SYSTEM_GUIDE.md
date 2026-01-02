# Progression System & Main Menu Guide

## Overview

Le système de progression permet de sauvegarder la progression du joueur entre les parties et d'offrir un système de méta-progression (upgrades permanents achetés avec de l'or).

## Architecture

```
ProgressionManager (DontDestroyOnLoad)
├─ PlayerProgressionData (données de sauvegarde)
│  ├─ Gold (monnaie méta-progression)
│  ├─ Max Spell Slots (nombre de sorts équipables)
│  ├─ Rune Unlocks (runes débloquées + niveaux max)
│  ├─ Level Unlocks (niveaux jouables)
│  └─ Statistics (runs, kills, best time, etc.)
└─ SaveSystem (JSON file I/O)
```

## Fichiers Créés

### Core Progression
- `PlayerProgressionData.cs` - Structure de données sérialisables
- `SaveSystem.cs` - Système de sauvegarde/chargement JSON
- `ProgressionManager.cs` - Singleton DontDestroyOnLoad qui gère la progression

### UI Menus
- `MainMenuUI.cs` - Menu principal avec navigation
- `LevelSelectionUI.cs` - Sélection de niveaux
- `LevelSelectButton.cs` - Bouton individuel de sélection de niveau
- `UpgradesMenuUI.cs` - Menu des upgrades méta-progression
- `UpgradeCardButton.cs` - Carte d'upgrade individuelle
- `SettingsMenuUI.cs` - Placeholder pour settings
- `LeaderboardUI.cs` - Placeholder pour leaderboard

## Setup dans Unity

### 1. Créer un GameObject ProgressionManager (DontDestroyOnLoad)

```
Hierarchy:
└─ PersistentManagers (root, DontDestroyOnLoad)
   ├─ GameStateController
   ├─ GameStateManager
   ├─ ProgressionManager ← NEW
   └─ ...autres managers persistants
```

Ajouter le component `ProgressionManager` et configurer:
- Auto Save On Change: ✓ (sauvegarde automatique)
- Verbose Logging: ✓ (pour debugging)

### 2. Créer une scène MainMenu

```
Hierarchy:
└─ Canvas
   ├─ MainMenuPanel
   │  ├─ Title
   │  ├─ GoldDisplay (TextMeshProUGUI)
   │  ├─ PlayButton
   │  ├─ UpgradesButton
   │  ├─ SettingsButton
   │  ├─ LeaderboardButton
   │  └─ QuitButton
   ├─ LevelSelectionPanel
   │  ├─ BackButton
   │  └─ LevelButtonContainer (Vertical Layout Group)
   ├─ UpgradesPanel
   │  ├─ BackButton
   │  ├─ GoldDisplay
   │  └─ UpgradeCardContainer (Grid Layout Group)
   ├─ SettingsPanel
   │  ├─ BackButton
   │  ├─ TitleText
   │  ├─ PlaceholderText
   │  ├─ EnglishButton
   │  └─ FrenchButton
   └─ LeaderboardPanel
      ├─ BackButton
      ├─ TitleText
      └─ StatsText
```

### 3. Configurer les Components

**MainMenuUI:**
- Assigner tous les panels (CanvasGroup requis sur chaque panel)
- Assigner tous les boutons
- Assigner goldText

**LevelSelectionUI:**
- Créer des `LevelDefinition` dans le inspector
- Assigner le prefab `LevelSelectButton`
- Assigner le container

**UpgradesMenuUI:**
- Créer des `MetaUpgradeDefinition` dans le inspector
- Assigner le prefab `UpgradeCardButton`
- Assigner le container

### 4. Créer les Prefabs

**LevelSelectButton Prefab:**
```
Button
├─ Icon (Image)
├─ NameText (TextMeshProUGUI)
├─ DifficultyText (TextMeshProUGUI)
└─ LockedOverlay (GameObject)
   └─ LockIcon (Image)
```

**UpgradeCardButton Prefab:**
```
Button
├─ Icon (Image)
├─ NameText (TextMeshProUGUI)
├─ DescriptionText (TextMeshProUGUI)
├─ CostText (TextMeshProUGUI)
└─ PurchasedOverlay (GameObject)
   └─ CheckIcon (Image)
```

## Utilisation

### Débloquer une Rune
```csharp
ProgressionManager.Instance.UnlockRune("Bolt");
```

### Donner de l'Or
```csharp
ProgressionManager.Instance.AwardGold(100);
```

### Dépenser de l'Or
```csharp
bool success = ProgressionManager.Instance.SpendGold(50);
if (success) {
    // Purchase successful
}
```

### Augmenter le Niveau Max d'une Rune
```csharp
ProgressionManager.Instance.UpgradeRuneMaxLevel("Fire", 10);
```

### Débloquer un Niveau
```csharp
ProgressionManager.Instance.UnlockLevel("Level_Forest");
```

### Vérifier si une Rune est Débloquée
```csharp
bool unlocked = ProgressionManager.Instance.CurrentProgression.IsRuneUnlocked("Bolt");
```

### Enregistrer des Stats de Run
```csharp
// À la fin d'une partie
ProgressionManager.Instance.RecordRunStats(
    enemiesKilled: 250,
    runTime: 1234.5f,
    playerLevel: 15
);
```

## Types d'Upgrades Méta

### UnlockRune
Débloque une rune pour utilisation en jeu.
```csharp
new MetaUpgradeDefinition {
    nameKey = "UPGRADE_UNLOCK_BOLT",
    cost = 100,
    upgradeType = MetaUpgradeType.UnlockRune,
    targetRuneId = "Bolt"
}
```

### IncreaseRuneMaxLevel
Augmente le niveau maximum d'une rune (repeatable).
```csharp
new MetaUpgradeDefinition {
    nameKey = "UPGRADE_FIRE_MAX_LEVEL",
    cost = 50,
    upgradeType = MetaUpgradeType.IncreaseRuneMaxLevel,
    targetRuneId = "Fire"
}
```

### IncreaseSpellSlots
Augmente le nombre de slots de sorts (repeatable).
```csharp
new MetaUpgradeDefinition {
    nameKey = "UPGRADE_SPELL_SLOT",
    cost = 200,
    upgradeType = MetaUpgradeType.IncreaseSpellSlots
}
```

### UnlockLevel
Débloque un nouveau niveau jouable.
```csharp
new MetaUpgradeDefinition {
    nameKey = "UPGRADE_UNLOCK_FOREST",
    cost = 150,
    upgradeType = MetaUpgradeType.UnlockLevel,
    targetLevelId = "Level_Forest"
}
```

## Intégration avec le Jeu

### À la Fin d'une Partie
Ajoutez dans votre script de fin de partie:
```csharp
// Award gold based on performance
int goldEarned = Mathf.RoundToInt(score / 100f);
ProgressionManager.Instance.AwardGold(goldEarned);

// Record stats
ProgressionManager.Instance.RecordRunStats(
    EnemyManager.Instance.TotalEnemiesKilled,
    GameTimer.Instance.ElapsedTime,
    LevelManager.Instance.currentLevel
);

// Return to main menu
SceneManager.LoadScene("MainMenu");
```

### Filtrer les Runes selon Unlocks
Dans votre système de level-up, vérifiez les unlocks:
```csharp
bool IsRuneAvailable(RuneSO rune)
{
    var progression = ProgressionManager.Instance?.CurrentProgression;
    if (progression == null) return true; // Default to all available

    return progression.IsRuneUnlocked(rune.name);
}
```

### Respecter le Niveau Max des Runes
```csharp
bool CanUpgradeRune(Rune rune)
{
    var progression = ProgressionManager.Instance?.CurrentProgression;
    if (progression == null) return true;

    int maxLevel = progression.GetRuneMaxLevel(rune.Data.name);
    return rune.Level < maxLevel;
}
```

## Localisation

Toutes les clés de localisation sont dans `en.json` / `fr.json`:

```json
{ "key": "MENU_SETTINGS", "value": "Settings" }
{ "key": "MENU_LEADERBOARD", "value": "Leaderboard" }
{ "key": "MENU_UPGRADES", "value": "Upgrades" }
{ "key": "MENU_PLAY", "value": "Play" }
{ "key": "MENU_QUIT", "value": "Quit" }
```

## Sauvegarde

Le fichier de sauvegarde est stocké ici:
- **Windows:** `C:\Users\[Username]\AppData\LocalLow\[CompanyName]\[GameName]\progression.json`
- **Mac:** `~/Library/Application Support/[CompanyName]/[GameName]/progression.json`
- **Linux:** `~/.config/unity3d/[CompanyName]/[GameName]/progression.json`

Chemin accessible via: `Application.persistentDataPath`

### Menu de Debug Unity

Utilisez le menu `Tools > Save System` pour:
- **Show Save File Location** - Affiche et ouvre le dossier de sauvegarde
- **Delete Save File** - Supprime la save (avec confirmation)
- **Create Test Save (1000 Gold)** - Crée une save de test
- **View Save File Content** - Affiche le JSON de la save

### Réinitialiser la Sauvegarde (Debug)
```csharp
// Option 1: Via Unity Menu (Recommandé)
// Tools > Save System > Delete Save File

// Option 2: Via code
ProgressionManager.Instance.ResetProgression();

// Option 3: Supprimer le fichier
SaveSystem.DeleteSave();
```

### Configuration de la Progression Initiale

Modifiez `PlayerProgressionData.CreateDefault()` pour configurer:
- Gold de départ
- Spell slots initiaux
- Runes débloquées par défaut
- Niveaux débloqués par défaut

**Pour plus de détails**, voir: [SAVE_SYSTEM_FAQ.md](SAVE_SYSTEM_FAQ.md)

## Events

Le `ProgressionManager` expose des events pour réagir aux changements:

```csharp
// Subscribe to progression loaded
ProgressionManager.Instance.OnProgressionLoaded += (data) => {
    Debug.Log($"Progression loaded: {data.gold} gold");
};

// Subscribe to progression changes
ProgressionManager.Instance.OnProgressionChanged += (data) => {
    // Update UI, refresh displays, etc.
};
```

## Flow de Navigation

```
MainMenu
├─ Play → LevelSelection → GameScene
├─ Upgrades → UpgradesMenu → (purchase) → MainMenu
├─ Settings → SettingsMenu → MainMenu
├─ Leaderboard → LeaderboardMenu → MainMenu
└─ Quit → Exit Application
```

## TODO / Extensions Futures

- [ ] Système de achievements
- [ ] Leaderboard en ligne (cloud save)
- [ ] Plus d'upgrades méta (unlock characters, permanent buffs, etc.)
- [ ] Settings (audio, graphics, controls)
- [ ] Animation transitions entre panels
- [ ] Confirmation dialogs (quit, reset save, etc.)

## Troubleshooting

**La sauvegarde ne fonctionne pas:**
- Vérifier que `ProgressionManager` est bien DontDestroyOnLoad
- Vérifier les logs console pour les erreurs JSON
- Vérifier les permissions du dossier `persistentDataPath`

**Les runes ne se débloquent pas:**
- Vérifier que le `runeId` correspond au nom de l'asset RuneSO
- Vérifier que `autoSaveOnChange` est activé

**Les panels ne s'affichent pas:**
- Vérifier que chaque panel a un component `CanvasGroup`
- Vérifier que le premier panel est bien assigné dans `MainMenuUI`
