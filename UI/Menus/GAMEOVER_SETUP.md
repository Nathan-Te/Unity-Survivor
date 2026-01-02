# Game Over System - Setup Guide

## Overview

Le système de Game Over affiche un récapitulatif de la partie avec:
- ✅ Temps de survie
- ✅ Niveau atteint
- ✅ Ennemis tués
- ✅ Or gagné (calculé automatiquement)
- ✅ Boutons Retry (relancer) et Main Menu (retour menu)

## Architecture

```
PlayerController (dies)
    ↓ (fires event)
GameEvents.OnPlayerDeath
    ↓ (listened by)
GameOverManager
    ↓ (triggers)
GameOverUI (shows stats + buttons)
    ↓ (user clicks)
Retry → GameStateManager.RestartGame()
Main Menu → SceneManager.LoadScene("MainMenu")
```

## Fichiers Créés

1. **[GameOverUI.cs](GameOverUI.cs)** - UI du Game Over avec stats et navigation
2. **[GameOverManager.cs](../../../Core/GameOverManager.cs)** - Coordonne player death → UI display
3. **[GameEvents.cs](../../../Core/GameEvents.cs)** - Système d'events global
4. **PlayerController.cs** - Modifié pour déclencher l'event OnPlayerDeath

## Setup dans Unity

### 1. Créer le GameObject GameOverManager

Dans votre scène de jeu:

```
Hierarchy:
└─ UI
   ├─ PlayerHUD
   ├─ LevelUpUI
   └─ GameOverCanvas ← NEW
      ├─ GameOverManager (script)
      └─ GameOverPanel
         ├─ TitleText
         ├─ StatsPanel
         │  ├─ TimeSurvivedText
         │  ├─ LevelReachedText
         │  ├─ EnemiesKilledText
         │  └─ GoldEarnedText
         └─ ButtonsPanel
            ├─ RetryButton
            └─ MainMenuButton
```

### 2. Configurer GameOverCanvas

**Canvas Component:**
- Render Mode: Screen Space - Overlay
- Sort Order: 100 (au-dessus de tout)

**CanvasGroup Component:**
- Interactable: ✓
- Block Raycasts: ✓
- Ignore Parent Groups: ✗
- Alpha: 0 (caché par défaut)

### 3. Configurer GameOverManager

**Inspector Settings:**
- **Game Over UI**: Drag GameOverPanel's GameOverUI script here
- **Verbose Logging**: ✓ (pour debugging)

### 4. Configurer GameOverUI

**UI References:**
- **Canvas Group**: Le CanvasGroup du panel
- **Title Text**: "GAME OVER" TextMeshProUGUI
- **Time Survived Text**: "Time: 00:00" TextMeshProUGUI
- **Level Reached Text**: "Level: 1" TextMeshProUGUI
- **Enemies Killed Text**: "Kills: 0" TextMeshProUGUI
- **Gold Earned Text**: "Gold: +0" TextMeshProUGUI
- **Retry Button**: Button component
- **Main Menu Button**: Button component

**Gold Calculation Settings:**
- **Gold Per Second**: 0.5 (par défaut, ajustez au besoin)
- **Gold Per Kill**: 1 (par défaut, ajustez au besoin)
- **Gold Per Level**: 10 (par défaut, ajustez au besoin)

### 5. Styliser l'UI (Recommandations)

