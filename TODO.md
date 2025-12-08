# TODO List : SoulWeaver

Ce document recense les fonctionnalités manquantes, les améliorations à apporter et le contenu à produire pour atteindre la version Alpha jouable.

## 🚨 Priorité Haute : Mécaniques Manquantes

### 1. L'IA des Minions (Nécromancie)
Actuellement, l'effet `Nécrotique` instancie un Prefab inerte.
- [ ] Créer `MinionController.cs`.
- [ ] Implémenter le mouvement (Suivre le joueur en formation ou nuage).
- [ ] Implémenter la détection (Trouver l'ennemi le plus proche).
- [ ] Implémenter l'attaque (Utiliser une `SpellDefinition` simplifiée pour tirer).
- [ ] Gérer la durée de vie et la mort des minions.

### 2. La Carte Infinie (Chunking)
Le joueur se déplace actuellement sur un plan fini.
- [ ] Créer le système de **Chunks** (Tuiles de terrain).
- [ ] Implémenter le repositionnement dynamique (Tapis roulant) pour donner l'illusion de l'infini.
- [ ] Générer des obstacles procéduraux (Arbres, Ruines) dans les chunks pour valider le Steering des ennemis.

### 3. Système de "Loot" Physique
- [ ] Créer le prefab visuel pour le **Coffre** et le **Cristal de Soin**.
- [ ] Vérifier la collision des projectiles avec les objets `IDamageable` (Déjà codé, à tester en scène).
- [ ] Finaliser l'UI de récompense pour l'Autel (Ouvrir le menu de choix sans Level Up).

---

## 🛠️ Améliorations & Refactoring (Tech)

### UI / UX
- [ ] **Feedback Visuel :** Ajouter des couleurs ou des icônes pour distinguer clairement les types de cartes (Forme vs Mod vs Stat).
- [ ] **Pause Menu :** Pouvoir reprendre, recommencer ou quitter.

### Combat
- [ ] **VFX Manager :** Remplacer les changements de couleur (`tintColor`) par de vrais effets de particules (Explosions, Traînées).
- [ ] **Hit Flash :** Faire clignoter les ennemis en blanc quand ils sont touchés.
- [ ] **Sons :** Ajouter un `AudioManager` pour les tirs, impacts et level ups.

---

## 📦 Production de Contenu (Data)

Il faut remplir les ScriptableObjects pour créer la variété du jeu.

### 1. Formes (Forms)
- [ ] **Boomerang :** Projectile qui part et revient (Nécessite une nouvelle `MotionStrategy`).
- [ ] **Aura :** Zone de dégâts constante autour du joueur (Similaire à Orbit mais sans projectile).

### 2. Effets (Effects)
- [ ] **Poison :** Dégâts sur la durée (DoT) cumulables.
- [ ] **Vampirisme :** Chance de soin au touché.
- [ ] **Void :** Attire les ennemis vers le centre de l'impact (Implosion).

### 3. Modificateurs (Modifiers)
- [ ] **Heavy :** Dégâts+++, Taille++, Vitesse--, Cooldown--.
- [ ] **Sniper :** Vitesse+++, Portée+++, Cone réduit.
- [ ] **Echo :** Chance de relancer le sort une seconde fois gratuitement.

### 4. Stats Passives (StatUpgrades)
- [ ] Créer les Assets pour : Armor, Regen, Growth.

### 5. Ennemis & Vagues
- [ ] Configurer une boucle de jeu de 10 minutes dans `WaveManager`.
- [ ] Créer les prefabs visuels distincts pour : Squelette, Archer, Chien, Golem.

---

## 🏆 Meta-Progression (Long Terme)

- [ ] **Sauvegarde :** Stocker l'or et les Unlocks (Json).
- [ ] **Menu Principal :** Armurerie pour acheter des améliorations permanentes (ex: +1 Reroll de base).
- [ ] **Unlock System :** Débloquer la rune "Météore" après avoir tué le premier Boss.
- [ ] **Leaderboards :** Leaderboards du nombre de kils / temps survécus en partie pour que les joueurs soient en compétition
