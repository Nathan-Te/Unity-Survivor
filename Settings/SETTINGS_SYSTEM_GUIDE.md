# Settings System - Complete Guide

## Vue d'Ensemble

Le système de settings sauvegarde les préférences du joueur dans un fichier **`settings.json`** séparé du fichier de progression (`progression.json`).

### Pourquoi Deux Fichiers?

| Fichier | Contenu | Fréquence de Modification |
|---------|---------|---------------------------|
| **`progression.json`** | Or, déblocages, statistiques | À chaque fin de partie |
| **`settings.json`** | Audio, langue, graphiques, gameplay | Rarement (dans le menu settings) |

**Avantages**:
- Settings peuvent être modifiés sans risquer de corrompre la progression
- Partage de settings entre profils possible
- Reset settings sans perdre la progression

## Architecture

```
GameSettingsManager (DontDestroyOnLoad Singleton)
    ↓ loads on startup
SettingsSaveSystem (Static Class)
    ↓ reads/writes
settings.json (Application.persistentDataPath)
    ↓ contains
GameSettings (Serializable Data)
```

## Fichiers du Système

### 1. GameSettings.cs
Classe de données sérialisable contenant tous les settings:

```csharp
public class GameSettings
{
    // Audio
    public float masterVolume = 1.0f;
    public float musicVolume = 0.7f;
    public float sfxVolume = 0.8f;
    public bool muteAll = false;

    // Localization
    public string languageCode = "en";

    // Graphics
    public int qualityLevel = 2;
    public bool fullscreen = true;
    public int targetFrameRate = 60;
    public bool vsync = true;

    // Gameplay
    public bool showDamageNumbers = true;
    public bool screenShake = true;
    public float screenShakeIntensity = 1.0f;

    // Accessibility
    public bool colorBlindMode = false;
    public float uiScale = 1.0f;
}
```

### 2. SettingsSaveSystem.cs
Système statique pour sauvegarder/charger les settings:

```csharp
// Charger settings
GameSettings settings = SettingsSaveSystem.LoadSettings();

// Sauvegarder settings
SettingsSaveSystem.SaveSettings(settings);

// Vérifier existence
bool exists = SettingsSaveSystem.SettingsExists();

// Obtenir le chemin
string path = SettingsSaveSystem.GetSavePath();
```

### 3. GameSettingsManager.cs
Singleton DontDestroyOnLoad qui gère les settings:

```csharp
// Accéder au singleton
GameSettingsManager.Instance

// Obtenir settings actuels
GameSettings current = GameSettingsManager.Instance.CurrentSettings;

// Modifier un setting
GameSettingsManager.Instance.SetMasterVolume(0.5f);
GameSettingsManager.Instance.SetLanguage(Language.French);

// Reset aux valeurs par défaut
GameSettingsManager.Instance.ResetToDefault();
```

## Setup dans Unity

### Étape 1: Créer le GameObject

1. Créez un **GameObject vide** dans la scène de démarrage (ex: MainMenu)
2. Nommez-le `GameSettingsManager`
3. Attachez le script `GameSettingsManager.cs`
4. Assurez-vous qu'il est au **niveau racine** de la hiérarchie (pas enfant)

### Étape 2: Configurer l'Inspector

```
GameSettingsManager
├─ Auto Save On Change: ✓ (recommandé)
└─ Verbose Logging: ✓ (pour debug)
```

### Étape 3: Créer SimpleLocalizationManager

Si pas déjà fait, créez un GameObject avec `SimpleLocalizationManager` pour que la langue puisse être appliquée.

## Utilisation

### Charger les Settings au Démarrage

Le `GameSettingsManager` charge automatiquement les settings dans `Awake()`:

```csharp
protected override void Awake()
{
    base.Awake();
    DontDestroyOnLoad(gameObject);
    LoadSettings(); // ← Charge settings.json
}
```

**Flow**:
1. Charge `settings.json` (ou crée settings par défaut si inexistant)
2. Applique la langue au `SimpleLocalizationManager`
3. Applique les volumes à `AudioManager`
4. Applique les paramètres graphiques (qualité, fullscreen, etc.)

### Modifier un Setting

Dans votre UI de settings (ex: `SettingsMenuUI`):

