# Guide d'Animation pour Minions

## Vue d'ensemble

Le système d'animation des minions est **optionnel** et basé sur le système `EnemyAnimator.cs`. Il inclut :
- Optimisation LOD (Level of Detail) basée sur la distance
- Paramètres Animator : `Speed` (float) et `Attack` (trigger)
- Animation Events pour synchroniser les dégâts avec l'animation
- Culling automatique si hors caméra

---

## Setup Simple (Sans Animation)

Si vous ne voulez PAS d'animation :
- **N'ajoutez PAS** de composant `MinionAnimator`
- Les attaques seront exécutées instantanément
- Tout fonctionne normalement !

---

## Setup Avec Animation

### 1. Structure du Prefab

```
SkeletonWarrior (Prefab)
├─ MinionController                    ← Script principal
├─ Rigidbody
├─ CapsuleCollider
└─ Visual (GameObject)                 ← Contient l'animation
   ├─ MinionAnimator                   ← Script d'animation
   ├─ Animator                         ← Component Unity
   └─ SkinnedMeshRenderer              ← Modèle 3D
```

### 2. Créer l'AnimatorController

**Assets → Create → Animator Controller** (ex: `SkeletonWarrior_Anim`)

#### Paramètres
1. **Speed** (float) : Vitesse de déplacement
   - 0 = Idle
   - > 0 = Walk/Run
2. **Attack** (trigger) : Déclenche l'animation d'attaque

#### États
```
Entry → Idle
Idle ↔ Walk (transition quand Speed > 0.1 / Speed < 0.1)
Any State → Attack (via trigger Attack)
Attack → Idle (quand animation terminée)
```

### 3. Configuration des Transitions

#### Idle → Walk
- Condition : `Speed > 0.1`
- Has Exit Time : ✅ (coché)
- Exit Time : 0.9
- Transition Duration : 0.2s

#### Walk → Idle
- Condition : `Speed < 0.1`
- Has Exit Time : ✅ (coché)
- Exit Time : 0.9
- Transition Duration : 0.2s

#### Any State → Attack
- Condition : `Attack` (trigger)
- Has Exit Time : ❌ (décoché)
- Transition Duration : 0.1s

#### Attack → Idle
- Condition : Aucune
- Has Exit Time : ✅ (coché)
- Exit Time : 0.95
- Transition Duration : 0.1s

### 4. Ajouter Animation Events

Sur l'animation **Attack** :

