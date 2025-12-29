# 🎉 Système de Localisation SIMPLIFIÉ

## Changement Majeur

**AVANT** : Système complexe avec des dizaines de ScriptableObjects à créer manuellement
**MAINTENANT** : **1 fichier JSON par langue**, c'est tout !

## Fichiers Créés

### Nouveau Système Simple ✅

1. **SimpleLocalizationManager.cs** - Manager qui charge depuis JSON
2. **SimpleLocalizationHelper.cs** - Fonctions helper simplifiées
3. **SimpleLocalizedText.cs** - Composant UI auto-update
4. **SimpleLanguageSelector.cs** - Sélecteur de langue avec boutons

### Fichiers JSON (DÉJÀ REMPLIS !) ✅

- **en.json** - Toutes les chaînes en anglais (77 entrées)
- **fr.json** - Toutes les chaînes en français (77 entrées)

### Documentation

- **SIMPLE_SETUP.md** - Guide de setup rapide (2 minutes)

## Ce Qui Change

### Pour les Textes UI (hardcodés)

**AVANT (ancien système)** :
1. Créer 3 LocalizationTable ScriptableObjects
2. Remplir manuellement chaque entrée dans l'Inspector
3. Utiliser `LocalizationHelper.FormatEnemyCount(count)`

**MAINTENANT (nouveau système)** :
1. Éditer directement `en.json` et `fr.json`
2. Utiliser `SimpleLocalizationHelper.FormatEnemyCount(count)`

### Pour les Noms/Descriptions (ScriptableObjects)

**AUCUN CHANGEMENT** ❗
- RuneSO.runeName, RuneDefinition.Description, EnemyData.enemyName restent des `LocalizedString`
- Tu dois toujours créer des ScriptableObjects pour eux
- L'avantage : support multilingue pour le contenu data-driven

## Structure JSON

### Fichier : `Assets/Resources/Localization/en.json`

```json
{
  "entries": [
    { "key": "HUD_ENEMIES", "value": "Enemies: {0}" },
    { "key": "HUD_KILLS", "value": "Kills: {0}" },
    { "key": "HUD_LEVEL", "value": "LVL {0}" },
    { "key": "STAT_MOVE_SPEED", "value": "Speed" }
  ]
}
```

**Simple, non ?** Édite directement avec VS Code, Notepad++, ou n'importe quel éditeur !

## Les 77 Clés Disponibles

### HUD (7 clés)
- HUD_ENEMIES, HUD_KILLS, HUD_SCORE, HUD_COMBO, HUD_MULTIPLIER, HUD_LEVEL, HUD_HEALTH

### Level Up UI (7 clés)
- LEVELUP_TITLE, LEVELUP_SPECIAL, LEVELUP_CHOOSE, BAN_TITLE, BAN_CHOOSE, REROLL_COST, BAN_STOCK

### Inventory UI (4 clés)
- APPLY_ON, INCOMPATIBLE_FORM, ERROR_ADD_MODIFIER, REPLACE_MODIFIER

### Tooltips (5 clés)
- TOOLTIP_LEVEL, TOOLTIP_MAX_LEVEL, TOOLTIP_TYPE, TOOLTIP_TARGET, TOOLTIP_STAT_UPGRADE

### Stats (13 clés)
- STAT_MOVE_SPEED, STAT_MAX_HEALTH, STAT_HEALTH_REGEN, STAT_ARMOR, STAT_MAGNET_AREA, STAT_EXPERIENCE_GAIN
- STAT_GLOBAL_DAMAGE, STAT_GLOBAL_COOLDOWN, STAT_GLOBAL_AREA, STAT_GLOBAL_SPEED, STAT_GLOBAL_COUNT
- STAT_CRIT_CHANCE, STAT_CRIT_DAMAGE

### Éléments (5 clés)
- ELEMENT_PHYSICAL, ELEMENT_FIRE, ELEMENT_ICE, ELEMENT_LIGHTNING, ELEMENT_NECROTIC

### Rarités (4 clés)
- RARITY_COMMON, RARITY_RARE, RARITY_EPIC, RARITY_LEGENDARY

### Combat Effects (6 clés)
- EFFECT_BURN, EFFECT_SLOW, EFFECT_CHAIN, EFFECT_AOE, EFFECT_SUMMON, EFFECT_HOMING

