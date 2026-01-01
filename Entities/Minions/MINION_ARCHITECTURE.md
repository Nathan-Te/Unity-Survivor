# Architecture du Système de Minions

## Vue d'ensemble
Système d'invocation de créatures alliées (minions) pour l'effet Necrotic. Les minions utilisent le Job System pour le mouvement et l'object pooling pour la performance.

---

## Flow du Système

```
Enemy Death (with Necrotic)
    ↓
ProjectileDamageHandler.ApplyDamage()
    ↓ (MinionChance roll)
MinionManager.SpawnMinion()
    ↓
MinionPool.GetMinion() ← Object Pooling
    ↓
MinionController.Initialize()
    ↓
MinionMovementSystem ← Job System (Burst)
    ↓
MinionController.UpdateCombat()
    ↓ (Attack enemies)
EnemyController.TakeDamage()
```

---

## Composants Principaux

### 1. MinionData (ScriptableObject)
**Fichier** : `MinionData.cs`
**Rôle** : Définition des stats et comportement d'un type de minion

**Champs Clés** :
- `MinionType type` : Melee ou Ranged
- `float moveSpeed` : Vitesse de déplacement
- `float followDistance` : Distance de suivi autour du joueur
- `float maxFollowRange` : Distance max avant téléportation
- `float baseDamage` : Dégâts de base
- `float attackRange` : Portée d'attaque
- `float attackCooldown` : Cooldown entre attaques
- `float duration` : Durée de vie (0 = infini)
- `GameObject prefab` : Prefab du minion

### 2. MinionController
**Fichier** : `MinionController.cs`
**Rôle** : Contrôleur principal d'un minion individuel

**Responsabilités** :
- Gestion de la santé et des stats
- Détection et attaque des ennemis
- Gestion de la durée de vie (lifetime)
- Téléportation si trop loin du joueur
- Attaques mélée et ranged
- Intégration avec le système d'animation (optionnel)

**Méthodes Clés** :
- `UpdateCombat()` : Trouve et attaque l'ennemi le plus proche
- `FindNearestEnemy()` : Utilise `EnemyManager.GetTarget()`
- `PerformAttack()` : Déclenche l'animation d'attaque
- `OnAnimationAttackFrame()` : Callback depuis l'Animation Event
- `ExecuteAttack()` : Mélée (direct damage) ou Ranged (spawn projectile)
- `TakeDamage(float damage)` : Gestion des dégâts reçus
- `Die()` : Retourne au pool via MinionManager

**Animation Flow** :
1. `PerformAttack()` → Trigger attack animation (si animateur présent)
2. Animation Event → `MinionAnimator.OnAttackFrame()`
3. `MinionAnimator.OnAttackFrame()` → `MinionController.OnAnimationAttackFrame()`
4. `OnAnimationAttackFrame()` → `ExecuteAttack()` (damage/projectile)

**Fallback** : Si pas d'animateur, `ExecuteAttack()` est appelé immédiatement

### 2b. MinionAnimator (Optionnel)
**Fichier** : `MinionAnimator.cs`
**Rôle** : Gestion des animations avec optimisation LOD

**Similaire à** : `EnemyAnimator.cs`

**Fonctionnalités** :
- **Paramètres Animator** :
  - `float Speed` : Vitesse de déplacement (calculée depuis mouvement)
  - `trigger Attack` : Déclenche l'animation d'attaque
- **LOD System** :
  - Distance < 4m : Update chaque frame (HIGH quality)
  - Distance 4-10m : Update toutes les 3 frames (MED quality)
  - Distance > 10m : Update toutes les 6 frames (LOW quality)
- **Frame Offset** : Désynchronisation aléatoire pour éviter les pics CPU
- **Culling** : `AnimatorCullingMode.CullUpdateTransforms` (stop si hors caméra)
- **Renderer Check** : Seulement update si visible ou très proche

**Méthodes** :
- `TriggerAttackAnimation()` : Appelé par MinionController pour déclencher attaque
- `OnAttackFrame()` : Animation Event callback → appelle MinionController

### 3. MinionMovementSystem
**Fichier** : `MinionMovementSystem.cs`
**Rôle** : Gestion du mouvement via Job System (Burst)

**Similaire à** : `EnemyMovementSystem.cs`

**Architecture** :
- Utilise **TransformAccessArray** pour accès parallèle
- **NativeArrays** pour les données (speeds, followDistances)
- **RaycastCommands** pour détection d'obstacles
- **Burst compilation** pour performance maximale

**Job : MoveMinionsJob**
- Orbit autour du joueur à `followDistance`
- Obstacle avoidance (raycasts 3 directions)
- Smooth blending des directions
- Rotation face à la direction de mouvement

