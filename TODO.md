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

---

## 🛠️ Améliorations & Refactoring (Tech)

### UI / UX
- [ ] **Feedback Visuel :** Ajouter des couleurs ou des icônes pour distinguer clairement les types de cartes (Forme vs Effect vs Mod vs Stat).
- [ ] **Pause Menu :** Pouvoir reprendre, recommencer ou quitter.

### Combat
- [ ] **Visuel :** Ajouter un effet visuel à la mort des ennemis
- [ ] **Sons :** Ajouter un `AudioManager` pour les tirs, impacts et level ups et de la musique

### LevelUp

avec toutes les runes vérifier que xe n'est pas trop compliqué d'avoir ce qu'il faut, peut-être ne plus pouvoir remplacer une rune en place ? afin de limiter les runes proposées par que ce qu'on a si tous les slots sont pris

---

## 📦 Production de Contenu (Data)

Il faut remplir les ScriptableObjects pour créer la variété du jeu.

Tester le Nova avec des Kunai en Physical
Trouver un prefab pour l'orbit Physical

Bolt Lighting doit être un éclair
smite Lightning doit être de la foudre tombant du ciel

bolt fire doit être une boule de feu
smite fire doit être une météorite

## 📦 Création de nouveau Contenu (Data)

### 1. Formes (Forms)
- [ ] **Boomerang :** Projectile qui part et revient (Nécessite une nouvelle `MotionStrategy`).
- [ ] **Aura :** Zone de dégâts constante autour du joueur (Similaire à Orbit mais sans projectile).

### 2. Effets (Effects)
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
