# Game Over System - Guide Corrigé

## ✅ Corrections Apportées

### 1. Or collecté vs Or calculé
**AVANT (incorrect)**: Le système calculait l'or avec une formule `goldPerSecond × time + goldPerKill × kills`
**APRÈS (correct)**: Le système affiche l'or **réellement collecté** pendant la partie (delta entre début et fin)

### 2. Score ajouté
**NOUVEAU**: Affichage du score de `ArcadeScoreSystem` + sauvegarde dans `highestScore`

### 3. Sauvegarde dans tous les cas
**CORRECTION**: La progression est sauvegardée **automatiquement** dans `GameOverUI.Show()`, que le joueur clique sur Retry ou Main Menu

## Architecture Corrigée

```
PlayerController (dies)
    ↓ gather stats
timeSurvived, levelReached, enemiesKilled, SCORE
    ↓ fire event
GameEvents.OnPlayerDeath
    ↓ listened by
GameOverManager
    ↓ triggers
GameOverUI.Show()
    ↓
1. Calculate gold collected (session gold - initial gold)
2. Convert session gold → total gold (GoldManager)
3. Award gold to ProgressionManager
4. Record run stats (with SCORE)
5. SAVE PROGRESSION (critical!)
6. Display UI
    ↓ user clicks
[Retry] → GameStateManager.RestartGame()
[Main Menu] → SceneManager.LoadScene("MainMenu")
```

## Statistiques Affichées

| Stat | Source | Format |
|------|--------|--------|
| **Time Survived** | `GameTimer.Instance.ElapsedTime` | MM:SS |
| **Level Reached** | `LevelManager.Instance.currentLevel` | Int |
| **Enemies Killed** | `EnemyManager.Instance.TotalEnemiesKilled` | Int |
| **Score** | `ArcadeScoreSystem.Instance.TotalScore` | Int |
| **Gold Collected** | `GoldManager.Instance.CurrentSessionGold - _goldAtStartOfRun` | +Int |

## Gestion de l'Or (Corrigé)

### Flow de l'Or

```
1. Début de partie
   └─ GameOverUI.OnRunStart() enregistre l'or initial

2. Pendant la partie
   └─ GoldCoin collectées → GoldManager.AddGold()
   └─ GoldManager.CurrentSessionGold augmente

3. Fin de partie (Game Over)
   └─ goldCollected = CurrentSessionGold - _goldAtStartOfRun
   └─ GoldManager.ConvertSessionGoldToTotal()
   └─ ProgressionManager.AwardGold(delta)
   └─ ProgressionManager.SaveProgression() ← SAUVEGARDÉ!

4. Retry ou Main Menu
   └─ Or déjà sauvegardé, pas de perte!
```

### Exemple Concret

```
Début de partie:
- _goldAtStartOfRun = 0

Pendant la partie:
- Collecte 50 gold coins
- GoldManager.CurrentSessionGold = 50

Fin de partie (Game Over):
- goldCollected = 50 - 0 = 50
- Converti en total gold
- ProgressionManager.gold += 50
- SAUVEGARDÉ immédiatement

Retry:
- L'or est DÉJÀ sauvegardé
- Nouvelle partie démarre avec session gold = 0
```

## Setup dans Unity

### UI References (Mis à Jour)

```csharp
[Header("UI References")]
[SerializeField] private CanvasGroup canvasGroup;
[SerializeField] private TextMeshProUGUI titleText;
[SerializeField] private TextMeshProUGUI timeSurvivedText;
[SerializeField] private TextMeshProUGUI levelReachedText;
[SerializeField] private TextMeshProUGUI enemiesKilledText;
[SerializeField] private TextMeshProUGUI scoreText;            // ← NOUVEAU
[SerializeField] private TextMeshProUGUI goldCollectedText;    // ← RENOMMÉ
[SerializeField] private Button retryButton;
[SerializeField] private Button mainMenuButton;
```

### Structure UI (Mise à Jour)

```
GameOverPanel
├─ TitleText ("GAME OVER")
├─ StatsPanel
│  ├─ TimeSurvivedText
│  ├─ LevelReachedText
│  ├─ EnemiesKilledText
│  ├─ ScoreText           ← NOUVEAU
│  └─ GoldCollectedText   ← RENOMMÉ
└─ ButtonsPanel
   ├─ RetryButton
   └─ MainMenuButton
```

### Appeler OnRunStart()

**IMPORTANT**: Il faut appeler `GameOverUI.OnRunStart()` au début de chaque partie pour enregistrer l'or initial!

**Option 1 - Dans GameDirector/GameManager:**
```csharp
private void Start()
{
    var gameOverUI = FindFirstObjectByType<GameOverUI>();
    if (gameOverUI != null)
    {
        gameOverUI.OnRunStart();
    }
}
```

**Option 2 - Dans GameOverUI.Start() (auto):**
```csharp
private void Start()
{
    // ... autres initialisations

    // Record initial gold at start
    OnRunStart();
}
```