### 4. MinionPool
**Fichier** : `MinionPool.cs`
**Rôle** : Object pooling pour éviter Instantiate/Destroy

**Similaire à** : `EnemyPool.cs`

**Fonctionnalités** :
- Dictionnaire de pools par prefab
- `GetMinion()` : Récupère ou instancie
- `ReturnToPool()` : Désactive et met en pool
- `SpawnPopAnimation()` : Animation de spawn (scale 0 → 1)
- `ClearAll()` / `DestroyAll()` : Nettoyage

### 5. MinionManager (Singleton)
**Fichier** : `MinionManager.cs`
**Rôle** : Coordinateur principal du système de minions

**Similaire à** : `EnemyManager.cs`

**Responsabilités** :
- Gestion de la liste des minions actifs
- Coordination avec `MinionMovementSystem`
- Respect de la limite `MaxMinions`
- Spawn via `MinionPool`
- Registration/Unregistration (swap-back removal)

**Méthodes Clés** :
- `SpawnMinion(MinionData, Vector3, int maxMinions)` : Spawn avec limite
- `RegisterMinion(MinionController)` : Ajoute au tracking + movement system
- `UnregisterMinion(MinionController)` : Retire (swap-back)
- `GetActiveMinionCount()` : Nombre actuel de minions

---

## Intégration avec le Spell System

### RuneStats (Additions)
**Fichier** : `RuneStats.cs`

**Nouveaux Champs** :
```csharp
[Header("Minions (Necrotic)")]
public float FlatMinionChance; // +10% chance de spawn
public int FlatMaxMinions;     // +1 minion max
```

**Accumulation** :
- Additionnés dans l'opérateur `+` (comme les autres stats)
- Accumulés depuis Effect rune + Modifier runes

### SpellDefinition
**Fichier** : `SpellDefinition.cs`

**Nouveaux Champs** :
```csharp
public float MinionChance;   // Chance finale (0-1)
public int MaxMinions;       // Limite de minions actifs
public GameObject MinionPrefab; // Prefab à spawn
```

### SpellBuilder
**Fichier** : `SpellBuilder.cs`

**Accumulation** :
```csharp
// Base effect
def.MinionChance = effect.minionSpawnChance + effectStats.FlatMinionChance;
def.MaxMinions = effectStats.FlatMaxMinions;
def.MinionPrefab = effect.minionPrefab;

// Modifiers
foreach (var modRune in slot.modifierRunes)
{
    def.MinionChance += modStats.FlatMinionChance;
    def.MaxMinions += modStats.FlatMaxMinions;
}
```

### ProjectileDamageHandler
**Fichier** : `ProjectileDamageHandler.cs` (ligne 142-157)

**Logique de Spawn** :
```csharp
// Sur kill enemy
bool isFatal = (enemy.currentHp - finalDamage) <= 0;
if (isFatal && def.MinionChance > 0 && def.MinionPrefab != null)
{
    if (Random.value <= def.MinionChance)
    {
        var minionData = def.MinionPrefab.GetComponent<MinionController>()?.Data;
        MinionManager.Instance.SpawnMinion(minionData, spawnPos, def.MaxMinions);
    }
}
```

---

## Collision Layers

### Layer "Minion"
**CRITICAL** : Doit être créé dans Project Settings

### Collision Matrix
| Layer | Minion |
|-------|--------|
| **Player** | ✅ Enabled (suivi) |
| **Enemy** | ✅ Enabled (combat) |
| **Projectile** | ❌ **DISABLED** (pass-through) |
| **Minion** | ❌ Disabled (pas de collision entre minions) |
| **Obstacle** | ✅ Enabled (pathfinding) |

**Raison Projectile Disabled** :
Les projectiles du joueur doivent passer à travers les minions pour ne pas bloquer le gameplay.

---

## Comportements par Type

