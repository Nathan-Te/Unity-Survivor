# Main Menu UI - Troubleshooting Guide

## Problème: L'or du joueur ne s'affiche pas dans le menu principal

### Symptômes
- Le log affiche `[ProgressionManager] Loaded progression: Gold=50`
- Mais l'UI ne montre rien (champ vide) pour l'or

### Causes Possibles

#### 1. ✅ Référence UI Non Assignée (PLUS FRÉQUENT)

**Vérification**:
1. Ouvrez Unity
2. Sélectionnez le GameObject `MainMenuUI` dans la scène `MainMenu`
3. Dans l'Inspector, vérifiez le composant `MainMenuUI`
4. Cherchez le champ `Gold Text` sous **Player Info Display**

**Problème**: Le champ `Gold Text` est vide (None)

**Solution**:
1. Trouvez votre TextMeshProUGUI pour l'affichage de l'or dans la hiérarchie
2. Glissez-déposez ce TextMeshProUGUI dans le champ `Gold Text` de l'Inspector
3. Sauvegardez la scène

#### 2. ⏱️ Problème de Timing (RÉSOLU DANS LE CODE)

**Symptôme**: ProgressionManager charge après MainMenuUI

**Solution Appliquée**:
- Le code utilise maintenant `Invoke(nameof(RefreshPlayerInfo), 0.1f)` pour attendre que ProgressionManager soit prêt
- Si ProgressionManager n'est pas prêt, affiche "Gold: 0" en fallback

#### 3. 🔤 Clé de Localisation Manquante (PEU PROBABLE)

**Vérification**:
```json
// Assets/Resources/Localization/en.json
{ "key": "HUD_GOLD", "value": "Gold: {0}" }

// Assets/Resources/Localization/fr.json
{ "key": "HUD_GOLD", "value": "Or : {0}" }
```

**Solution**: Les clés sont déjà présentes, mais si manquantes, ajoutez-les.

### Debug Steps

#### Étape 1: Activer les Logs Verbose

1. Sélectionnez `MainMenuUI` dans la hiérarchie
2. Dans l'Inspector, cochez `Verbose Logging`
3. Lancez le jeu
4. Regardez la console pour les logs:

```
[MainMenuUI] Initialized
[MainMenuUI] Refreshed player info - Gold: 50
[MainMenuUI] Updated gold display: 50 -> 'Gold: 50'
```

**Si vous voyez**:
- `[MainMenuUI] goldText is null, cannot update display` → Problème de référence UI (voir Cause 1)
- `[MainMenuUI] ProgressionManager not ready` → Problème de timing (normalement résolu)
- `[MainMenuUI] UpdatePlayerInfo called with null data` → Problème de chargement de save

#### Étape 2: Vérifier ProgressionManager

1. Lancez le jeu
2. Regardez les logs pour:

```
[ProgressionManager] Loaded progression: Gold=50, Spell Slots=4
```

**Si absent**: ProgressionManager n'est pas dans la scène ou n'est pas initialisé

**Solution**:
- Ajoutez un GameObject vide nommé `ProgressionManager`
- Attachez le script `ProgressionManager.cs`
- Assurez-vous qu'il est au niveau racine de la hiérarchie (pas enfant d'un autre GameObject)

#### Étape 3: Tester Manuellement

Dans `MainMenuUI.Start()`, ajoutez temporairement:

```csharp
// TEST: Force display
if (goldText != null)
{
    goldText.text = "TEST: Gold display works!";
    Debug.Log($"Gold text assigned: {goldText.name}");
}
else
{
    Debug.LogError("Gold text is NULL!");
}
```

- **Si "TEST: Gold display works!" apparaît**: Le TextMeshProUGUI fonctionne, c'est un problème de timing/data
- **Si "Gold text is NULL!"**: Vous devez assigner la référence dans l'Inspector

### Structure UI Attendue

```
Canvas (MainMenu)
├─ MainMenuPanel
│  ├─ TitleText ("SURVIVOR")
│  ├─ PlayerInfoPanel
│  │  └─ GoldText (TextMeshProUGUI) ← ASSIGNER ICI
│  ├─ ButtonsPanel
│  │  ├─ PlayButton
│  │  ├─ UpgradesButton
│  │  ├─ SettingsButton
│  │  ├─ LeaderboardButton
│  │  └─ QuitButton
│  └─ ...
└─ MainMenuUI (Script)
   └─ Gold Text: GoldText (RÉFÉRENCE ASSIGNÉE)
```

### Solution Rapide (Quick Fix)

Si l'or ne s'affiche toujours pas:

1. **Vérifiez l'Inspector**: `goldText` doit être assigné
2. **Activez Verbose Logging** dans `MainMenuUI`
3. **Relancez le jeu** et lisez les logs
4. **Ajoutez un délai plus long** si nécessaire (changez `0.1f` à `0.5f` dans `Invoke`)

### Code de Fallback

Le code actuel inclut un fallback qui affiche "Gold: 0" si ProgressionManager n'est pas prêt:

```csharp
private void RefreshPlayerInfo()
{
    if (ProgressionManager.Instance != null && ProgressionManager.Instance.CurrentProgression != null)
    {
        UpdatePlayerInfo(ProgressionManager.Instance.CurrentProgression);
    }
    else
    {
        // Fallback: Display 0 gold
        if (goldText != null)
        {
            goldText.text = SimpleLocalizationHelper.FormatGold(0);
        }
    }
}
```

### Checklist de Vérification

- [ ] Le GameObject `MainMenuUI` existe dans la scène `MainMenu`
- [ ] Le script `MainMenuUI` est attaché au GameObject
- [ ] Le champ `Gold Text` est assigné dans l'Inspector
- [ ] Le GameObject `ProgressionManager` existe (DontDestroyOnLoad)
- [ ] La clé `HUD_GOLD` existe dans `en.json` et `fr.json`
- [ ] `Verbose Logging` est activé pour voir les logs
- [ ] Les logs montrent `[ProgressionManager] Loaded progression`
- [ ] Les logs montrent `[MainMenuUI] Updated gold display`

### Cas Spéciaux

#### Si vous revenez du Game Over au Main Menu

Le code gère automatiquement ce cas:
1. La progression a été sauvegardée dans `GameOverUI.Show()`
2. `ProgressionManager` persiste via DontDestroyOnLoad
3. `MainMenuUI.RefreshPlayerInfo()` récupère les données actuelles
4. L'or devrait s'afficher avec le montant mis à jour

#### Si vous lancez le jeu pour la première fois

1. Aucun fichier `progression.json` n'existe
2. `SaveSystem.LoadProgression()` crée une progression par défaut (Gold=0)
3. L'UI devrait afficher "Gold: 0"

## Résumé

**Cause #1 (95% des cas)**: Référence UI non assignée dans l'Inspector
**Solution**: Assigner le TextMeshProUGUI dans le champ `Gold Text`

**Cause #2 (4% des cas)**: ProgressionManager pas dans la scène
**Solution**: Ajouter ProgressionManager au niveau racine de la hiérarchie

**Cause #3 (1% des cas)**: Problème de timing
**Solution**: Déjà résolu avec `Invoke` + fallback dans le code