## Localisation (Mis à Jour)

```json
// en.json
{ "key": "GAMEOVER_SCORE", "value": "Score: {0}" }
{ "key": "GAMEOVER_GOLD", "value": "Gold Collected: +{0}" }

// fr.json
{ "key": "GAMEOVER_SCORE", "value": "Score : {0}" }
{ "key": "GAMEOVER_GOLD", "value": "Or Collecté : +{0}" }
```

## Données Sauvegardées

### PlayerProgressionData (Mis à Jour)

```csharp
[Header("Statistics")]
public int totalRunsCompleted = 0;
public int totalEnemiesKilled = 0;
public float bestRunTime = 0f;
public int highestLevel = 0;
public int highestScore = 0;  // ← NOUVEAU
```

### Fichier progression.json

```json
{
  "gold": 500,  // ← Or total persistant (augmente après chaque run)
  "maxSpellSlots": 5,
  "totalRunsCompleted": 10,
  "totalEnemiesKilled": 2500,
  "bestRunTime": 1234.5,
  "highestLevel": 25,
  "highestScore": 150000  // ← NOUVEAU meilleur score
}
```

## Code Important

### PlayerController.Die()

```csharp
// Get score from ArcadeScoreSystem
int score = 0;
if (ArcadeScoreSystem.Instance != null)
{
    score = ArcadeScoreSystem.Instance.TotalScore;
}

// Fire death event (with score)
GameEvents.OnPlayerDeath.Invoke(timeSurvived, levelReached, enemiesKilled, score);
```

### GameOverUI.Show()

```csharp
// Get gold collected during this run
int goldCollected = 0;
if (GoldManager.Instance != null)
{
    goldCollected = GoldManager.Instance.CurrentSessionGold - _goldAtStartOfRun;

    // Convert session gold to total (persistent gold)
    GoldManager.Instance.ConvertSessionGoldToTotal();
}

// Award the gold to ProgressionManager
if (ProgressionManager.Instance != null && GoldManager.Instance != null)
{
    ProgressionManager.Instance.AwardGold(GoldManager.Instance.TotalGold - ProgressionManager.Instance.CurrentProgression.gold);
}

// Record run statistics (including score)
ProgressionManager.Instance.RecordRunStats(enemiesKilled, timeSurvived, levelReached, score);

// SAVE PROGRESSION (critical for both Retry and Main Menu!)
ProgressionManager.Instance.SaveProgression();
```

## Testing

### Test 1: Vérifier l'affichage du score
1. Jouez et tuez des ennemis (le score augmente)
2. Mourez
3. Vérifiez que le score s'affiche correctement

### Test 2: Vérifier le gold collecté
1. Collectez 100 gold coins pendant la partie
2. Mourez
3. Game Over devrait afficher "+100" (pas un calcul, l'or réel)

### Test 3: Vérifier la sauvegarde (Retry)
1. Collectez 50 gold, mourez
2. Cliquez sur Retry
3. Retournez au Main Menu
4. L'or devrait être sauvegardé (+50 visible dans le menu)

### Test 4: Vérifier la sauvegarde (Main Menu direct)
1. Collectez 75 gold, mourez
2. Cliquez directement sur Main Menu
3. L'or devrait être sauvegardé (+75 visible dans le menu)

### Test 5: Vérifier le highestScore
1. Faites un run avec 10000 score
2. Mourez, vérifiez la sauvegarde
3. Faites un autre run avec 5000 score
4. `Tools > Save System > View Save File Content`
5. `highestScore` devrait toujours être 10000 (pas écrasé)

## Différences avec l'Ancienne Version

| Aspect | ❌ Ancien (incorrect) | ✅ Nouveau (correct) |
|--------|----------------------|---------------------|
| **Or** | Calculé avec formule | Or réellement collecté (delta) |
| **Score** | Pas affiché | Affiché + sauvegardé |
| **Sauvegarde Retry** | Pas clair si sauvegardé | **Toujours sauvegardé** |
| **Sauvegarde Main Menu** | Pas clair si sauvegardé | **Toujours sauvegardé** |
| **GoldManager** | Pas utilisé | Utilisé correctement |
| **Session Gold** | Ignoré | Converti en total gold |

## Résumé des Changements

1. ✅ **Supprimé**: `goldPerSecond`, `goldPerKill`, `goldPerLevel` (calcul incorrect)
2. ✅ **Ajouté**: Tracking de `_goldAtStartOfRun` pour calculer le delta
3. ✅ **Ajouté**: Paramètre `score` dans tous les events/méthodes
4. ✅ **Ajouté**: `highestScore` dans `PlayerProgressionData`
5. ✅ **Corrigé**: Utilisation de `GoldManager` pour convertir session → total
6. ✅ **Corrigé**: Sauvegarde **explicite** dans `GameOverUI.Show()` (avant affichage)
7. ✅ **Ajouté**: Méthode `OnRunStart()` pour tracker l'or initial

Le système est maintenant **correct et complet**! 🎉
