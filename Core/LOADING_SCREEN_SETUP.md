# Loading Screen - Unity Setup Instructions

## Step-by-Step Prefab Creation

### 1. Create Root GameObject
1. In your **MainMenu** scene, create an empty GameObject
2. Name it: `LoadingScreen`
3. Add Component: `SceneLoader` (the script we just created)
4. **IMPORTANT**: Keep this object at the **root level** (no parent)

### 2. Create Canvas
1. Right-click `LoadingScreen` → UI → Canvas
2. Rename to: `LoadingCanvas`
3. Configure Canvas component:
   - **Render Mode**: Screen Space - Overlay
   - **Pixel Perfect**: Checked (optional)
   - **Sort Order**: 999 (must be on top of everything)
4. Add Component: `Canvas Scaler`
   - **UI Scale Mode**: Scale With Screen Size
   - **Reference Resolution**: 1920 x 1080
   - **Match**: 0.5 (Width/Height)

### 3. Create Loading Panel
1. Right-click `LoadingCanvas` → UI → Panel
2. Rename to: `LoadingPanel`
3. Configure RectTransform:
   - **Anchors**: Stretch-Stretch (covers entire screen)
   - **Left/Right/Top/Bottom**: 0
4. Add Component: `Canvas Group`
   - **Alpha**: 0 (starts hidden)
   - **Interactable**: False
   - **Blocks Raycasts**: False
5. Configure Image (Background):
   - **Color**: Black (R:0, G:0, B:0, A:230)
   - **Raycast Target**: True

### 4. Create Progress Bar Container
1. Right-click `LoadingPanel` → Create Empty
2. Rename to: `ProgressBarContainer`
3. Configure RectTransform:
   - **Anchors**: Bottom-Center
   - **Pos X**: 0, **Pos Y**: 150
   - **Width**: 800, **Height**: 40

### 5. Create Slider (Progress Bar)
1. Right-click `ProgressBarContainer` → UI → Slider
2. Rename to: `ProgressBar`
3. Configure RectTransform:
   - **Anchors**: Stretch-Stretch
   - **Left/Right/Top/Bottom**: 0
4. Configure Slider component:
   - **Direction**: Left to Right
   - **Min Value**: 0
   - **Max Value**: 1
   - **Whole Numbers**: Unchecked
   - **Value**: 0
   - **Interactable**: False
5. Delete child: `Handle Slide Area` (not needed)

### 6. Style Progress Bar Background
1. Select: `ProgressBar/Background`
2. Configure Image:
   - **Color**: Dark Gray (R:50, G:50, B:50, A:255)
   - **Sprite**: UI-Sprite (default Unity sprite)
   - **Image Type**: Sliced
3. Configure RectTransform:
   - **Left/Right/Top/Bottom**: 0

### 7. Style Progress Bar Fill
1. Select: `ProgressBar/Fill Area/Fill`
2. Configure Image:
   - **Color**: Bright Green (R:100, G:255, B:100, A:255)
   - **Sprite**: UI-Sprite (default Unity sprite)
   - **Image Type**: Sliced
3. Configure RectTransform:
   - **Right**: 10 (padding)

### 8. Create Tip Text
1. Right-click `LoadingPanel` → UI → Text - TextMeshPro
2. Rename to: `TipText`
3. Configure RectTransform:
   - **Anchors**: Bottom-Center
   - **Pos X**: 0, **Pos Y**: 80
   - **Width**: 900, **Height**: 60
4. Configure TextMeshProUGUI:
   - **Text**: "Tip: Loading..."
   - **Font Size**: 24
   - **Alignment**: Center-Middle
   - **Color**: White
   - **Word Wrapping**: Enabled
   - **Overflow**: Truncate

### 9. Optional: Add Loading Icon/Spinner
1. Right-click `LoadingPanel` → UI → Image
2. Rename to: `LoadingSpinner`
3. Configure RectTransform:
   - **Anchors**: Center-Center
   - **Pos X**: 0, **Pos Y**: 0
   - **Width**: 128, **Height**: 128
4. Add Component: `Rotate` (create a simple rotation script)
```csharp
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] private float speed = 180f;

    void Update()
    {
        transform.Rotate(0, 0, -speed * Time.unscaledDeltaTime);
    }
}
```

### 10. Configure SceneLoader Component
Select `LoadingScreen` root object and assign:

