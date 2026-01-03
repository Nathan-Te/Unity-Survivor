# Loading Screen System - Integration Guide

## Overview
The `SceneLoader` system provides smooth scene transitions with a loading screen, preventing screen freezing during scene loads.

## Architecture

### SceneLoader.cs
- **Type**: DontDestroyOnLoad Singleton
- **Location**: `Assets/Scripts/Core/SceneLoader.cs`
- **Purpose**: Manages async scene loading with progress feedback

### Key Features
- ✅ Async loading with `LoadSceneAsync` (no freezing)
- ✅ Progress bar visualization
- ✅ Random gameplay tips during loading
- ✅ Smooth fade in/out transitions
- ✅ Memory cleanup before scene load
- ✅ Minimum loading time to prevent flashing
- ✅ Works with paused games (uses `Time.unscaledDeltaTime`)

## Setup Instructions

### 1. Create Loading Screen UI

Create a new GameObject hierarchy in your **first scene** (MainMenu or Splash):

```
LoadingScreen (DontDestroyOnLoad)
└── SceneLoader (Component)
    └── Canvas (Screen Space - Overlay, Sort Order: 999)
        └── LoadingPanel
            ├── Background (Image - black, alpha 0.9)
            ├── ProgressBar (Slider)
            │   ├── Background
            │   ├── Fill Area
            │   │   └── Fill (colored bar)
            │   └── Handle Slide Area (optional)
            └── TipText (TextMeshProUGUI)
```

### 2. Configure SceneLoader Component

Assign the following in the Inspector:

**UI References:**
- `Loading Canvas Group`: Attach to LoadingPanel GameObject
- `Progress Bar`: Assign the Slider component
- `Tip Text`: Assign the TextMeshProUGUI component

**Loading Settings:**
- `Minimum Loading Time`: 1.5s (prevents flashing for fast loads)
- `Fade Duration`: 0.5s (fade in/out speed)

**Game Tips:**
Add helpful tips for players (already pre-filled with examples)

**Settings:**
- `Verbose Logging`: Enable for debugging

### 3. Update Scene Loading Calls

Replace all `SceneManager.LoadScene()` calls with `SceneLoader.Instance.LoadScene()`:

#### Example: LevelSelectionUI.cs
```csharp
// ❌ BEFORE
SceneManager.LoadScene(level.sceneName);

// ✅ AFTER
using SurvivorGame.Core;
SceneLoader.Instance.LoadScene(level.sceneName);
```

#### Example: GameStateManager.cs
```csharp
// ❌ BEFORE
SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

// ✅ AFTER
using SurvivorGame.Core;
SceneLoader.Instance.LoadScene(sceneName);
```

#### Example: GameOverUI.cs
```csharp
// In OnRetryClicked()
// ❌ BEFORE
GameStateManager.RestartGame(); // Uses SceneManager internally

// ✅ AFTER
using SurvivorGame.Core;
SceneLoader.Instance.LoadScene(SceneManager.GetActiveScene().name);

// In OnMainMenuClicked()
// ✅ AFTER
SceneLoader.Instance.LoadScene("MainMenu");
```

## Integration Checklist

- [ ] Create LoadingScreen GameObject with SceneLoader component
- [ ] Assign all UI references in Inspector
- [ ] Configure loading settings and tips
- [ ] Update LevelSelectionUI to use SceneLoader
- [ ] Update GameOverUI to use SceneLoader
- [ ] Update MainMenuUI (if it loads scenes)
- [ ] Update GameStateManager.RestartGame() to use SceneLoader
- [ ] Update GameStateManager.ReturnToMainMenu() to use SceneLoader
- [ ] Test scene transitions (MainMenu -> Game, Game -> MainMenu, Restart)
- [ ] Verify memory cleanup works (check MemoryManager logs)
- [ ] Test minimum loading time (fast loads should still show screen briefly)

## Loading Flow

```
User Clicks "Play" Button
    ↓
SceneLoader.LoadScene("GameScene") called
    ↓
PHASE A: Fade In (0.5s)
    - Canvas activates
    - Alpha 0 → 1
    - Display random tip
    ↓
PHASE B: Cleanup
    - Call MemoryManager.ForceCleanup()
    - GC.Collect()
    - Resources.UnloadUnusedAssets()
    ↓
PHASE C: Loading
    - LoadSceneAsync started
    - Progress bar updates (0% → 90% → 100%)
    - Wait for minimum loading time (1.5s)
    - Allow scene activation when ready
    ↓
PHASE D: Wait (0.2s)
    - Brief pause at 100%
    ↓
PHASE E: Fade Out (0.5s)
    - Alpha 1 → 0
    - Canvas deactivates
    ↓
New Scene is Active!
```

## API Reference

### Public Methods

```csharp
// Load a scene asynchronously with loading screen
SceneLoader.Instance.LoadScene(string sceneName);
```

### Example Usage

```csharp
using SurvivorGame.Core;

// Load game scene
SceneLoader.Instance.LoadScene("GameScene");

// Return to main menu
SceneLoader.Instance.LoadScene("MainMenu");

// Restart current scene
string currentScene = SceneManager.GetActiveScene().name;
SceneLoader.Instance.LoadScene(currentScene);
```

## Compatibility Notes

### Works With
- ✅ GameStateManager (cleanup before load)
- ✅ MemoryManager (automatic cleanup)
- ✅ Paused games (uses unscaled time)
- ✅ DontDestroyOnLoad objects (persists across scenes)

### Important
- SceneLoader must be created in the **first scene** that loads (MainMenu or Splash)
- SceneLoader persists across ALL scenes (DontDestroyOnLoad)
- Only ONE SceneLoader should exist at a time (Singleton pattern)
- If SceneLoader is missing, loading will fail (intentional safety)

## Troubleshooting

### Loading screen doesn't appear
- Check that `loadingCanvasGroup` is assigned in Inspector
- Verify Canvas Sort Order is high (999+)
- Ensure Canvas is set to "Screen Space - Overlay"

### Progress bar doesn't update
- Verify `progressBar` is assigned in Inspector
- Check that Slider value range is 0-1

### Scene loads instantly without loading screen
- Increase `Minimum Loading Time` value
- Check that `LoadSceneAsync` is being used (not regular `LoadScene`)

### Tips don't show
- Assign `tipText` in Inspector
- Add tips to the `gameTips` array

### MemoryManager cleanup doesn't work
- Ensure MemoryManager exists as DontDestroyOnLoad
- Check MemoryManager logs (enable verbose logging)

## Performance Optimization

### Minimum Loading Time
- **Too Short (<0.5s)**: Loading screen flashes briefly, jarring UX
- **Recommended (1.5s)**: Good balance for most games
- **Too Long (>3s)**: Unnecessary wait time on fast hardware

### Fade Duration
- **Too Short (<0.2s)**: Harsh transition
- **Recommended (0.5s)**: Smooth, professional feel
- **Too Long (>1s)**: Sluggish UX

### Tips Array
- Add 10-20 tips for variety
- Keep tips short (1-2 sentences)
- Mix gameplay mechanics, strategy advice, and lore

## Next Steps

After integration, consider:
1. Localizing tips using `SimpleLocalizationHelper`
2. Adding loading animations (spinner, pulsing text)
3. Tracking loading analytics (average load time)
4. Adding level-specific loading tips