### Minion Melee
- **Mouvement** : Orbite autour du joueur
- **Combat** : Charge l'ennemi le plus proche
- **Attaque** : Damage direct au contact (via `EnemyController.TakeDamage`)
- **Range** : `attackRange = 1.5f` (melee)
- **Stop Distance** : `stopDistance = 1.5f` (s'arrête au contact)

### Minion Ranged
- **Mouvement** : Orbite autour du joueur
- **Combat** : Tire sur l'ennemi le plus proche
- **Attaque** : Spawn projectile (via `ProjectilePool`)
- **Range** : `attackRange = 8.0f` (distance)
- **Projectile** : Défini dans `MinionData.projectilePrefab`

**Note** : Le projectile du minion utilise `ProjectileController` avec un `SpellDefinition` simplifié.

---

## Performance

### Optimisations Implémentées
1. **Job System** : Mouvement parallélisé (Burst compiled)
2. **Object Pooling** : Pas de `Instantiate`/`Destroy` en runtime
3. **Swap-back Removal** : O(1) pour unregister minion
4. **Spatial Queries** : Utilise `EnemyManager.GetTarget()` (déjà optimisé)
5. **Native Collections** : `NativeArray`, `NativeList`, `TransformAccessArray`

### Capacité Maximale
- **Recommandé** : 20-30 minions actifs
- **Maximum** : 50 minions (configuré dans `MinionManager.maxMinionsCapacity`)

---

## Lifecycle d'un Minion

```
1. Spawn
   - Enemy killed with Necrotic effect
   - Roll MinionChance
   - MinionManager.SpawnMinion() checks MaxMinions limit
   - MinionPool.GetMinion() (from pool or new)
   - MinionController.OnEnable() → InitializeStats()

2. Active
   - MinionMovementSystem updates position (Job System)
   - MinionController.UpdateCombat() attacks enemies
   - Check lifetime timer (if duration > 0)
   - Check distance to player (teleport if > maxFollowRange)

3. Death
   - HP <= 0 OR lifetime expired
   - MinionController.Die()
   - MinionManager.UnregisterMinion() (swap-back removal)
   - MinionPool.ReturnToPool() (disable + pool)
```

---

## Upgrade Progression Example

### Level 1 (Common)
- `MinionChance = 10%`
- `MaxMinions = 1`
- **Result** : 10% chance to spawn 1 minion on kill

### Level 2 (Rare)
- `MinionChance = 25%` (+15%)
- `MaxMinions = 2` (+1)
- **Result** : 25% chance, max 2 minions

### Level 3 (Epic)
- `MinionChance = 45%` (+20%)
- `MaxMinions = 4` (+2)
- **Result** : 45% chance, max 4 minions

### Level 4 (Legendary)
- `MinionChance = 70%` (+25%)
- `MaxMinions = 7` (+3)
- **Result** : 70% chance, max 7 minions

**Max Potential** : 100% chance, 10+ minions actifs

---

## Comparison avec Enemy System

| Feature | EnemyMovementSystem | MinionMovementSystem |
|---------|---------------------|----------------------|
| **Target** | Player (chase) | Player (orbit) |
| **Behavior** | Flee/Stop/Chase | Orbit at followDistance |
| **Raycasts** | 3 directions | 3 directions |
| **Job** | `MoveEnemiesJob` | `MoveMinionsJob` |
| **Stop** | `stopDistance` from player | `followDistance` from player |
| **Teleport** | No | Yes (if > maxFollowRange) |

**Shared Architecture** :
- Both use `IJobParallelForTransform`
- Both use Burst compilation
- Both use RaycastCommands for obstacles
- Both use swap-back removal

---

## Files Created

```
Assets/Scripts/Entities/Minions/
├── MinionData.cs (ScriptableObject)
├── MinionController.cs (Main controller)
├── MinionAnimator.cs (Animation system with LOD - optional)
├── MinionMovementSystem.cs (Job System movement)
├── MinionPool.cs (Object pooling)
├── MinionManager.cs (Coordinator singleton)
├── MINION_SETUP.md (Configuration guide)
├── MINION_ANIMATION_GUIDE.md (Animation setup guide)
├── MINION_ARCHITECTURE.md (This file)
└── README.md (Overview)
```

---

## Next Steps (Extensions Possibles)

### Variantes de Minions
- **Tank** : High HP, low damage, taunt enemies
- **Mage** : AOE spells, low HP
- **Healer** : Heal player over time

### Upgrades Avancées
- **Minion Damage Multiplier** : +50% damage for all minions
- **Minion Duration** : +10s lifetime
- **Minion Speed** : +20% movement speed
- **Minion AOE** : Minions explode on death

### VFX/Audio
- Spawn VFX (particules)
- Death VFX (dissolution)
- Attack sounds (melee/ranged)
- Ambient sounds (breathing, footsteps)

---

## Critical Rules (pour CLAUDE.md)

1. **NEVER `Instantiate`/`Destroy` minions** - Use `MinionPool.GetMinion/ReturnToPool`
2. **ALWAYS check `MaxMinions` limit** - Enforced in `MinionManager.SpawnMinion()`
3. **ALWAYS disable Minion ↔ Projectile collision** - Layer Collision Matrix
4. **ALWAYS use Job System for movement** - Performance critical
5. **NEVER modify MinionData directly** - It's a ScriptableObject (shared asset)
