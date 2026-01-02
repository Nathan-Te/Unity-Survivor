# Save System - FAQ et Guide

## Question 1: Comment configurer la save initiale d'un joueur?

### Réponse Courte
Modifiez la méthode `PlayerProgressionData.CreateDefault()` dans [PlayerProgressionData.cs](PlayerProgressionData.cs).

### Réponse Détaillée

La progression par défaut est créée dans `PlayerProgressionData.CreateDefault()`. Vous pouvez y configurer:

```csharp
public static PlayerProgressionData CreateDefault()
{
    var data = new PlayerProgressionData
    {
        // 1. GOLD DE DÉPART
        gold = 0,  // Changez pour donner de l'or au départ (ex: 500)

        // 2. NOMBRE DE SPELL SLOTS
        maxSpellSlots = 4,  // Par défaut 4, augmentez pour plus de sorts

        // 3. RUNES DÉBLOQUÉES (vide = toutes locked)
        runeUnlocks = new List<RuneUnlockData>(),

        // 4. NIVEAUX DÉBLOQUÉS
        unlockedLevelIds = new List<string>
        {
            "Level_Tutorial"  // Tutorial débloqué par défaut
            // Ajoutez plus de lignes pour débloquer d'autres niveaux
        },

        // 5. STATISTIQUES (toujours 0 au départ)
        totalRunsCompleted = 0,
        totalEnemiesKilled = 0,
        bestRunTime = 0f,
        highestLevel = 0
    };

    // 6. DÉBLOQUER DES RUNES PAR DÉFAUT (optionnel)
    // data.UnlockRune("Bolt");
    // data.UpgradeRuneMaxLevel("Bolt", 5);

    return data;
}
```

### Exemples de Configuration

#### Exemple 1: Nouveau Joueur Standard
```csharp
gold = 0,
maxSpellSlots = 4,
runeUnlocks = new List<RuneUnlockData>(),
unlockedLevelIds = new List<string> { "Level_Tutorial" }
```

#### Exemple 2: Mode Debug (tout débloqué)
```csharp
gold = 10000,
maxSpellSlots = 8,
unlockedLevelIds = new List<string>
{
    "Level_Tutorial",
    "Level_Forest",
    "Level_Dungeon",
    "Level_Boss"
}

// Puis dans le code:
data.UnlockRune("Bolt");
data.UpgradeRuneMaxLevel("Bolt", 10);
data.UnlockRune("Fire");
data.UpgradeRuneMaxLevel("Fire", 10);
// etc.
```

#### Exemple 3: Démo/Preview Build
```csharp
gold = 500,  // Un peu d'or pour tester les upgrades
maxSpellSlots = 5,
unlockedLevelIds = new List<string>
{
    "Level_Tutorial",
    "Level_Forest"  // 2 niveaux pour la démo
}
```

### Comment appliquer les changements

1. **Modifier** `PlayerProgressionData.CreateDefault()`
2. **Supprimer** le fichier de save existant via:
   - Unity Menu: `Tools > Save System > Delete Save File`
   - Ou supprimer manuellement le fichier
3. **Relancer** le jeu → nouvelle save avec vos paramètres sera créée

---

## Question 2: Où se trouve le fichier de sauvegarde?

### Réponse Courte
**Windows**: `C:\Users\[VotreNom]\AppData\LocalLow\[CompanyName]\[GameName]\progression.json`

Pour trouver le chemin exact:
- **Unity Menu**: `Tools > Save System > Show Save File Location`
- **Code**: `Debug.Log(SaveSystem.GetSavePath());`

### Réponse Détaillée

#### Localisation par Plateforme

| Plateforme | Chemin |
|------------|--------|
| **Windows** | `C:\Users\[Username]\AppData\LocalLow\[CompanyName]\[GameName]\progression.json` |
| **Mac** | `~/Library/Application Support/[CompanyName]/[GameName]/progression.json` |
| **Linux** | `~/.config/unity3d/[CompanyName]/[GameName]/progression.json` |

#### Variables de Chemin

