# Loading Screen Tips - Localization Guide (Optional)

## Overview
This guide shows how to localize loading screen tips using the existing SimpleLocalizationHelper system.

**Note**: This is **OPTIONAL**. The current implementation uses hardcoded English tips, which works perfectly fine if you don't need multiple languages.

## Current Implementation (Hardcoded)

SceneLoader currently has hardcoded English tips:
```csharp
[SerializeField] private string[] gameTips = new string[]
{
    "Tip: Combine Form and Effect runes to create powerful spells!",
    "Tip: Different elements have different effects on enemies.",
    // ... more tips
};
```

## Option 1: Keep It Simple (Recommended for Single Language)

If your game is English-only, **don't change anything**. The current implementation is:
- ✅ Simple
- ✅ Fast
- ✅ Easy to edit in Inspector
- ✅ No dependencies on localization system

## Option 2: Full Localization (Multi-Language Support)

If you need multiple languages, follow these steps:

### Step 1: Add Tips to Localization JSON

Edit `Assets/Resources/Localization/en.json`:
```json
{
  "entries": [
    { "key": "LOADING_TIP_1", "value": "Combine Form and Effect runes to create powerful spells!" },
    { "key": "LOADING_TIP_2", "value": "Different elements have different effects on enemies." },
    { "key": "LOADING_TIP_3", "value": "Multicast repeats your entire spell cast multiple times." },
    { "key": "LOADING_TIP_4", "value": "Multishot adds extra projectiles to each cast." },
    { "key": "LOADING_TIP_5", "value": "Level up your runes to unlock powerful upgrades!" },
    { "key": "LOADING_TIP_6", "value": "Collect gems to gain experience and level up." },
    { "key": "LOADING_TIP_7", "value": "Some enemies are resistant to certain elements." },
    { "key": "LOADING_TIP_8", "value": "Use the ban feature to avoid unwanted runes." },
    { "key": "LOADING_TIP_9", "value": "Area spells always spread in a full circle." },
    { "key": "LOADING_TIP_10", "value": "Orbit spells never despawn and can hit multiple enemies." },
    { "key": "LOADING_TIP_11", "value": "Critical hits can stack - over 100% crit chance means multiple crits!" },
    { "key": "LOADING_TIP_12", "value": "Necrotic damage can spawn ghost minions when enemies die." },
    { "key": "LOADING_TIP_13", "value": "Ghost minions explode on impact, dealing area damage." },
    { "key": "LOADING_TIP_14", "value": "Move to avoid enemy attacks - mobility is key!" },
    { "key": "LOADING_TIP_15", "value": "Try different spell combinations to find your playstyle." }
  ]
}
```

Edit `Assets/Resources/Localization/fr.json`:
```json
{
  "entries": [
    { "key": "LOADING_TIP_1", "value": "Combinez les runes Forme et Effet pour créer des sorts puissants !" },
    { "key": "LOADING_TIP_2", "value": "Différents éléments ont des effets différents sur les ennemis." },
    { "key": "LOADING_TIP_3", "value": "Multicast répète votre sort entier plusieurs fois." },
    { "key": "LOADING_TIP_4", "value": "Multishot ajoute des projectiles supplémentaires à chaque lancer." },
    { "key": "LOADING_TIP_5", "value": "Améliorez vos runes pour débloquer des améliorations puissantes !" },
    { "key": "LOADING_TIP_6", "value": "Collectez des gemmes pour gagner de l'expérience et monter de niveau." },
    { "key": "LOADING_TIP_7", "value": "Certains ennemis résistent à certains éléments." },
    { "key": "LOADING_TIP_8", "value": "Utilisez la fonction de bannissement pour éviter les runes indésirables." },
    { "key": "LOADING_TIP_9", "value": "Les sorts de zone se propagent toujours en cercle complet." },
    { "key": "LOADING_TIP_10", "value": "Les sorts en orbite ne disparaissent jamais et peuvent toucher plusieurs ennemis." },
    { "key": "LOADING_TIP_11", "value": "Les coups critiques s'accumulent - plus de 100% de chance critique signifie plusieurs critiques !" },
    { "key": "LOADING_TIP_12", "value": "Les dégâts nécrotiques peuvent invoquer des sbires fantômes à la mort des ennemis." },
    { "key": "LOADING_TIP_13", "value": "Les sbires fantômes explosent à l'impact, infligeant des dégâts de zone." },
    { "key": "LOADING_TIP_14", "value": "Déplacez-vous pour éviter les attaques ennemies - la mobilité est la clé !" },
    { "key": "LOADING_TIP_15", "value": "Essayez différentes combinaisons de sorts pour trouver votre style de jeu." }
  ]
}
```

### Step 2: Modify SceneLoader.cs

Replace the `gameTips` array with localization keys:

```csharp
using SurvivorGame.Localization; // Add this

[Header("Game Tips")]
[SerializeField] private string[] gameTipKeys = new string[]
{
    "LOADING_TIP_1",
    "LOADING_TIP_2",
    "LOADING_TIP_3",
    "LOADING_TIP_4",
    "LOADING_TIP_5",
    "LOADING_TIP_6",
    "LOADING_TIP_7",
    "LOADING_TIP_8",
    "LOADING_TIP_9",
    "LOADING_TIP_10",
    "LOADING_TIP_11",
    "LOADING_TIP_12",
    "LOADING_TIP_13",
    "LOADING_TIP_14",
    "LOADING_TIP_15"
};
```

Then modify the `DisplayRandomTip()` method:

```csharp
/// <summary>
/// Displays a random tip from the gameTipKeys array
/// </summary>
private void DisplayRandomTip()
{
    if (tipText == null || gameTipKeys == null || gameTipKeys.Length == 0)
        return;

    int randomIndex = Random.Range(0, gameTipKeys.Length);
    string tipKey = gameTipKeys[randomIndex];

    // Get localized text
    string localizedTip = SimpleLocalizationHelper.Get(tipKey, "Loading...");
    tipText.text = localizedTip;

    if (verboseLogging)
        Debug.Log($"[SceneLoader] Displaying tip: {localizedTip}");
}
```

### Step 3: Subscribe to Language Changes (Optional)

If you want tips to update when language changes mid-loading (rare):

```csharp
private string _currentTipKey = "";

protected override void Awake()
{
    base.Awake();
    // ... existing code ...

    // Subscribe to language changes
    SimpleLocalizationManager.OnLanguageChanged += RefreshCurrentTip;
}

protected override void OnDestroy()
{
    SimpleLocalizationManager.OnLanguageChanged -= RefreshCurrentTip;
    base.OnDestroy();
}

private void DisplayRandomTip()
{
    if (tipText == null || gameTipKeys == null || gameTipKeys.Length == 0)
        return;

    int randomIndex = Random.Range(0, gameTipKeys.Length);
    _currentTipKey = gameTipKeys[randomIndex];

    RefreshCurrentTip();
}

private void RefreshCurrentTip()
{
    if (string.IsNullOrEmpty(_currentTipKey) || tipText == null)
        return;

    string localizedTip = SimpleLocalizationHelper.Get(_currentTipKey, "Loading...");
    tipText.text = localizedTip;
}
```

## Option 3: Hybrid Approach (Fallback for Missing Localization)

Keep both hardcoded and localized tips:

```csharp
[Header("Game Tips")]
[Tooltip("Localization keys (optional - leave empty to use hardcoded tips)")]
[SerializeField] private string[] gameTipKeys = new string[0];

[Tooltip("Hardcoded tips (used if gameTipKeys is empty)")]
[SerializeField] private string[] gameTipsHardcoded = new string[]
{
    "Tip: Combine Form and Effect runes to create powerful spells!",
    // ... more tips
};

private void DisplayRandomTip()
{
    if (tipText == null) return;

    string tipText = "";

    // Try localized tips first
    if (gameTipKeys != null && gameTipKeys.Length > 0 && SimpleLocalizationManager.Instance != null)
    {
        int randomIndex = Random.Range(0, gameTipKeys.Length);
        string tipKey = gameTipKeys[randomIndex];
        tipText = SimpleLocalizationHelper.Get(tipKey, null);
    }

    // Fallback to hardcoded tips
    if (string.IsNullOrEmpty(tipText) && gameTipsHardcoded != null && gameTipsHardcoded.Length > 0)
    {
        int randomIndex = Random.Range(0, gameTipsHardcoded.Length);
        tipText = gameTipsHardcoded[randomIndex];
    }

    this.tipText.text = tipText;

    if (verboseLogging)
        Debug.Log($"[SceneLoader] Displaying tip: {tipText}");
}
```

## Recommendation

**For most projects**: Use **Option 1** (hardcoded tips)
- ✅ Simpler
- ✅ Fewer dependencies
- ✅ Easier to maintain

**For multi-language games**: Use **Option 2** (full localization)
- ✅ Consistent with rest of game
- ✅ Supports all languages
- ✅ Centralized translation management

**For mixed projects**: Use **Option 3** (hybrid)
- ✅ Works with or without localization
- ✅ Graceful degradation
- ✅ Flexible

## Full Localized Tip List (EN/FR)

### English Tips
1. "Combine Form and Effect runes to create powerful spells!"
2. "Different elements have different effects on enemies."
3. "Multicast repeats your entire spell cast multiple times."
4. "Multishot adds extra projectiles to each cast."
5. "Level up your runes to unlock powerful upgrades!"
6. "Collect gems to gain experience and level up."
7. "Some enemies are resistant to certain elements."
8. "Use the ban feature to avoid unwanted runes."
9. "Area spells always spread in a full circle."
10. "Orbit spells never despawn and can hit multiple enemies."
11. "Critical hits can stack - over 100% crit chance means multiple crits!"
12. "Necrotic damage can spawn ghost minions when enemies die."
13. "Ghost minions explode on impact, dealing area damage."
14. "Move to avoid enemy attacks - mobility is key!"
15. "Try different spell combinations to find your playstyle."

### French Tips (Astuces)
1. "Combinez les runes Forme et Effet pour créer des sorts puissants !"
2. "Différents éléments ont des effets différents sur les ennemis."
3. "Multicast répète votre sort entier plusieurs fois."
4. "Multishot ajoute des projectiles supplémentaires à chaque lancer."
5. "Améliorez vos runes pour débloquer des améliorations puissantes !"
6. "Collectez des gemmes pour gagner de l'expérience et monter de niveau."
7. "Certains ennemis résistent à certains éléments."
8. "Utilisez la fonction de bannissement pour éviter les runes indésirables."
9. "Les sorts de zone se propagent toujours en cercle complet."
10. "Les sorts en orbite ne disparaissent jamais et peuvent toucher plusieurs ennemis."
11. "Les coups critiques s'accumulent - plus de 100% de chance critique signifie plusieurs critiques !"
12. "Les dégâts nécrotiques peuvent invoquer des sbires fantômes à la mort des ennemis."
13. "Les sbires fantômes explosent à l'impact, infligeant des dégâts de zone."
14. "Déplacez-vous pour éviter les attaques ennemies - la mobilité est la clé !"
15. "Essayez différentes combinaisons de sorts pour trouver votre style de jeu."

---

**Bottom Line**: Only localize tips if you actually need multiple languages. The default hardcoded implementation is perfectly fine for most cases!
