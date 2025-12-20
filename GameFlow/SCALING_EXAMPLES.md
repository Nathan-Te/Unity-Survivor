# Enemy Scaling - Examples & Scenarios

## Example 1: Standard 10-Minute Game (No Infinite Mode)

### Configuration
```
HP Curve: (0s, 1.0) → (600s, 5.0)
Damage Curve: (0s, 1.0) → (600s, 3.0)
Enable Infinite Scaling: FALSE
```

### Timeline
| Time | HP Mult | Damage Mult | Example Enemy (50 base HP, 10 base DMG) |
|------|---------|-------------|------------------------------------------|
| 0:00 | 1.0x    | 1.0x        | 50 HP, 10 DMG                            |
| 2:30 | 2.0x    | 1.5x        | 100 HP, 15 DMG                           |
| 5:00 | 3.0x    | 2.0x        | 150 HP, 20 DMG                           |
| 7:30 | 4.0x    | 2.5x        | 200 HP, 25 DMG                           |
| 10:00| 5.0x    | 3.0x        | 250 HP, 30 DMG                           |
| 15:00| 5.0x    | 3.0x        | 250 HP, 30 DMG *(capped)*                |

**Result:** After 10 minutes, difficulty plateaus. Good for timed runs.

---

## Example 2: Infinite Survival Mode

### Configuration
```
HP Curve: (0s, 1.0) → (300s, 3.0)
Damage Curve: (0s, 1.0) → (300s, 2.0)
Enable Infinite Scaling: TRUE
Infinite HP Growth Per Minute: 0.5
Infinite Damage Growth Per Minute: 0.3
```

### Timeline
| Time | HP Mult | Damage Mult | Example Enemy (50 base HP, 10 base DMG) |
|------|---------|-------------|------------------------------------------|
| 0:00 | 1.0x    | 1.0x        | 50 HP, 10 DMG                            |
| 2:30 | 2.0x    | 1.5x        | 100 HP, 15 DMG                           |
| 5:00 | 3.0x    | 2.0x        | 150 HP, 20 DMG *(curve ends)*            |
| 6:00 | 3.5x    | 2.3x        | 175 HP, 23 DMG *(+1 min past curve)*     |
| 7:00 | 4.0x    | 2.6x        | 200 HP, 26 DMG *(+2 min past curve)*     |
| 10:00| 5.5x    | 3.5x        | 275 HP, 35 DMG *(+5 min past curve)*     |
| 20:00| 10.5x   | 6.5x        | 525 HP, 65 DMG *(+15 min past curve)*    |
| 30:00| 15.5x   | 9.5x        | 775 HP, 95 DMG *(+25 min past curve)*    |

**Calculation (after curve ends):**
- At 10 minutes (5 min past 300s curve end):
  - HP: `3.0 + (0.5 × 5) = 5.5x`
  - Damage: `2.0 + (0.3 × 5) = 3.5x`

**Result:** Difficulty increases forever. Perfect for "how long can you survive" gameplay.

---

## Example 3: Exponential Early Game, Linear Late Game

### Configuration
```
HP Curve:
  - (0s, 1.0)
  - (60s, 2.0)   - Double HP in 1 minute
  - (120s, 3.5)  - Fast ramp
  - (300s, 5.0)  - Plateau slope
Enable Infinite Scaling: TRUE
Infinite HP Growth Per Minute: 0.2  (slow)
```

### Visual Curve Shape
```
5.0 |                    ___________  (slow infinite growth)
    |               ___--
3.5 |         ___---
    |    __---
2.0 | __-
1.0 |_
    0   60   120       300         600 (seconds)
```

**Result:** Punishing early game, manageable late game. Forces player to get strong fast.

---

## Example 4: Gentle Ramp for New Players

### Configuration
```
HP Curve: (0s, 1.0) → (1200s, 2.5)  - 20 minutes to reach 2.5x
Damage Curve: (0s, 1.0) → (1200s, 1.8)
Enable Infinite Scaling: FALSE
```

### Timeline
| Time | HP Mult | Damage Mult | Notes                    |
|------|---------|-------------|--------------------------|
| 0:00 | 1.0x    | 1.0x        | Easy start               |
| 10:00| 1.75x   | 1.4x        | Still manageable         |
| 20:00| 2.5x    | 1.8x        | Peak difficulty (capped) |

**Result:** Long, forgiving progression. Good for tutorials or casual mode.

---

## Recommended Settings by Mode

### Story/Campaign Mode
- Curve: 0-600s (10 min)
- HP: 1.0x → 3.0x
- Damage: 1.0x → 2.0x
- Infinite: **Disabled**

### Arcade/Survival Mode
- Curve: 0-300s (5 min)
- HP: 1.0x → 4.0x
- Damage: 1.0x → 2.5x
- Infinite: **Enabled** (HP +0.5/min, DMG +0.3/min)

### Hard/Nightmare Mode
- Curve: 0-180s (3 min)
- HP: 1.0x → 5.0x
- Damage: 1.0x → 3.0x
- Infinite: **Enabled** (HP +1.0/min, DMG +0.5/min)

### Tutorial/Easy Mode
- Curve: 0-1200s (20 min)
- HP: 1.0x → 2.0x
- Damage: 1.0x → 1.5x
- Infinite: **Disabled**
