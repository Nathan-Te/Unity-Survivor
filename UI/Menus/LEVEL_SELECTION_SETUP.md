# Guide de Configuration - Level Selection UI

## Vue d'ensemble

Le système `LevelSelectionUI` permet de créer un menu de sélection de niveaux avec:
- ✅ Locked/Unlocked states (basé sur `ProgressionManager`)
- ✅ Localisation automatique (EN/FR)
- ✅ Icônes et difficulté par niveau
- ✅ Chargement automatique des scènes

## Étape 1: Créer les Scènes de Niveau

Avant de configurer `LevelSelectionUI`, assurez-vous que vos scènes de jeu existent:

```
Assets/Scenes/
├─ MainMenu.unity
├─ TutorialLevel.unity    ← Créez vos scènes de niveau
├─ ForestLevel.unity
├─ DungeonLevel.unity
└─ BossLevel.unity
```

**Important**: Ajoutez toutes ces scènes dans **Build Settings** (`File > Build Settings > Add Open Scenes`)

## Étape 2: Ajouter les Clés de Localisation

Dans `en.json` et `fr.json`, ajoutez les clés pour chaque niveau:

### Exemple en.json
```json
{ "key": "LEVEL_TUTORIAL_NAME", "value": "Tutorial" },
{ "key": "LEVEL_TUTORIAL_DESC", "value": "Learn the basics of combat and survival" },
{ "key": "LEVEL_FOREST_NAME", "value": "Dark Forest" },
{ "key": "LEVEL_FOREST_DESC", "value": "Face waves of enemies in the haunted forest" }
```

### Exemple fr.json
```json
{ "key": "LEVEL_TUTORIAL_NAME", "value": "Tutoriel" },
{ "key": "LEVEL_TUTORIAL_DESC", "value": "Apprenez les bases du combat et de la survie" },
{ "key": "LEVEL_FOREST_NAME", "value": "Forêt Sombre" },
{ "key": "LEVEL_FOREST_DESC", "value": "Affrontez des vagues d'ennemis dans la forêt hantée" }
```

## Étape 3: Créer les Icônes de Niveau

Créez des sprites pour vos niveaux (recommandé: 256x256 ou 512x512):

```
Assets/Art/UI/LevelIcons/
├─ icon_tutorial.png
├─ icon_forest.png
├─ icon_dungeon.png
└─ icon_boss.png
```

Configurez les sprites:
- **Texture Type**: Sprite (2D and UI)
- **Max Size**: 256 ou 512

## Étape 4: Remplir availableLevels dans Unity Inspector

### 4.1 Sélectionner le GameObject LevelSelectionUI

Dans votre scène MainMenu:
1. Sélectionnez le GameObject avec le component `LevelSelectionUI`
2. Dans l'Inspector, trouvez la section **"Level Definitions"**
3. Cliquez sur le **+** pour ajouter un nouveau `LevelDefinition`

### 4.2 Remplir chaque LevelDefinition

Pour chaque niveau, remplissez les champs:

#### **Tutorial Level (Exemple complet)**
```
Element 0
├─ Level Id: "Level_Tutorial"
├─ Name Key: "LEVEL_TUTORIAL_NAME"
├─ Scene Name: "TutorialLevel"          ← Nom EXACT de votre scène Unity
├─ Icon: icon_tutorial (Sprite)
├─ Description Key: "LEVEL_TUTORIAL_DESC"
└─ Difficulty: 1                         ← 1-5 étoiles
```

#### **Forest Level**
```
Element 1
├─ Level Id: "Level_Forest"
├─ Name Key: "LEVEL_FOREST_NAME"
├─ Scene Name: "ForestLevel"
├─ Icon: icon_forest
├─ Description Key: "LEVEL_FOREST_DESC"
└─ Difficulty: 2
```