```csharp
using SurvivorGame.Settings;

// Changer le volume master
public void OnMasterVolumeChanged(float value)
{
    GameSettingsManager.Instance.SetMasterVolume(value);
    // ← Auto-sauvegardé si autoSaveOnChange = true
}

// Changer la langue
public void OnLanguageChanged(Language language)
{
    GameSettingsManager.Instance.SetLanguage(language);
    // ← Auto-sauvegardé + appliqué à SimpleLocalizationManager
}

// Changer la qualité graphique
public void OnQualityChanged(int level)
{
    GameSettingsManager.Instance.SetQualityLevel(level);
    // ← Auto-sauvegardé + appliqué à QualitySettings
}
```

### Écouter les Changements de Settings

Si vous voulez réagir aux changements de settings:

```csharp
private void Start()
{
    GameSettingsManager.Instance.OnSettingsChanged += OnSettingsChanged;
}

private void OnDestroy()
{
    var manager = FindFirstObjectByType<GameSettingsManager>();
    if (manager != null)
    {
        manager.OnSettingsChanged -= OnSettingsChanged;
    }
}

private void OnSettingsChanged(GameSettings settings)
{
    // Réagir aux changements
    Debug.Log($"Settings changed! Volume: {settings.masterVolume}");
}
```

## Localisation

### Integration avec SimpleLocalizationManager

Le système détecte automatiquement la langue système et l'applique:

```csharp
// Dans GameSettings.CreateDefault()
languageCode = GetSystemLanguageCode(); // "en" ou "fr" selon système

// Lors du chargement
SimpleLocalizationManager.Instance.SetLanguage(settings.GetLanguageEnum());
```

### Changer la Langue

```csharp
// Option 1: Depuis GameSettingsManager
GameSettingsManager.Instance.SetLanguage(Language.French);
// ← Sauvegarde dans settings.json ET applique à SimpleLocalizationManager

// Option 2: Directement (déconseillé, pas sauvegardé)
SimpleLocalizationManager.Instance.SetLanguage(Language.French);
// ← N'est PAS sauvegardé!
```

**IMPORTANT**: Utilisez toujours `GameSettingsManager.SetLanguage()` pour que la langue soit sauvegardée.

## Localisation du Fichier

### Windows
```
C:\Users\[Username]\AppData\LocalLow\[CompanyName]\[GameName]\settings.json
```

### Menu Unity (Pratique)

Utilisez le menu `Tools > Settings System`:
- **Show Settings File Location** - Affiche le chemin
- **Open Settings Folder** - Ouvre le dossier dans l'explorateur
- **View Settings File Content** - Affiche le contenu JSON

## Format du Fichier

Exemple de `settings.json`:

```json
{
    "masterVolume": 1.0,
    "musicVolume": 0.7,
    "sfxVolume": 0.8,
    "muteAll": false,
    "languageCode": "fr",
    "qualityLevel": 2,
    "fullscreen": true,
    "targetFrameRate": 60,
    "vsync": true,
    "showDamageNumbers": true,
    "screenShake": true,
    "screenShakeIntensity": 1.0,
    "colorBlindMode": false,
    "uiScale": 1.0
}
```

## Menu de Debug Unity

Accédez à `Tools > Settings System` pour:

| Commande | Description |
|----------|-------------|
| **Show Settings File Location** | Affiche le chemin du fichier |
| **Open Settings Folder** | Ouvre le dossier dans l'explorateur |
| **Delete Settings File** | Supprime settings.json (avec confirmation) |
| **Create Test Settings (Default)** | Crée settings par défaut |
| **View Settings File Content** | Affiche le JSON dans la console |
| **Force Create Default Settings** | Écrase settings existants avec défaut |
| **Create Custom Test Settings** | Crée settings de test (French, volumes réduits) |

## Workflow Typique

### 1. Premier Lancement du Jeu

```
1. GameSettingsManager.Awake()
2. LoadSettings()
3. settings.json n'existe pas
4. GameSettings.CreateDefault()
   ├─ Langue = langue système (fr si français, sinon en)
   ├─ Volumes = défaut (100%, 70%, 80%)
   └─ Graphiques = défaut (qualité actuelle système)
5. SaveSettings() ← Crée settings.json
6. ApplySettings()
   ├─ SimpleLocalizationManager.SetLanguage(...)
   └─ AudioManager.SetVolumes(...)
```