**UI References:**
- **Loading Canvas Group**: Drag `LoadingPanel` here
- **Progress Bar**: Drag `ProgressBar` slider here
- **Tip Text**: Drag `TipText` here

**Loading Settings:**
- **Minimum Loading Time**: 1.5
- **Fade Duration**: 0.5

**Game Tips:** (Array Size: 15+)
- Element 0: "Tip: Combine Form and Effect runes to create powerful spells!"
- Element 1: "Tip: Different elements have different effects on enemies."
- Element 2: "Tip: Multicast repeats your entire spell cast multiple times."
- Element 3: "Tip: Multishot adds extra projectiles to each cast."
- Element 4: "Tip: Level up your runes to unlock powerful upgrades!"
- Element 5: "Tip: Collect gems to gain experience and level up."
- Element 6: "Tip: Some enemies are resistant to certain elements."
- Element 7: "Tip: Use the ban feature to avoid unwanted runes."
- Element 8: "Tip: Area spells always spread in a full circle."
- Element 9: "Tip: Orbit spells never despawn and can hit multiple enemies."
- Element 10: "Tip: Critical hits can stack - over 100% crit chance means multiple crits!"
- Element 11: "Tip: Necrotic damage can spawn ghost minions when enemies die."
- Element 12: "Tip: Ghost minions explode on impact, dealing area damage."
- Element 13: "Tip: Move to avoid enemy attacks - mobility is key!"
- Element 14: "Tip: Try different spell combinations to find your playstyle."

**Settings:**
- **Verbose Logging**: True (for testing, disable in production)

### 11. Final Hierarchy Check
Your hierarchy should look like this:
```
LoadingScreen (SceneLoader component)
└── LoadingCanvas (Canvas, Canvas Scaler)
    └── LoadingPanel (CanvasGroup, Image)
        ├── ProgressBarContainer
        │   └── ProgressBar (Slider)
        │       ├── Background (Image)
        │       └── Fill Area
        │           └── Fill (Image)
        ├── TipText (TextMeshProUGUI)
        └── LoadingSpinner (Image, Rotate) [Optional]
```

### 12. Test in Play Mode
1. Enter Play Mode in MainMenu scene
2. LoadingScreen should be **invisible** initially
3. Try loading a scene (click Play button)
4. Loading screen should:
   - ✅ Fade in smoothly
   - ✅ Show a random tip
   - ✅ Progress bar fills from 0% to 100%
   - ✅ Fade out smoothly
   - ✅ New scene loads

### 13. Troubleshooting

**Loading screen appears on startup:**
- Check that `LoadingPanel` CanvasGroup has Alpha = 0
- Verify SceneLoader.Awake() calls HideLoadingScreen(instant: true)

**Progress bar doesn't move:**
- Ensure Slider min=0, max=1
- Check that progressBar reference is assigned in SceneLoader

**Tips don't show:**
- Verify tipText reference is assigned
- Check that gameTips array has elements

**Loading screen stays visible:**
- Check fadeDuration isn't too long
- Verify coroutine completes (check logs)

**Canvas not visible:**
- Check Sort Order is high (999)
- Ensure Render Mode is Screen Space - Overlay

## Color Palette Suggestions

### Professional Dark Theme
- Background: RGB(20, 20, 25) Alpha: 240
- Progress BG: RGB(40, 40, 45)
- Progress Fill: RGB(100, 200, 255) (Blue gradient)
- Text: White

### Vibrant Game Theme
- Background: RGB(10, 5, 30) Alpha: 250 (Dark purple)
- Progress BG: RGB(50, 30, 60)
- Progress Fill: RGB(150, 255, 150) (Bright green)
- Text: RGB(255, 255, 200) (Light yellow)

### Minimal Theme
- Background: RGB(0, 0, 0) Alpha: 200
- Progress BG: RGB(80, 80, 80)
- Progress Fill: White
- Text: White

## Next Steps

After setup:
1. ✅ Save the scene
2. ✅ Test all scene transitions (MainMenu → Game, Game → MainMenu, Restart)
3. ✅ Adjust colors/fonts to match your game's art style
4. ⭐ Consider localizing tips (use SimpleLocalizationHelper)
5. ⭐ Add loading animations (spinner, pulsing effects)
6. ⭐ Track analytics (loading time, player feedback)
