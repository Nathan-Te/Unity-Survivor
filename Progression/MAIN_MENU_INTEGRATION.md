# Main Menu → Game Scene Integration

## Problème Résolu: GameStateController Error

### Symptôme
Lors du lancement d'un niveau depuis le MainMenu, une erreur apparaissait:
```
[Singleton] An instance of 'GameStateController' is needed but none exists in the scene.
```

### Cause
`LevelSelectionUI` tentait d'accéder à `GameStateController.Instance` dans le MainMenu, mais ce singleton DontDestroyOnLoad n'existe que dans les scènes de jeu.

### Solution
Supprimé l'appel à `GameStateController.Instance.SetState()` dans `LevelSelectionUI.OnLevelSelected()`.

**Pourquoi ça fonctionne:**
- `GameStateController` s'initialise automatiquement avec `_currentState = GameState.Playing`
- Quand la scène de jeu se charge, le GameStateController est créé via son Awake() et est déjà en état `Playing`
- Pas besoin de modifier l'état depuis le MainMenu

## Flow de Navigation Corrigé

### MainMenu → Game Scene
```
1. User clique sur un niveau dans LevelSelectionUI
2. OnLevelSelected() vérifie que le niveau est débloqué
3. SceneManager.LoadScene(sceneName) charge la scène
4. GameStateController s'initialise automatiquement en état Playing
5. Le jeu démarre normalement
```

### Game Scene → MainMenu
```
1. Player meurt / quitte / redémarre
2. Code appelle SceneManager.LoadScene("MainMenu")
3. MainMenuUI.Start() s'exécute
4. Time.timeScale = 1f (au cas où on vient d'une scène en pause)
5. UI s'initialise normalement
```

## Autres Améliorations

### Time.timeScale Reset
Ajouté `Time.timeScale = 1f` dans `MainMenuUI.Start()` pour garantir que le temps n'est jamais figé dans le menu principal, même si on vient d'une scène de jeu en pause.

## Code Modifié

### LevelSelectionUI.cs
**AVANT:**
```csharp
// Reset game state before loading
if (GameStateController.Instance != null)
{
    GameStateController.Instance.SetState(GameStateController.GameState.Playing);
}

SceneManager.LoadScene(level.sceneName);
```

**APRÈS:**
```csharp
// Load the level scene
// Note: GameStateController will be initialized in the game scene
// and will automatically set state to Playing in its Awake/Start
SceneManager.LoadScene(level.sceneName);
```

### MainMenuUI.cs
**AJOUTÉ:**
```csharp
private void Start()
{
    // Ensure time is running (in case we came from a paused game scene)
    Time.timeScale = 1f;

    // ... rest of initialization
}
```

## Notes pour le Futur

### Singletons DontDestroyOnLoad vs Scene-Based

**DontDestroyOnLoad Singletons** (persistent entre scènes):
- `GameStateController`
- `GameStateManager`
- `ProgressionManager`
- `LevelManager`
- `GameTimer`
- `PlayerStats`

Ces singletons peuvent ne PAS exister dans le MainMenu. Toujours vérifier avec `?.` ou `!= null`.

**Scene-Based Singletons** (détruits lors du changement de scène):
- `PlayerController`
- `EnemyManager`
- Tous les pools (ProjectilePool, EnemyPool, etc.)
- `MapManager`
- UI managers de jeu

Ces singletons n'existent QUE dans leur scène respective.

### Best Practice: Accès aux Singletons depuis MainMenu

```csharp
// ✅ CORRECT - Safe null check
if (ProgressionManager.Instance != null)
{
    var data = ProgressionManager.Instance.CurrentProgression;
}

// ✅ CORRECT - Null-conditional operator
var gold = ProgressionManager.Instance?.CurrentProgression?.gold ?? 0;

// ❌ INCORRECT - Déclenche l'erreur du Singleton si n'existe pas
if (GameStateController.Instance != null)  // ← Log d'erreur si pas trouvé!
{
    // ...
}
```

**Note**: Le simple fait d'accéder à `.Instance` déclenche `FindFirstObjectByType<T>()` qui log une erreur si non trouvé. C'est pour ça qu'on évite complètement d'y accéder depuis le MainMenu pour les singletons de jeu.

## Testing Checklist

- [x] Lancer le jeu → MainMenu s'affiche sans erreur
- [x] Cliquer sur un niveau débloqué → Scene se charge sans erreur
- [x] Jouer → GameStateController fonctionne normalement
- [x] Retourner au MainMenu → Pas de freeze (time scale = 1)
- [x] Relancer un niveau → Fonctionne normalement
- [x] Tester pause dans le jeu → Fonctionne
- [x] Retourner au menu depuis une pause → Menu fonctionne

## Résumé

L'erreur est résolue en respectant la séparation des responsabilités:
- **MainMenu** = Gestion de la navigation et de la progression méta
- **Game Scenes** = Gestion de l'état de jeu via GameStateController

Le MainMenu ne doit JAMAIS accéder aux singletons spécifiques au gameplay (GameStateController, PlayerController, etc.), seulement aux singletons de méta-progression (ProgressionManager).