### 2. Ouverture Menu Settings

```
1. SettingsMenuUI.Start()
2. Lit GameSettingsManager.Instance.CurrentSettings
3. Affiche les valeurs dans les sliders/dropdowns
```

### 3. Modification d'un Setting

```
1. User change slider volume → 0.5
2. OnMasterVolumeChanged(0.5)
3. GameSettingsManager.SetMasterVolume(0.5)
   ├─ _currentSettings.masterVolume = 0.5
   ├─ AudioManager.Instance.SetGlobalVolume(0.5)
   ├─ OnSettingsChanged event fired
   └─ SaveSettings() (si autoSaveOnChange = true)
4. settings.json mis à jour
```

### 4. Redémarrage du Jeu

```
1. GameSettingsManager.Awake()
2. LoadSettings()
3. settings.json existe
4. Charge le JSON → GameSettings
5. ApplySettings()
   ├─ Applique masterVolume = 0.5
   └─ Applique languageCode = "fr"
```

## Intégration avec SettingsMenuUI

Exemple de menu de settings:

```csharp
using SurvivorGame.Settings;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{
    [Header("Audio Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Language Dropdown")]
    [SerializeField] private Dropdown languageDropdown;

    private void Start()
    {
        // Charger les valeurs actuelles
        if (GameSettingsManager.Instance != null)
        {
            var settings = GameSettingsManager.Instance.CurrentSettings;

            masterVolumeSlider.value = settings.masterVolume;
            musicVolumeSlider.value = settings.musicVolume;
            sfxVolumeSlider.value = settings.sfxVolume;

            // Setup language dropdown
            languageDropdown.value = settings.languageCode == "fr" ? 1 : 0;
        }

        // Écouter les changements
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    private void OnMasterVolumeChanged(float value)
    {
        GameSettingsManager.Instance.SetMasterVolume(value);
    }

    private void OnMusicVolumeChanged(float value)
    {
        GameSettingsManager.Instance.SetMusicVolume(value);
    }

    private void OnLanguageChanged(int index)
    {
        Language language = index == 1 ? Language.French : Language.English;
        GameSettingsManager.Instance.SetLanguage(language);
    }
}
```

## Best Practices

### ✅ DO

- Utilisez `GameSettingsManager.SetXXX()` pour modifier les settings
- Activez `autoSaveOnChange` pour sauvegarder automatiquement
- Utilisez le menu Unity `Tools > Settings System` pour debug
- Testez avec différentes langues (créez custom test settings)

### ❌ DON'T

- Ne modifiez PAS `CurrentSettings` directement sans appeler les méthodes `SetXXX()`
- N'appelez PAS `SimpleLocalizationManager.SetLanguage()` directement (utilisez `GameSettingsManager.SetLanguage()`)
- Ne sauvegardez PAS les settings à chaque frame (utilisez `autoSaveOnChange` ou sauvegardez manuellement)

## Troubleshooting

### "Settings are not saved"

1. Vérifiez que `autoSaveOnChange = true` dans l'Inspector
2. Ou appelez manuellement `GameSettingsManager.Instance.SaveSettings()`
3. Vérifiez les permissions du dossier `Application.persistentDataPath`

### "Language doesn't change"

1. Vérifiez que `SimpleLocalizationManager` existe dans la scène
2. Utilisez `GameSettingsManager.SetLanguage()` (pas directement SimpleLocalizationManager)
3. Vérifiez que la clé de langue existe (`en.json`, `fr.json`)

### "Settings reset on restart"

1. Vérifiez que `GameSettingsManager` est DontDestroyOnLoad
2. Vérifiez que settings.json existe (utilisez le menu Unity)
3. Vérifiez les logs pour erreurs de désérialisation JSON

## Résumé

- **2 fichiers**: `progression.json` (jeu) + `settings.json` (préférences)
- **GameSettingsManager**: Singleton DontDestroyOnLoad qui gère tout
- **Auto-save**: Active par défaut, sauvegarde automatique lors des changements
- **Langue**: Détecte langue système, applique à SimpleLocalizationManager
- **Menu Unity**: `Tools > Settings System` pour debug

Le système est prêt à l'emploi! Créez juste le GameObject `GameSettingsManager` et lancez le jeu.
