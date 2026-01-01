# Guide de Configuration du Système de Minions (Necrotic)

## Vue d'ensemble

Le système de minions permet d'invoquer des créatures alliées qui combattent à vos côtés. Ils sont invoqués via l'effet Necrotic lorsque vous tuez des ennemis.

## Architecture

```
MinionManager → MinionMovementSystem (Job System) → MinionController
             → MinionPool (Object Pooling)
```

## 1. Configuration des Layers (IMPORTANT!)

### Créer le Layer "Minion"
1. **Edit → Project Settings → Tags and Layers**
2. Ajouter un nouveau layer : **"Minion"**

### Configuration de la Collision Matrix
**Edit → Project Settings → Physics → Layer Collision Matrix**

Configure les interactions suivantes :

| Layer | Player | Enemy | Projectile | Minion | Obstacle |
|-------|--------|-------|------------|--------|----------|
| **Minion** | ✅ | ✅ | ❌ | ❌ | ✅ |

**Explication** :
- ✅ **Minion ↔ Player** : Les minions peuvent interagir avec le joueur (suivi)
- ✅ **Minion ↔ Enemy** : Les minions peuvent attaquer les ennemis
- ❌ **Minion ↔ Projectile** : Les projectiles du joueur PASSENT À TRAVERS les minions
- ❌ **Minion ↔ Minion** : Les minions ne se bloquent pas entre eux
- ✅ **Minion ↔ Obstacle** : Les minions évitent les obstacles (pathfinding)

## 2. Création d'un MinionData (ScriptableObject)

### Via Unity Editor
1. **Assets → Create → Survivor → Minion Data**
2. Configurer les paramètres :

```
[Minion Info]
- Minion Name: "Skeleton Warrior"
- Type: Melee (ou Ranged)

[Movement]
- Move Speed: 3.5
- Follow Distance: 3.0 (distance autour du joueur)
- Max Follow Range: 15.0 (téléporte si trop loin)
- Stop Distance: 1.5 (distance d'arrêt pour mélée)

[Combat - Melee]
- Base Damage: 10
- Attack Range: 1.5
- Attack Cooldown: 1.5

[Combat - Ranged]
- Base Damage: 8
- Attack Range: 8.0
- Attack Cooldown: 2.0
- Projectile Speed: 8.0
- Projectile Prefab: [Assign projectile]

[Stats]
- Max Health: 50
- Duration: 20 (0 = infini)

[Visual]
- Prefab: [Assign minion prefab]
```

## 3. Création d'un Prefab de Minion

### Setup du Prefab
1. Créer un GameObject
2. Ajouter les composants :
   - **MinionController** (script)
   - **Rigidbody** (isKinematic = false)
   - **Collider** (CapsuleCollider recommandé)
3. **Configurer le Layer : "Minion"**
4. Assigner le **MinionData** dans MinionController
5. Sauvegarder comme prefab

### Avec Animation (Optionnel)
Si vous voulez que votre minion ait des animations :

1. Ajouter un sous-GameObject avec :
   - **Animator** component (avec AnimatorController)
   - **MinionAnimator** script
   - Le modèle 3D / Sprite

2. **AnimatorController Setup** :
   - Paramètre **float "Speed"** : Vitesse de déplacement (0 = idle, >0 = walk/run)
   - Paramètre **trigger "Attack"** : Déclenche l'animation d'attaque

3. **Animation Events** :
   - Sur l'animation d'attaque, ajouter un **Animation Event** à la frame de frappe
   - Fonction : `OnAttackFrame()` (appelle MinionAnimator.OnAttackFrame)

### Exemple de Hiérarchie
```
SkeletonWarrior (Prefab)
├─ MinionController
├─ Rigidbody
├─ CapsuleCollider
└─ Visual (GameObject avec Animator)
   ├─ MinionAnimator
   ├─ Animator (AnimatorController)
   └─ SkinnedMeshRenderer / SpriteRenderer
```

### Exemple de Hiérarchie SANS Animation
```
SkeletonWarrior (Prefab)
├─ MinionController
├─ Rigidbody
├─ CapsuleCollider
└─ Visual (Mesh/Sprite direct)
```

## 4. Configuration de l'Effet Necrotic

### Dans SpellEffect (ScriptableObject)
```csharp
[SpellEffect - Necrotic]
- Element Type: Necrotic
- Minion Spawn Chance: 0.0 (base, sera augmenté par upgrades)
- Minion Prefab: [Assign minion prefab]
```

### Dans les Upgrades RuneSO (Necrotic Effect)
Utiliser les nouveaux champs dans **RuneStats** :

```
[Common Upgrade]
Stats:
  - FlatMinionChance: 0.1 (+10% chance de spawn)
  - FlatMaxMinions: 1 (+1 minion maximum)

[Rare Upgrade]
Stats:
  - FlatMinionChance: 0.15 (+15%)
  - FlatMaxMinions: 1

[Epic Upgrade]
Stats:
  - FlatMinionChance: 0.2 (+20%)
  - FlatMaxMinions: 2

[Legendary Upgrade]
Stats:
  - FlatMinionChance: 0.25 (+25%)
  - FlatMaxMinions: 3
  - DamageMult: 0.5 (+50% dégâts pour les minions?)
```