1. Ouvrir l'animation dans la fenêtre **Animation**
2. Cliquer sur la frame où le coup doit frapper (ex: 50% de l'animation)
3. **Add Event** → Sélectionner la fonction : `OnAttackFrame`

**Important** : La fonction `OnAttackFrame()` est définie dans `MinionAnimator.cs`

### 5. Assigner dans le Prefab

1. Sur le GameObject "Visual" :
   - Ajouter le composant **MinionAnimator**
   - Ajouter le composant **Animator**
   - Assigner l'AnimatorController dans Animator
2. Sur le GameObject racine (SkeletonWarrior) :
   - Le `MinionController` trouvera automatiquement le `MinionAnimator` enfant

---

## Flow d'Attaque

### Avec Animation
```
1. MinionController.UpdateCombat() détecte un ennemi
2. MinionController.PerformAttack()
3. MinionAnimator.TriggerAttackAnimation()
4. Animator joue l'animation "Attack"
5. Animation Event sur frame de frappe → MinionAnimator.OnAttackFrame()
6. MinionAnimator.OnAttackFrame() → MinionController.OnAnimationAttackFrame()
7. MinionController.ExecuteAttack() → Damage/Projectile
```

### Sans Animation (Fallback)
```
1. MinionController.UpdateCombat() détecte un ennemi
2. MinionController.PerformAttack()
3. Pas d'animateur détecté → ExecuteAttack() immédiatement
4. Damage/Projectile appliqué
```

---

## Optimisations LOD

Le `MinionAnimator` ajuste automatiquement la fréquence de mise à jour selon la distance au joueur :

| Distance | Update Interval | Notes |
|----------|----------------|-------|
| **< 4m** | Chaque frame (1) | HIGH quality - Proche du joueur |
| **4-10m** | Toutes les 3 frames | MED quality - Distance moyenne |
| **> 10m** | Toutes les 6 frames | LOW quality - Loin du joueur |

**Avantages** :
- CPU économisé sur les minions lointains
- Animations fluides pour les minions proches
- Frame offset aléatoire (évite les pics CPU)

---

## Paramètres Animator

### Speed (float)
Calculé automatiquement par `MinionAnimator.Update()` :

```csharp
float distanceMoved = (currentPos - lastPos).magnitude;
float speed = distanceMoved / (Time.deltaTime * updateInterval);
animator.SetFloat("Speed", speed);
```

**Usage dans Animator** :
- Blend Tree : Idle (Speed = 0) → Walk (Speed = 1-3) → Run (Speed = 3+)
- Ou simple transition : Idle ↔ Walk

### Attack (trigger)
Déclenché manuellement par `MinionController.PerformAttack()` :

```csharp
minionAnimator.TriggerAttackAnimation();
// → animator.SetTrigger("Attack");
```

---

## Exemples d'AnimatorController

### Exemple Simple (Idle/Walk/Attack)
```
States:
- Idle (default)
- Walk (blended with Idle via Speed)
- Attack (trigger from Any State)

Parameters:
- Speed (float)
- Attack (trigger)

Transitions:
- Idle → Walk : Speed > 0.1
- Walk → Idle : Speed < 0.1
- Any State → Attack : Attack trigger
- Attack → Idle : Exit Time
```

### Exemple Avancé (Blend Tree)
```
Blend Tree (1D, Speed):
├─ Idle (Speed = 0)
├─ Walk (Speed = 2)
└─ Run (Speed = 5)

States:
- Movement (Blend Tree)
- Attack (trigger)

Parameters:
- Speed (float)
- Attack (trigger)

Transitions:
- Any State → Attack : Attack trigger
- Attack → Movement : Exit Time
```

---

## Animation Event Setup (Détaillé)

### Pour Melee Minion
1. Ouvrir l'animation "Attack" dans la fenêtre **Animation**
2. Chercher la frame de frappe (généralement 40-60% de l'animation)
3. Cliquer sur cette frame
4. **Add Event** → `OnAttackFrame`
5. Tester : Le coup doit frapper visuellement au moment exact

### Pour Ranged Minion
1. Ouvrir l'animation "Attack"
2. Chercher la frame de lâcher du projectile (ex: frame où la main relâche)
3. **Add Event** → `OnAttackFrame`
4. Le projectile sera spawné à cette frame exacte

---

## Debugging

### Animation ne joue pas
1. Vérifier que `MinionAnimator` est sur un GameObject enfant avec `Animator`
2. Vérifier que l'AnimatorController est assigné
3. Vérifier les paramètres "Speed" et "Attack" existent dans l'AnimatorController
4. Console : Chercher des warnings Animator

### Attack Event ne se déclenche pas
1. Ouvrir l'animation "Attack" dans la fenêtre Animation
2. Vérifier qu'il y a bien un Event marker sur la timeline
3. Vérifier que l'Event appelle la fonction `OnAttackFrame`
4. Vérifier que `MinionAnimator` a bien la méthode `OnAttackFrame()`

### Animation trop rapide/lente
1. Vérifier la vitesse de l'animation clip (ex: 1.0x)
2. Vérifier les Transition Durations (0.1-0.2s recommandé)
3. Vérifier le paramètre Speed (doit être calculé automatiquement)

### Minions lointains ont des animations saccadées
C'est **normal** ! Le LOD system réduit la fréquence d'update à distance pour économiser du CPU. Si vous voulez des animations plus fluides :

1. Augmenter `DIST_HIGH_QUALITY` dans `MinionAnimator.cs`
2. Réduire `updateInterval` pour les distances moyennes/lointaines
3. **Attention** : Impact sur les performances !

---

## Configuration Recommandée

### Pour Minions Melee
- **Idle** : Posture de combat (shield up, weapon ready)
- **Walk** : Marche agressive
- **Attack** : Swing rapide (0.5-0.8s)
- **Event Frame** : 50% de l'animation

### Pour Minions Ranged
- **Idle** : Posture arc/bâton en main
- **Walk** : Marche normale
- **Attack** : Tir arc ou cast spell (0.6-1.0s)
- **Event Frame** : Frame de release (60-70%)

---

## Performance Tips

1. **Utilisez AnimatorCullingMode.CullUpdateTransforms** (déjà configuré)
2. **Limitez le nombre de bones** dans les rigs (< 30 pour minions)
3. **Désactivez "Apply Root Motion"** (mouvement géré par MinionMovementSystem)
4. **Utilisez des animations courtes** (Idle < 2s, Walk < 1s, Attack < 1s)
5. **Évitez les Blend Trees complexes** (max 3-4 animations par Blend Tree)

---

## Comparaison avec EnemyAnimator

| Feature | EnemyAnimator | MinionAnimator |
|---------|---------------|----------------|
| **LOD System** | ✅ Oui | ✅ Oui (identique) |
| **Speed Parameter** | ✅ Oui | ✅ Oui |
| **Attack Parameter** | ✅ Oui | ✅ Oui |
| **Frame Offset** | ✅ Oui | ✅ Oui |
| **Culling Mode** | ✅ CullUpdateTransforms | ✅ CullUpdateTransforms |
| **Renderer Check** | ✅ Oui | ✅ Oui |
| **Callback** | `EnemyRangedCombat.SpawnProjectile()` | `MinionController.OnAnimationAttackFrame()` |

**Différences** :
- `MinionAnimator` appelle `MinionController` au lieu de `EnemyController`
- Sinon, architecture identique !

---

## Exemple de Setup Complet (Skeleton Warrior)

### 1. Créer l'AnimatorController
**Assets → Create → Animator Controller** : `SkeletonWarrior_Anim`

### 2. Paramètres
- `Speed` (float)
- `Attack` (trigger)

### 3. États
- **Idle** : `Skeleton_Idle.anim`
- **Walk** : `Skeleton_Walk.anim`
- **Attack** : `Skeleton_Attack.anim`

### 4. Transitions
- Idle ↔ Walk (via Speed)
- Any State → Attack (via trigger)
- Attack → Idle (Exit Time)

### 5. Animation Event
Sur `Skeleton_Attack.anim` :
- Frame 15 (sur 30) : Event `OnAttackFrame`

### 6. Prefab
```
SkeletonWarrior
├─ MinionController (data = SkeletonWarrior_Data)
├─ Rigidbody
├─ CapsuleCollider
└─ Visual
   ├─ MinionAnimator
   ├─ Animator (Controller = SkeletonWarrior_Anim)
   └─ SkeletonMesh
```

---

## Résumé

✅ **Optionnel** : Fonctionne sans animation
✅ **LOD** : Performance optimisée automatiquement
✅ **Simple** : 2 paramètres Animator seulement
✅ **Event-based** : Synchronisation attaque/animation
✅ **Identique à EnemyAnimator** : Architecture éprouvée

**Conseil** : Commencez SANS animation pour tester le gameplay, ajoutez les animations ensuite !
