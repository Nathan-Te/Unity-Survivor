# Setup Simple de Localisation (2 Minutes)

## C'est quoi ?

**1 fichier JSON par langue**, c'est tout ! Tu édites directement les fichiers avec n'importe quel éditeur de texte.

## Setup (2 étapes)

### 1. Créer le GameObject (30 secondes)

1. Dans ta scène, créer un GameObject vide
2. Le nommer `LocalizationManager`
3. Ajouter le composant `SimpleLocalizationManager`
4. C'est tout ✓

### 2. Les fichiers sont déjà créés ! (0 seconde)

Les fichiers sont dans `Assets/Resources/Localization/` :
- **en.json** - Anglais (déjà rempli)
- **fr.json** - Français (déjà rempli)

## Utilisation dans le code

### Remplacement Simple

```csharp
using SurvivorGame.Localization;

// ❌ AVANT
enemyCountText.text = $"Ennemis : {count}";

// ✅ APRÈS
enemyCountText.text = SimpleLocalizationHelper.FormatEnemyCount(count);
```

### Exemple Complet

```csharp
using SurvivorGame.Localization;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI enemyCountText;
    [SerializeField] private TextMeshProUGUI killCountText;
    [SerializeField] private TextMeshProUGUI levelText;

    private void Start()
    {
        // S'abonner aux changements de langue
        SimpleLocalizationManager.OnLanguageChanged += RefreshAllText;
    }

    private void UpdateEnemyCount(int count)
    {
        enemyCountText.text = SimpleLocalizationHelper.FormatEnemyCount(count);
    }

    private void UpdateKillCount(int count)
    {
        killCountText.text = SimpleLocalizationHelper.FormatKillCount(count);
    }

    private void UpdateLevel(int level)
    {
        levelText.text = SimpleLocalizationHelper.FormatLevel(level);
    }

    private void RefreshAllText()
    {
        // Rafraîchir tous les textes quand la langue change
        UpdateEnemyCount(_cachedEnemyCount);
        UpdateKillCount(_cachedKillCount);
        UpdateLevel(_cachedLevel);
    }

    private void OnDestroy()
    {
        SimpleLocalizationManager.OnLanguageChanged -= RefreshAllText;
    }
}
```

## Changer de Langue

```csharp
using SurvivorGame.Localization;

// Passer en anglais
SimpleLocalizationManager.Instance.SetLanguage(Language.English);

// Passer en français
SimpleLocalizationManager.Instance.SetLanguage(Language.French);
```

## Ajouter/Modifier des Textes

### Ouvre simplement le fichier JSON !

**Fichier :** `Assets/Resources/Localization/en.json`

```json
{
  "entries": [
    { "key": "HUD_ENEMIES", "value": "Enemies: {0}" },
    { "key": "HUD_KILLS", "value": "Kills: {0}" },

    { "key": "MY_NEW_KEY", "value": "My new text" }
  ]
}
```

**⚠️ IMPORTANT** : Les commentaires `//` ne sont PAS supportés dans Unity JSON !
- Ne pas utiliser `// commentaire`
- Utiliser seulement du JSON valide

Puis dans le code :

```csharp
string text = SimpleLocalizationHelper.Get("MY_NEW_KEY");
```

## Toutes les Clés Disponibles

### HUD
- `HUD_ENEMIES` - "Enemies: {0}" / "Ennemis : {0}"
- `HUD_KILLS` - "Kills: {0}" / "Kills : {0}"
- `HUD_SCORE` - "Score: {0:N0}" / "Score : {0:N0}"
- `HUD_COMBO` - "Combo x{0}" / "Combo x{0}"
- `HUD_MULTIPLIER` - "x{0:F1}" / "x{0:F1}"
- `HUD_LEVEL` - "LVL {0}" / "NIV {0}"
- `HUD_HEALTH` - "{0} / {1}" / "{0} / {1}"

### Level Up UI
- `LEVELUP_TITLE` - "LEVEL UP! Choose a reward"
- `LEVELUP_SPECIAL` - "SPECIAL REWARD!"
- `BAN_TITLE` - "BANISHMENT: Click on a card"
- `REROLL_COST` - "Reroll ({0})"
- `BAN_STOCK` - "Ban ({0})"

### Inventory
- `APPLY_ON` - "Apply {0} on?"
- `INCOMPATIBLE_FORM` - "Incompatible with this form!"
- `ERROR_ADD_MODIFIER` - "Error adding modifier"
- `REPLACE_MODIFIER` - "Replace which Modifier?"

### Stats
- `STAT_MOVE_SPEED` - "Speed" / "Vitesse"
- `STAT_MAX_HEALTH` - "Health" / "Santé"
- `STAT_GLOBAL_DAMAGE` - "Damage" / "Dégâts"
- Etc. (voir en.json pour la liste complète)

### Éléments
- `ELEMENT_FIRE` - "Fire" / "Feu"
- `ELEMENT_ICE` - "Ice" / "Glace"
- Etc.

### Rarités
- `RARITY_COMMON` - "Common" / "Commun"
- `RARITY_LEGENDARY` - "Legendary" / "Légendaire"
- Etc.

## Fonctions Helper Disponibles

### HUD
```csharp
SimpleLocalizationHelper.FormatEnemyCount(42)     // "Enemies: 42"
SimpleLocalizationHelper.FormatKillCount(100)     // "Kills: 100"
SimpleLocalizationHelper.FormatScore(12345)       // "Score: 12,345"
SimpleLocalizationHelper.FormatLevel(5)           // "LVL 5"
SimpleLocalizationHelper.FormatHealth(80, 100)    // "80 / 100"
```

### Level Up
```csharp
SimpleLocalizationHelper.GetLevelUpTitle()        // "LEVEL UP! Choose a reward"
SimpleLocalizationHelper.FormatRerollCost(3)      // "Reroll (3)"
```

### Enums
```csharp
SimpleLocalizationHelper.GetStatName(StatType.MoveSpeed)      // "Speed"
SimpleLocalizationHelper.GetElementName(ElementType.Fire)     // "Fire"
SimpleLocalizationHelper.GetRarityName(Rarity.Legendary)      // "Legendary"
```

### Combat
```csharp
SimpleLocalizationHelper.FormatBurn(10f, 3f)      // "Burn: 10/tick for 3s"
SimpleLocalizationHelper.FormatChain(5)           // "Chain x5"
SimpleLocalizationHelper.GetDamageLabel()         // "Damage"
```

## Ajouter une Nouvelle Langue

1. Créer `es.json` dans `Resources/Localization/`
2. Copier la structure de `en.json`
3. Traduire les valeurs
4. Ajouter `Spanish` dans `Language.cs` enum
5. Mettre à jour `GetLanguageFileName()` dans `SimpleLocalizationManager.cs`

C'est tout !

## Migration depuis l'ancien système

Si tu as déjà créé des LocalizationTable :
1. Ignorer les anciens fichiers
2. Utiliser `SimpleLocalizationManager` à la place
3. Remplacer `LocalizationHelper` par `SimpleLocalizationHelper`
4. Les clés sont les mêmes, juste le système est plus simple !

---

**C'est beaucoup plus simple, non ?** 😊

Un seul fichier JSON par langue, éditable avec n'importe quel éditeur de texte !