#### Title Text
- Font Size: 72
- Color: Rouge/Orange (#FF4444)
- Alignment: Center
- Font Style: Bold

#### Stats Text
- Font Size: 24
- Color: Blanc
- Alignment: Left

#### Retry Button
- Color: Vert (#44FF44)
- Text: "Retry" (auto-localisé)

#### Main Menu Button
- Color: Bleu (#4444FF)
- Text: "Main Menu" (auto-localisé)

## Calcul de l'Or

L'or est calculé automatiquement selon la formule:

```
Gold = (Time × GoldPerSecond) + (Kills × GoldPerKill) + (Level × GoldPerLevel)
```

### Exemples

**Run Court:**
- Time: 120s (2 minutes)
- Level: 5
- Kills: 50

Gold = (120 × 0.5) + (50 × 1) + (5 × 10) = 60 + 50 + 50 = **160 gold**

**Run Moyen:**
- Time: 600s (10 minutes)
- Level: 15
- Kills: 250

Gold = (600 × 0.5) + (250 × 1) + (15 × 10) = 300 + 250 + 150 = **700 gold**

**Run Long:**
- Time: 1800s (30 minutes)
- Level: 30
- Kills: 1000

Gold = (1800 × 0.5) + (1000 × 1) + (30 × 10) = 900 + 1000 + 300 = **2200 gold**

### Ajuster les Valeurs

Pour rendre le jeu plus/moins généreux:

```csharp
// Dans GameOverUI Inspector
goldPerSecond = 1.0f;   // Double l'or du temps
goldPerKill = 2.0f;     // Double l'or des kills
goldPerLevel = 20f;     // Double l'or des levels
```

## Flow Complet

### 1. Player Dies
```csharp
// PlayerController.cs
if (_currentHp <= 0)
{
    Die(); // Nouvelle méthode
}
```

### 2. Gather Statistics
```csharp
// PlayerController.Die()
float timeSurvived = GameTimer.Instance.ElapsedTime;
int levelReached = LevelManager.Instance.currentLevel;
int enemiesKilled = EnemyManager.Instance.TotalEnemiesKilled;
```

### 3. Fire Event
```csharp
// PlayerController.Die()
GameEvents.OnPlayerDeath.Invoke(timeSurvived, levelReached, enemiesKilled);
```

### 4. GameOverManager Receives Event
```csharp
// GameOverManager.OnPlayerDeath()
GameStateController.Instance.Pause(); // Stop gameplay
gameOverUI.Show(timeSurvived, levelReached, enemiesKilled);
```

### 5. GameOverUI Displays
```csharp
// GameOverUI.Show()
int goldEarned = CalculateGoldEarned(...);
ProgressionManager.Instance.AwardGold(goldEarned);
ProgressionManager.Instance.RecordRunStats(...);
DisplayStatistics(...);
SetVisible(true);
```

### 6. User Clicks Button

**Retry:**
```csharp
GameStateManager.RestartGame();
// → Full restart with progression reset
```

**Main Menu:**
```csharp
Time.timeScale = 1f;
SceneManager.LoadScene("MainMenu");
// → Return to menu with gold saved
```

## Localisation

Toutes les clés sont dans `en.json` / `fr.json`:

```json
{ "key": "GAMEOVER_TITLE", "value": "GAME OVER" }
{ "key": "GAMEOVER_TIME", "value": "Time Survived: {0}" }
{ "key": "GAMEOVER_LEVEL", "value": "Level Reached: {0}" }
{ "key": "GAMEOVER_KILLS", "value": "Enemies Killed: {0}" }
{ "key": "GAMEOVER_GOLD", "value": "Gold Earned: {0}" }
{ "key": "GAMEOVER_RETRY", "value": "Retry" }
{ "key": "GAMEOVER_MAINMENU", "value": "Main Menu" }
```

## Testing

### Test 1: Vérifier que le Game Over s'affiche
1. Lancez le jeu
2. Laissez-vous tuer par un ennemi
3. L'écran Game Over devrait apparaître avec vos stats

### Test 2: Vérifier le calcul de gold
1. Jouez pendant 2 minutes, tuez 50 ennemis, atteignez niveau 5
2. Mourez
3. Vérifiez que le gold affiché est cohérent (environ 160 avec les valeurs par défaut)

### Test 3: Bouton Retry
1. Cliquez sur Retry
2. Le niveau devrait redémarrer complètement
3. Votre progression (gold gagné) devrait être sauvegardée

### Test 4: Bouton Main Menu
1. Cliquez sur Main Menu
2. Vous devriez retourner au MainMenu
3. Le gold gagné devrait être visible dans le menu

### Test 5: Vérifier la sauvegarde
1. Mourez et gagnez de l'or
2. Retournez au Main Menu
3. Fermez le jeu
4. Relancez → Le gold devrait être sauvegardé

## Personnalisation Avancée

### Ajouter des Stats Supplémentaires

```csharp
// Dans GameOverUI.cs
[SerializeField] private TextMeshProUGUI wavesCompletedText;

// Dans Show()
int wavesCompleted = GetWavesCompleted();
wavesCompletedText.text = SimpleLocalizationHelper.GetFormatted("GAMEOVER_WAVES", wavesCompleted);
```

### Changer la Formule de Gold

```csharp
// Dans GameOverUI.CalculateGoldEarned()
private int CalculateGoldEarned(...)
{
    // Formule custom
    float bonus = levelReached > 20 ? 1.5f : 1.0f; // Bonus pour haut niveau
    int baseGold = Mathf.RoundToInt(timeSurvived * goldPerSecond);
    int killGold = Mathf.RoundToInt(enemiesKilled * goldPerKill * bonus);

    return baseGold + killGold;
}
```

### Ajouter des Achievements

```csharp
// Dans GameOverUI.Show()
if (timeSurvived > 600) // 10 minutes
{
    UnlockAchievement("SURVIVOR_10MIN");
}

if (enemiesKilled > 1000)
{
    UnlockAchievement("KILLER_1000");
}
```

## Troubleshooting

### Game Over ne s'affiche pas

**Causes possibles:**
1. GameOverManager pas dans la scène
2. GameOverUI reference manquante
3. Event pas déclenché

**Solutions:**
1. Vérifiez que GameOverManager existe dans la scène
2. Vérifiez que gameOverUI est assigné dans l'Inspector
3. Ajoutez des logs dans `PlayerController.Die()` et `GameOverManager.OnPlayerDeath()`

### Stats incorrectes

**Causes:**
1. Singletons manquants (GameTimer, LevelManager, EnemyManager)
2. Valeurs par défaut utilisées

**Solution:**
```csharp
// Dans PlayerController.Die(), ajoutez des logs
Debug.Log($"Time: {timeSurvived}, Level: {levelReached}, Kills: {enemiesKilled}");
```

### Gold pas sauvegardé

**Cause:** ProgressionManager.autoSaveOnChange = false

**Solution:**
1. Sélectionnez ProgressionManager dans la scène
2. Activez "Auto Save On Change" dans l'Inspector
3. Ou appelez manuellement `ProgressionManager.Instance.SaveProgression()`

### Boutons ne fonctionnent pas

**Cause:** CanvasGroup.blocksRaycasts = false

**Solution:**
Vérifiez que SetVisible(true) active bien `blocksRaycasts = true`

## Résumé: Checklist de Setup

- [ ] Créer GameOverCanvas avec CanvasGroup
- [ ] Créer GameOverPanel avec tous les TextMeshProUGUI
- [ ] Ajouter GameOverUI script au panel
- [ ] Créer et configurer les boutons (Retry, Main Menu)
- [ ] Créer GameOverManager dans la scène
- [ ] Assigner gameOverUI dans GameOverManager
- [ ] Tester que le Game Over s'affiche à la mort
- [ ] Vérifier le calcul de gold
- [ ] Tester Retry et Main Menu
- [ ] Vérifier la sauvegarde du gold

C'est prêt! 🎮