## 5. Configuration de la Scène

### Ajouter MinionManager
1. Créer un GameObject vide : **"MinionManager"**
2. Ajouter le composant **MinionManager**
3. Configurer :
   ```
   - Player Transform: [Drag PlayerController]
   - Obstacle Layer: [Select "Obstacle" layer]
   - Max Minions Capacity: 50
   - Rotation Speed: 8
   - Avoidance Blend Speed: 3
   ```

### Ajouter MinionPool
1. Créer un GameObject vide : **"MinionPool"**
2. Ajouter le composant **MinionPool**
3. Configurer :
   ```
   - Max Pool Size Per Prefab: 20
   - Spawn Pop Duration: 0.3
   - Spawn Pop Curve: [Animation curve]
   ```

## 6. Types de Minions

### Minion Mélée
- **Comportement** : Suit le joueur, charge l'ennemi le plus proche, attaque au contact
- **Use Case** : Tank, blocage d'ennemis
- **Config** : `Type = Melee`, `Attack Range = 1.5`, `Stop Distance = 1.5`

### Minion Ranged
- **Comportement** : Suit le joueur, tire des projectiles à distance
- **Use Case** : DPS, support à distance
- **Config** : `Type = Ranged`, `Attack Range = 8.0`, `Projectile Prefab = [assign]`

## 7. Gameplay

### Invocation
- Tuez un ennemi avec un sort Necrotic
- Chance de spawn basée sur `MinionChance` (accumulé via upgrades)
- Position de spawn : Position de l'ennemi tué

### Limite de Minions
- Définie par `MaxMinions` (accumulé via upgrades)
- Si limite atteinte, aucun nouveau minion ne spawn
- Conseil : Commencer avec MaxMinions = 2-3, augmenter via upgrades

### Comportement
- **Suivi** : Les minions orbitent autour du joueur à `FollowDistance`
- **Téléportation** : Si trop loin (`MaxFollowRange`), téléporte vers le joueur
- **Combat** : Attaque automatiquement l'ennemi le plus proche dans `AttackRange`
- **Mort** : Dégâts = 0 HP, ou durée expirée

## 8. Performance

### Optimisations Incluses
- ✅ **Job System** : Mouvement parallélisé avec Burst compilation
- ✅ **Object Pooling** : Pas de Instantiate/Destroy en runtime
- ✅ **Spatial Queries** : Utilise EnemyManager.GetTarget() (optimisé)

### Conseils
- Limiter `maxMinionsCapacity` à 50 max
- Utiliser des minions avec faible poly count
- Activer Burst compilation dans Project Settings

## 9. Debugging

### Vérifier le Spawn
```csharp
// Dans ProjectileDamageHandler.cs ligne 142-157
// Ajouter Debug.Log pour tracer les spawns
Debug.Log($"[Necrotic] Spawning minion at {spawnPos}, Chance: {def.MinionChance}, MaxMinions: {def.MaxMinions}");
```

### Vérifier le MinionManager
```csharp
// Dans Update() de votre UI
if (MinionManager.Instance != null)
{
    Debug.Log($"Active Minions: {MinionManager.Instance.ActiveMinionCount}");
}
```

## 10. Exemple de Setup Complet

### Spell Effect (Necrotic)
- `minionSpawnChance = 0.0` (base)
- `minionPrefab = SkeletonWarrior_Prefab`

### Rune Necrotic (Upgrades)
- **Common** : +10% chance, +1 max
- **Rare** : +15% chance, +1 max
- **Epic** : +20% chance, +2 max
- **Legendary** : +25% chance, +3 max

### Résultat Final
Avec 4 upgrades Legendary :
- `MinionChance = 100%` (spawn garanti)
- `MaxMinions = 12` (12 minions actifs max)

---

## Troubleshooting

### "Minions ne spawnent pas"
1. Vérifier que `MinionChance > 0` (via upgrades)
2. Vérifier que `MaxMinions > 0` (via upgrades)
3. Vérifier que le prefab a un `MinionController` avec `MinionData` assigné

### "Minions traversent les obstacles"
1. Vérifier le Layer "Minion" → Collision avec "Obstacle"
2. Vérifier que `obstacleLayer` est assigné dans MinionManager

### "Projectiles tuent les minions"
1. **Physics → Layer Collision Matrix**
2. Décocher **Minion ↔ Projectile**

### "Minions ne bougent pas"
1. Vérifier que `MinionMovementSystem` est initialisé
2. Vérifier que `playerTransform` est assigné dans MinionManager
3. Vérifier que `Rigidbody` est sur le prefab (isKinematic = false)

---

## Prochaines Étapes

- Créer des variantes de minions (Warrior, Archer, Mage)
- Ajouter des VFX de spawn
- Ajouter des sons d'attaque pour les minions
- Créer des upgrades spécifiques aux minions (durée, dégâts, vitesse)