- **`[Username]`**: Votre nom d'utilisateur Windows (ex: `miste`)
- **`[CompanyName]`**: Défini dans `Edit > Project Settings > Player > Company Name`
- **`[GameName]`**: Défini dans `Edit > Project Settings > Player > Product Name`

#### Exemple Concret

Si vos settings Unity sont:
- **Company Name**: `MyStudio`
- **Product Name**: `SurvivorGame`
- **Username**: `miste`

Le chemin sera:
```
C:\Users\miste\AppData\LocalLow\MyStudio\SurvivorGame\progression.json
```

### Comment Accéder au Fichier

#### Méthode 1: Menu Unity (Recommandé)
1. Dans Unity, allez à `Tools > Save System > Show Save File Location`
2. Le dossier s'ouvrira automatiquement dans l'explorateur Windows

#### Méthode 2: Explorateur Windows
1. Appuyez sur `Windows + R`
2. Tapez: `%userprofile%\AppData\LocalLow`
3. Naviguez vers `[CompanyName]\[GameName]`

#### Méthode 3: Code
```csharp
Debug.Log(Application.persistentDataPath);
// Affiche: C:\Users\miste\AppData\LocalLow\MyStudio\SurvivorGame
```

### Pourquoi le fichier n'existe pas encore?

Le fichier `progression.json` n'est créé QUE quand:
1. Le joueur lance le jeu pour la première fois ET
2. Le `ProgressionManager` sauvegarde les données

**Dans l'Editor**, le fichier peut ne pas exister si:
- Vous n'avez jamais sauvegardé (pas de changement de progression)
- Le dossier `AppData\LocalLow\[Company]\[Game]` n'a pas été créé

**Solution**: Forcez une sauvegarde via:
- Unity Menu: `Tools > Save System > Force Create Default Save`
- Ou jouez jusqu'à gagner de l'or / débloquer quelque chose

---

## Menu de Debug Unity

Le menu `Tools > Save System` offre plusieurs commandes utiles:

| Commande | Description |
|----------|-------------|
| **Show Save File Location** | Affiche le chemin et ouvre le dossier |
| **Open Save Folder** | Ouvre le dossier de sauvegarde dans l'explorateur |
| **Delete Save File** | Supprime le fichier (avec confirmation) |
| **Create Test Save (1000 Gold)** | Crée une save de test avec 1000 gold et niveaux débloqués |
| **View Save File Content** | Affiche le contenu JSON de la save |
| **Force Create Default Save** | Crée une nouvelle save par défaut |

### Usage du Menu de Debug

#### Scenario 1: "Je veux voir où est ma save"
1. `Tools > Save System > Show Save File Location`
2. Le dossier s'ouvre, le fichier est visible

#### Scenario 2: "Je veux tester avec de l'or"
1. `Tools > Save System > Create Test Save (1000 Gold)`
2. Relancez le jeu → vous avez 1000 gold

#### Scenario 3: "Je veux reset ma progression"
1. `Tools > Save System > Delete Save File`
2. Confirmez
3. Relancez le jeu → nouvelle progression par défaut

#### Scenario 4: "Je veux voir ce qu'il y a dans ma save"
1. `Tools > Save System > View Save File Content`
2. Le JSON s'affiche dans les logs et une popup

---

## Format du Fichier de Sauvegarde

Le fichier `progression.json` est un fichier JSON lisible:

```json
{
  "gold": 150,
  "maxSpellSlots": 5,
  "runeUnlocks": [
    {
      "runeId": "Bolt",
      "isUnlocked": true,
      "maxLevel": 7
    },
    {
      "runeId": "Fire",
      "isUnlocked": true,
      "maxLevel": 5
    }
  ],
  "unlockedLevelIds": [
    "Level_Tutorial",
    "Level_Forest"
  ],
  "totalRunsCompleted": 3,
  "totalEnemiesKilled": 487,
  "bestRunTime": 234.5,
  "highestLevel": 12
}
```

### Modification Manuelle (Avancé)

Vous POUVEZ éditer ce fichier manuellement:
1. Fermez le jeu
2. Ouvrez `progression.json` avec un éditeur de texte
3. Modifiez les valeurs (respectez la syntaxe JSON!)
4. Sauvegardez
5. Relancez le jeu → changements appliqués