### Combat Labels (13 clés)
- LABEL_DAMAGE, LABEL_COOLDOWN, LABEL_COUNT, LABEL_PIERCE, LABEL_SPREAD, LABEL_RANGE
- LABEL_CRIT_CHANCE, LABEL_CRIT_DAMAGE, LABEL_SIZE, LABEL_SPEED, LABEL_DURATION
- LABEL_KNOCKBACK, LABEL_MULTICAST

## Setup Rapide (2 Minutes)

### 1. Créer le Manager (30 secondes)

1. GameObject vide → "LocalizationManager"
2. Add Component → `SimpleLocalizationManager`
3. Done!

### 2. Tester (30 secondes)

```csharp
using SurvivorGame.Localization;

// Dans ton script UI
enemyCountText.text = SimpleLocalizationHelper.FormatEnemyCount(42);
// Résultat : "Enemies: 42" (EN) ou "Ennemis : 42" (FR)
```

### 3. Changer de Langue (10 secondes)

```csharp
// Passer en français
SimpleLocalizationManager.Instance.SetLanguage(Language.French);

// Tous les textes se mettent à jour automatiquement !
```

## Ajouter une Nouvelle Clé

### Dans le JSON :

```json
{
  "entries": [
    // ... autres entrées ...
    { "key": "MY_NEW_KEY", "value": "My new text: {0}" }
  ]
}
```

### Dans le Code :

```csharp
string text = SimpleLocalizationHelper.Get("MY_NEW_KEY");
// ou avec formatage
string text = SimpleLocalizationHelper.GetFormatted("MY_NEW_KEY", myValue);
```

## Ajouter une Nouvelle Langue

1. Créer `es.json` dans `Resources/Localization/`
2. Copier la structure de `en.json`
3. Traduire toutes les valeurs
4. Ajouter `Spanish` dans `Language.cs` enum
5. Mettre à jour `GetLanguageFileName()` dans `SimpleLocalizationManager.cs` :

```csharp
Language.Spanish => "es",
```

C'est tout ! Pas de ScriptableObjects à créer ! 🎉

## Composant UI Auto-Update

Pour un texte statique qui s'update automatiquement :

1. Ajouter `SimpleLocalizedText` sur ton TextMeshPro
2. Dans l'Inspector, définir Key = "LEVELUP_TITLE"
3. Le texte se met à jour automatiquement quand tu changes de langue !

## Migration depuis l'Ancien Système

Si tu as déjà commencé avec le système complexe :

### Ce qui reste identique ✅
- `LocalizedString` pour les ScriptableObjects (runes, enemies)
- `Language.cs` enum
- Event `OnLanguageChanged`

### Ce qui change 🔄
- Remplacer `LocalizationHelper` par `SimpleLocalizationHelper`
- Remplacer `LocalizationManager` par `SimpleLocalizationManager`
- Les ScriptableObjects LocalizationTable ne sont plus nécessaires
- Tout est dans `en.json` et `fr.json` maintenant !

## Avantages du Nouveau Système

✅ **1 seul fichier JSON par langue** - Facile à éditer
✅ **Pas de ScriptableObjects pour l'UI** - Moins de fichiers à gérer
✅ **Édition directe** - N'importe quel éditeur de texte
✅ **Versioning facile** - Diff clair dans Git
✅ **Traduction simple** - Envoyer JSON aux traducteurs
✅ **77 clés déjà remplies** - Anglais et Français prêts !

## Fichiers à Utiliser

### Pour l'UI (textes dynamiques)
- `en.json` / `fr.json` → Éditer directement
- `SimpleLocalizationHelper` → Utiliser dans le code

### Pour les ScriptableObjects (noms/descriptions)
- `LocalizedString` SO → Créer assets individuels
- `runeName`, `Description`, `enemyName` → Restent LocalizedString

## Prochain Step

1. ✅ Lis [SIMPLE_SETUP.md](SIMPLE_SETUP.md)
2. ✅ Crée le GameObject `LocalizationManager`
3. ✅ Teste avec un script UI simple
4. ✅ Remplace progressivement les hardcoded strings

---

**C'est BEAUCOUP plus simple maintenant !** 😊

Un seul fichier JSON à éditer, pas de dizaines de ScriptableObjects à créer manuellement !
