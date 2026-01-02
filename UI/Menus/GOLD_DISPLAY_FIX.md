# Gold Display Fix - Solution Définitive

## ❌ Problème Identifié

Le texte de l'or (`goldText`) est **vidé** même si le code l'assigne correctement.

### Cause Racine

Le TextMeshProUGUI pour l'affichage de l'or a probablement un composant **`SimpleLocalizedText`** ou **`LocalizedTextMeshPro`** attaché.

Ces composants appellent `UpdateText()` dans leur `OnEnable()`, ce qui **écrase** le texte assigné par `MainMenuUI`.

## ✅ Solution

### Option 1: Retirer le Composant de Localisation (RECOMMANDÉ)

1. Dans Unity, sélectionnez le GameObject contenant le TextMeshProUGUI pour l'or
2. Dans l'Inspector, vérifiez s'il y a un composant `SimpleLocalizedText` ou `LocalizedTextMeshPro`
3. **SUPPRIMEZ** ce composant (clic droit → Remove Component)
4. Relancez le jeu

**Pourquoi?** L'or est un **texte dynamique** basé sur les données de sauvegarde, PAS un texte statique localisé.

### Option 2: Désactiver le Composant

Si vous ne pouvez pas supprimer le composant:

1. Décochez la case "Enabled" du composant `SimpleLocalizedText`
2. Cela empêchera `OnEnable()` d'être appelé

## 🔍 Vérification

### Dans Unity Inspector

Sélectionnez le GameObject avec `goldText` et vérifiez qu'il a:

```
✅ TextMeshProUGUI (requis)
❌ SimpleLocalizedText (NE DOIT PAS ÊTRE PRÉSENT)
❌ LocalizedTextMeshPro (NE DOIT PAS ÊTRE PRÉSENT)
```

### Dans le Code

Le composant `SimpleLocalizedText` fait ceci dans `OnEnable()`:

```csharp
private void OnEnable()
{
    SimpleLocalizationManager.OnLanguageChanged += UpdateText;
    UpdateText();  // ← ÉCRASE LE TEXTE ICI!
}

public void UpdateText()
{
    // Si _key est vide ou incorrect, le texte devient vide ou faux
    _textComponent.text = SimpleLocalizationManager.Instance.GetString(_key, _key);
}
```

## 📋 Textes Statiques vs Dynamiques

### Textes Statiques (Utiliser SimpleLocalizedText)

Textes qui **ne changent jamais** sauf lors du changement de langue:

- Titres de menus ("MAIN MENU", "SETTINGS")
- Labels de boutons ("PLAY", "QUIT")
- Titres de sections ("STATISTICS", "UPGRADES")

**Pattern**:
```csharp
// Dans l'Inspector du TextMeshProUGUI:
// Ajoutez SimpleLocalizedText
// key = "MENU_TITLE"
```

### Textes Dynamiques (NE PAS utiliser SimpleLocalizedText)

Textes qui **changent en fonction de données**:

- **Gold** (change selon la sauvegarde)
- Score (change pendant le jeu)
- Niveau du joueur (change pendant le jeu)
- Compteurs d'ennemis (change pendant le jeu)
- Timer (change chaque frame)

**Pattern**:
```csharp
// Dans le code C#:
goldText.text = SimpleLocalizationHelper.FormatGold(goldAmount);
// PAS de SimpleLocalizedText sur le GameObject!
```

## 🛠️ Cas Spécial: Texte avec Format ET Localisation

Si vous voulez "Gold: {0}" localisé + valeur dynamique:

### ❌ Mauvais
```csharp
// NE PAS faire ceci
goldText.text = "Gold: " + goldAmount; // Pas localisé!
```

### ✅ Bon
```csharp
// Helper qui utilise HUD_GOLD du JSON
goldText.text = SimpleLocalizationHelper.FormatGold(goldAmount);
// Résultat: "Gold: 50" (anglais) ou "Or : 50" (français)
```

Le helper utilise la clé de localisation en interne, mais le **texte final reste dynamique**.

## 🔧 Correction du Validateur

Le validateur créé précédemment (`MainMenuUIValidator.cs`) devrait aussi vérifier l'absence de `SimpleLocalizedText`:

```csharp
// Vérifier que goldText n'a PAS de SimpleLocalizedText
var goldTextGO = (goldTextProperty.objectReferenceValue as TMPro.TextMeshProUGUI)?.gameObject;
if (goldTextGO != null)
{
    var localizedComponent = goldTextGO.GetComponent<SimpleLocalizedText>();
    if (localizedComponent != null)
    {
        EditorGUILayout.HelpBox(
            "WARNING: Gold Text has a SimpleLocalizedText component attached! " +
            "This will overwrite the dynamic gold value. Remove this component.",
            MessageType.Error
        );
    }
}
```

## 📝 Résumé de la Procédure

1. **Ouvrez la scène MainMenu**
2. **Trouvez le GameObject** avec le TextMeshProUGUI pour l'or
3. **Vérifiez l'Inspector**:
   - Si vous voyez `SimpleLocalizedText` ou `LocalizedTextMeshPro`
   - **Supprimez ce composant**
4. **Sauvegardez la scène**
5. **Relancez le jeu**
6. **Vérifiez** que l'or s'affiche maintenant

## 🧪 Test de Validation

Après avoir supprimé le composant:

1. Lancez le jeu
2. Ouvrez la console
3. Vous devriez voir:
```
[ProgressionManager] Loaded progression: Gold=50
[MainMenuUI] Updated gold display: 50 -> 'Gold: 50' (current text: 'Gold: 50')
[MainMenuUI] Gold text after 1 frame: 'Gold: 50' (should NOT be empty)
```

4. **Si vous voyez** `Gold text was CLEARED after assignment`:
   - Le composant `SimpleLocalizedText` est toujours présent
   - Retournez dans Unity et supprimez-le

## ⚠️ Note Importante

**Ne mettez JAMAIS** de composant `SimpleLocalizedText` sur des TextMeshProUGUI qui affichent:
- Des valeurs numériques (or, score, vie, etc.)
- Des timers
- Des compteurs
- Des statistiques dynamiques

Ces textes sont gérés **par le code C#** qui utilise les helpers de localisation (`SimpleLocalizationHelper.FormatGold`, etc.).