#### **Dungeon Level**
```
Element 2
├─ Level Id: "Level_Dungeon"
├─ Name Key: "LEVEL_DUNGEON_NAME"
├─ Scene Name: "DungeonLevel"
├─ Icon: icon_dungeon
├─ Description Key: "LEVEL_DUNGEON_DESC"
└─ Difficulty: 3
```

#### **Boss Level**
```
Element 3
├─ Level Id: "Level_Boss"
├─ Name Key: "LEVEL_BOSS_NAME"
├─ Scene Name: "BossLevel"
├─ Icon: icon_boss
├─ Description Key: "LEVEL_BOSS_DESC"
└─ Difficulty: 5
```

## Étape 5: Configurer les Unlocks par Défaut

Par défaut, seul le premier niveau (Tutorial) est débloqué. Pour modifier cela:

### Option A: Via Code (Recommandé pour le premier lancement)

Dans `PlayerProgressionData.CreateDefault()`:

```csharp
public static PlayerProgressionData CreateDefault()
{
    var data = new PlayerProgressionData
    {
        gold = 0,
        maxSpellSlots = 4,
        runeUnlocks = new List<RuneUnlockData>(),
        unlockedLevelIds = new List<string>
        {
            "Level_Tutorial"  // ← Débloqué par défaut
        },
        // ...
    };
    return data;
}
```

### Option B: Débloquer via Upgrades Menu

Créez des upgrades pour débloquer les niveaux (voir section suivante).

## Étape 6: Créer des Upgrades pour Débloquer les Niveaux

Dans `UpgradesMenuUI`, ajoutez des upgrades de type `UnlockLevel`:

### Exemple dans l'Inspector

```
Element 0 (Unlock Forest)
├─ Name Key: "UPGRADE_UNLOCK_FOREST"
├─ Description Key: "UPGRADE_UNLOCK_FOREST_DESC"
├─ Cost: 100
├─ Upgrade Type: UnlockLevel
├─ Target Level Id: "Level_Forest"     ← Doit matcher le levelId
└─ Icon: icon_forest
```

### Ajouter les clés de localisation pour les upgrades

**en.json:**
```json
{ "key": "UPGRADE_UNLOCK_FOREST", "value": "Unlock: Dark Forest" },
{ "key": "UPGRADE_UNLOCK_FOREST_DESC", "value": "Unlock the haunted forest level" },
{ "key": "UPGRADE_UNLOCK_DUNGEON", "value": "Unlock: Ancient Dungeon" },
{ "key": "UPGRADE_UNLOCK_DUNGEON_DESC", "value": "Unlock the cursed dungeon level" }
```

**fr.json:**
```json
{ "key": "UPGRADE_UNLOCK_FOREST", "value": "Débloquer : Forêt Sombre" },
{ "key": "UPGRADE_UNLOCK_FOREST_DESC", "value": "Débloquez le niveau de la forêt hantée" },
{ "key": "UPGRADE_UNLOCK_DUNGEON", "value": "Débloquer : Donjon Ancien" },
{ "key": "UPGRADE_UNLOCK_DUNGEON_DESC", "value": "Débloquez le niveau du donjon maudit" }
```

## Étape 7: Tester le Système

### Test 1: Vérifier les Locked States

