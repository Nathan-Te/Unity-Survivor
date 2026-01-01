# Système de Minions (Necrotic Effect)

## Vue d'ensemble
Système complet d'invocation de créatures alliées pour le jeu Survivor. Les minions sont invoqués lorsque le joueur tue des ennemis avec l'effet Necrotic.

## Fichiers Créés

### Scripts
```
Assets/Scripts/Entities/Minions/
├── MinionData.cs              - ScriptableObject définissant les stats d'un type de minion
├── MinionController.cs        - Contrôleur principal d'un minion
├── MinionAnimator.cs          - Système d'animation avec LOD (optionnel)
├── MinionMovementSystem.cs   - Job System pour mouvement parallélisé (Burst)
├── MinionPool.cs             - Object pooling pour performance
└── MinionManager.cs          - Singleton coordinateur du système
```

### Documentation
```
Assets/Scripts/Entities/Minions/
├── MINION_SETUP.md           - Guide de configuration pas-à-pas
├── MINION_ARCHITECTURE.md    - Architecture technique détaillée
└── README.md                 - Ce fichier
```

## Modifications Apportées

### 1. RuneStats.cs
**Nouveaux champs** :
- `float FlatMinionChance` : +% chance de spawn de minion
- `int FlatMaxMinions` : +nombre de minions actifs maximum

**Opérateur `+`** : Ajout de l'accumulation de ces stats

### 2. SpellDefinition.cs
**Nouveaux champs** :
- `int MaxMinions` : Limite de minions actifs (calculé par SpellBuilder)

**Champs existants utilisés** :
- `float MinionChance` : Chance finale de spawn (0-1)
- `GameObject MinionPrefab` : Prefab à instancier

### 3. SpellBuilder.cs
**Accumulation des stats de minions** :
- Depuis Effect rune : `effectStats.FlatMinionChance`, `effectStats.FlatMaxMinions`
- Depuis Modifier runes : Accumulation dans la boucle des modifiers

### 4. ProjectileDamageHandler.cs (ligne 142-157)
**Modification de la logique de spawn** :
- ✅ Avant : `Instantiate(def.MinionPrefab, ...)`
- ✅ Après : `MinionManager.Instance.SpawnMinion(minionData, spawnPos, def.MaxMinions)`
- **Respect de la limite MaxMinions**
- **Utilisation du système de pooling**

### 5. CLAUDE.md
**Ajouts** :
- Section "Minion System (Necrotic)"
- Ajout de `FlatMinionChance` et `FlatMaxMinions` dans RuneStats
- Ajout de MinionPool et MinionManager dans Scene-based Singletons
- Critical Rules #8 et #9 pour les minions

## Caractéristiques Techniques

### Performance
- ✅ **Job System** : Mouvement parallélisé avec Burst compilation
- ✅ **Object Pooling** : Aucun Instantiate/Destroy en runtime
- ✅ **Swap-back Removal** : O(1) pour l'unregister
- ✅ **Native Collections** : NativeArray, NativeList, TransformAccessArray

### Architecture
Inspirée de `EnemyMovementSystem` :
- Même structure de Job (`IJobParallelForTransform`)
- Même pattern de raycasts pour obstacles (3 directions)
- Même gestion des Native Collections
- Swap-back removal identique

### Collision Layers
**CRITICAL** : Layer "Minion" doit être créé
- ✅ Minion ↔ Player (suivi)
- ✅ Minion ↔ Enemy (combat)
- ❌ Minion ↔ Projectile (pass-through)
- ❌ Minion ↔ Minion (pas de collision)
- ✅ Minion ↔ Obstacle (pathfinding)

## Types de Minions Supportés

### Minion Melee
- Suit le joueur
- Charge l'ennemi le plus proche
- Attaque au contact (direct damage)
- Use case : Tank, bloquer les ennemis

### Minion Ranged
- Suit le joueur
- Tire des projectiles sur l'ennemi
- Attaque à distance (spawn projectile via ProjectilePool)
- Use case : DPS, support

## Configuration Rapide

### 1. Créer le Layer
**Edit → Project Settings → Tags and Layers**
- Ajouter layer : "Minion"