**Attention**: Si vous cassez la syntaxe JSON, le jeu utilisera une progression par défaut.

---

## Workflow de Test Rapide

### Test 1: Vérifier que la save fonctionne
```
1. Lancez le jeu
2. Gagnez de l'or (ou forcez via code/menu)
3. Fermez le jeu
4. Tools > Save System > View Save File Content
   → Vérifiez que le gold est sauvegardé
5. Relancez le jeu
   → Le gold doit être présent
```

### Test 2: Reset complet
```
1. Tools > Save System > Delete Save File
2. Relancez le jeu
   → Logs doivent afficher "No save file found. Creating default progression."
3. Vérifiez que la progression par défaut est correcte
```

### Test 3: Test avec progression avancée
```
1. Tools > Save System > Create Test Save (1000 Gold)
2. Relancez le jeu
3. Vérifiez que vous avez 1000 gold et 3 niveaux débloqués
4. Achetez des upgrades
5. Fermez et relancez
   → Upgrades doivent être sauvegardés
```

---

## Problèmes Courants

### "No save file found" à chaque lancement

**Causes possibles**:
1. Le dossier `AppData\LocalLow` n'a pas les bonnes permissions
2. Le jeu ne sauvegarde jamais (vérifiez que `ProgressionManager` existe)
3. Le chemin change (vérifiez Company/Product name dans Project Settings)

**Solutions**:
1. Vérifiez les permissions du dossier
2. Ajoutez des logs dans `SaveSystem.SaveProgression()`
3. Forcez une sauvegarde via `ProgressionManager.Instance.SaveProgression()`

### Le fichier existe mais les changements ne sont pas sauvegardés

**Causes possibles**:
1. `autoSaveOnChange = false` dans ProgressionManager
2. Vous modifiez la progression sans passer par ProgressionManager
3. Le jeu crash avant la sauvegarde

**Solutions**:
1. Activez `autoSaveOnChange` dans l'Inspector
2. Utilisez toujours `ProgressionManager.Instance.AwardGold()` etc.
3. Sauvegardez manuellement avant de quitter

### Le jeu utilise toujours la progression par défaut

**Cause**: Le fichier JSON est corrompu (syntaxe invalide)

**Solution**:
1. `Tools > Save System > View Save File Content`
2. Vérifiez qu'il n'y a pas d'erreur JSON
3. Si corrompu, supprimez et recréez

---

## Code Utile pour Debugging

### Afficher le chemin de sauvegarde
```csharp
Debug.Log($"Save path: {SaveSystem.GetSavePath()}");
Debug.Log($"Save exists: {SaveSystem.SaveExists()}");
```

### Forcer une sauvegarde
```csharp
if (ProgressionManager.Instance != null)
{
    ProgressionManager.Instance.SaveProgression();
}
```

### Donner de l'or en debug
```csharp
// Ajoutez un bouton UI ou une touche clavier
if (Input.GetKeyDown(KeyCode.F1))
{
    ProgressionManager.Instance?.AwardGold(1000);
    Debug.Log("Awarded 1000 gold!");
}
```

### Débloquer tout en debug
```csharp
[ContextMenu("Debug: Unlock All")]
private void DebugUnlockAll()
{
    if (ProgressionManager.Instance != null)
    {
        ProgressionManager.Instance.UnlockLevel("Level_Tutorial");
        ProgressionManager.Instance.UnlockLevel("Level_Forest");
        ProgressionManager.Instance.UnlockLevel("Level_Dungeon");
        ProgressionManager.Instance.AwardGold(10000);
        Debug.Log("All content unlocked!");
    }
}
```

---

## Résumé

### Pour Configurer la Progression Initiale
✅ Modifiez `PlayerProgressionData.CreateDefault()`

### Pour Trouver le Fichier de Save
✅ `Tools > Save System > Show Save File Location`

### Pour Reset la Progression
✅ `Tools > Save System > Delete Save File`

### Pour Tester avec de l'Or
✅ `Tools > Save System > Create Test Save (1000 Gold)`

C'est aussi simple que ça! 🎮