1. Lancez le jeu
2. Ouvrez le menu Level Selection
3. Seul le Tutorial devrait être cliquable (les autres ont l'overlay "Locked")

### Test 2: Débloquer un Niveau

Via code (temporaire pour tester):
```csharp
// Dans Start() de MainMenuUI ou un bouton de debug
ProgressionManager.Instance.UnlockLevel("Level_Forest");
```

Ou via le menu Upgrades:
1. Donnez-vous de l'or: `ProgressionManager.Instance.AwardGold(1000);`
2. Achetez l'upgrade "Unlock Forest"
3. Retournez au Level Selection → Forest devrait être débloqué

### Test 3: Lancer un Niveau

1. Cliquez sur Tutorial (débloqué)
2. La scène `TutorialLevel` devrait se charger
3. Vérifiez que le jeu fonctionne normalement

## Résolution de Problèmes

### ❌ Le niveau ne se charge pas

**Cause**: Scene name incorrect ou scène non ajoutée au Build Settings

**Solution**:
1. Vérifiez que `sceneName` dans LevelDefinition correspond EXACTEMENT au nom de la scène
2. Ouvrez `File > Build Settings` et ajoutez toutes vos scènes

### ❌ Les noms ne s'affichent pas / affichent la clé

**Cause**: Clé de localisation manquante dans en.json/fr.json

**Solution**:
1. Vérifiez que `nameKey` existe dans `en.json` et `fr.json`
2. Vérifiez qu'il n'y a pas de faute de frappe dans la clé

### ❌ Tous les niveaux sont locked

**Cause**: Aucun niveau débloqué dans la progression

**Solution**:
```csharp
// Débloquez le tutorial par défaut
ProgressionManager.Instance.UnlockLevel("Level_Tutorial");
```

### ❌ L'icône ne s'affiche pas

**Cause**: Sprite non assigné ou mauvais type de texture

**Solution**:
1. Vérifiez que le sprite est assigné dans l'Inspector
2. Vérifiez que la texture est de type "Sprite (2D and UI)"

## Exemple Complet: Workflow de Progression

### 1. Joueur lance le jeu (première fois)
- Progression par défaut créée
- Seul "Level_Tutorial" débloqué

### 2. Joueur complète le Tutorial
```csharp
// À la fin du tutorial
ProgressionManager.Instance.AwardGold(50);
ProgressionManager.Instance.RecordRunStats(enemiesKilled, time, level);
SceneManager.LoadScene("MainMenu");
```

### 3. Joueur achète "Unlock Forest"
- Dépense 100 gold
- "Level_Forest" ajouté à `unlockedLevelIds`
- Bouton Forest devient cliquable

### 4. Joueur sélectionne Forest
- Scène "ForestLevel" se charge
- Le jeu continue normalement

## Conseils de Design

### Difficulté (1-5 étoiles)
- **1 étoile**: Tutorial, facile
- **2-3 étoiles**: Niveaux normaux
- **4 étoiles**: Niveaux difficiles
- **5 étoiles**: Boss, challenges extrêmes

### Prix de déblocage recommandés
- **Niveau 1**: 0 gold (débloqué par défaut)
- **Niveau 2**: 100 gold
- **Niveau 3**: 200 gold
- **Niveau 4**: 400 gold
- **Boss**: 1000 gold

### Organisation des IDs
Utilisez un préfixe cohérent:
- `Level_Tutorial`
- `Level_Forest`
- `Level_Dungeon`
- `Level_Boss_Final`

Cela facilite le debug et la maintenance!

## Code de Debug Utile

Ajoutez ceci à un bouton de debug pour tout débloquer:

```csharp
[ContextMenu("Debug: Unlock All Levels")]
public void DebugUnlockAll()
{
    ProgressionManager.Instance.UnlockLevel("Level_Tutorial");
    ProgressionManager.Instance.UnlockLevel("Level_Forest");
    ProgressionManager.Instance.UnlockLevel("Level_Dungeon");
    ProgressionManager.Instance.UnlockLevel("Level_Boss");
    Debug.Log("All levels unlocked!");
}
```

## Résumé: Checklist Rapide

- [ ] Créer les scènes de niveau
- [ ] Ajouter les scènes au Build Settings
- [ ] Créer les icônes (sprites)
- [ ] Ajouter les clés de localisation (en.json + fr.json)
- [ ] Remplir `availableLevels` dans LevelSelectionUI Inspector
- [ ] Configurer les unlocks par défaut dans CreateDefault()
- [ ] (Optionnel) Créer des upgrades pour débloquer les niveaux
- [ ] Tester locked/unlocked states
- [ ] Tester le chargement des scènes

Vous êtes prêt! 🎮