### 2. Configurer les Collisions
**Edit → Project Settings → Physics → Layer Collision Matrix**
- Décocher : Minion ↔ Projectile
- Décocher : Minion ↔ Minion

### 3. Créer un MinionData
**Assets → Create → Survivor → Minion Data**
- Configurer les stats (voir MINION_SETUP.md)

### 4. Créer le Prefab
- Ajouter `MinionController`, `Rigidbody`, `Collider`
- Assigner le layer "Minion"
- Assigner le MinionData

### 5. Ajouter à la Scène
- Créer GameObject "MinionManager" avec composant `MinionManager`
- Créer GameObject "MinionPool" avec composant `MinionPool`

### 6. Configurer Necrotic Effect
Dans les upgrades du Necrotic SpellEffect :
```
Common:  FlatMinionChance = 0.1,  FlatMaxMinions = 1
Rare:    FlatMinionChance = 0.15, FlatMaxMinions = 1
Epic:    FlatMinionChance = 0.2,  FlatMaxMinions = 2
Legendary: FlatMinionChance = 0.25, FlatMaxMinions = 3
```

## Exemple de Progression

### Niveau 1 (Common upgrade)
- Chance : 10%
- Max : 1 minion
- **Résultat** : 10% chance de spawn 1 minion

### Niveau 2 (+ Rare upgrade)
- Chance : 25%
- Max : 2 minions
- **Résultat** : 25% chance, max 2 minions

### Niveau 3 (+ Epic upgrade)
- Chance : 45%
- Max : 4 minions
- **Résultat** : 45% chance, max 4 minions

### Niveau 4 (+ Legendary upgrade)
- Chance : 70%
- Max : 7 minions
- **Résultat** : 70% chance, max 7 minions

### Build Maximal
Avec 4 upgrades Legendary :
- **Chance : 100%** (spawn garanti)
- **Max : 12 minions** actifs simultanément

## Debugging

### Vérifier le Spawn
Ajouter logs dans [ProjectileDamageHandler.cs:146](c:\Users\miste\Unity Projects\Survivor\Assets\Scripts\Combat\Projectiles\ProjectileDamageHandler.cs#L146) :
```csharp
Debug.Log($"[Necrotic] Minion spawned! Chance: {def.MinionChance}, Max: {def.MaxMinions}");
```

### Vérifier MinionManager
Dans Update() :
```csharp
Debug.Log($"Active Minions: {MinionManager.Instance.ActiveMinionCount}");
```

### Vérifier les Collisions
**Window → Analysis → Physics Debugger**
- Layer Collision Matrix
- Vérifier que Minion ↔ Projectile est décoché

## Prochaines Étapes (Extensions)

### Variantes de Minions
- **Tank** : High HP, taunt enemies
- **Mage** : AOE spells
- **Healer** : Heal player over time

### Upgrades Avancées
- Minion Damage Multiplier (+50% dmg)
- Minion Duration (+10s lifetime)
- Minion Speed (+20% move speed)
- Minion Explosion (damage on death)

### VFX/Audio
- Spawn VFX (particules, flash)
- Death VFX (dissolution)
- Attack sounds (melee hit, arrow shoot)
- Ambient sounds (breathing, footsteps)

## Support

Pour plus de détails :
- **Setup** : Voir [MINION_SETUP.md](MINION_SETUP.md)
- **Architecture** : Voir [MINION_ARCHITECTURE.md](MINION_ARCHITECTURE.md)
- **Gameplay** : Voir [CLAUDE.md](../../../CLAUDE.md) section "Minion System"

## Résumé Technique

**Lignes de code ajoutées** : ~900 lignes
**Fichiers modifiés** : 5
**Fichiers créés** : 11 (6 scripts + 5 docs)
**Systèmes intégrés** : Spell System, Job System, Pooling System, Collision System, Animation System

**Performance** :
- Job System : ✅ Burst compiled
- Pooling : ✅ No Instantiate/Destroy
- Collections : ✅ Native (NativeArray, NativeList)
- Complexity : O(1) add/remove, O(n) parallel movement

**Compatibilité** :
- Unity 2021.3+
- Burst Compiler 1.6+
- Collections Package 1.2+
