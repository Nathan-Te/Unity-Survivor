# 🧪 Cahier de Tests : SoulWeaver (Alpha)

**Version du test :** [Date du jour]
**Objectif :** Valider la stabilité des systèmes Core, Combat et Progression avant l'ajout de nouvelles features.

---

## 1. 🕹️ Contrôles & Mouvement Joueur
*Script : `PlayerController.cs`*

- [ ] **Déplacement WASD :** Le joueur se déplace fluidement.
- [ ] **Collisions Environnement :** Le joueur ne traverse pas les murs/arbres (Layer `Obstacle`).
- [ ] **Collisions Ennemis :** Le joueur est ralenti ou bloqué par la masse des ennemis.
- [ ] **Visée Automatique (Défaut) :** Le joueur regarde dans la direction de son déplacement.
- [ ] **Visée Manuelle (Toggle Clic Droit) :**
    - [ ] Le joueur s'oriente vers la souris indépendamment du mouvement.
    - [ ] Les projectiles partent vers le curseur.
- [ ] **Stats Passives :**
    - [ ] **Vitesse :** Une carte "Move Speed" augmente visiblement la vitesse.
    - [ ] **Régénération :** Si blessé, les PV remontent doucement (avec stat Regen > 0).

## 2. ⚔️ Système de Sorts (Combat Modulaire)
*Scripts : `SpellManager`, `ProjectileController`, `SpellBuilder`*

### A. Formes (Mouvement)
- [ ] **Bolt (Tir) :** Projectile rapide, droit. Disparaît au contact ou portée max.
- [ ] **Orbit (Bouclier) :**
    - [ ] Apparaît autour du joueur.
    - [ ] Tourne de façon symétrique.
    - [ ] Disparaît après la durée et réapparaît (Cycle Cooldown/Duration respecté).
    - [ ] Ne se détruit pas au premier impact (Pierce infini par défaut).
- [ ] **Smite (Météore) :**
    - [ ] Apparaît au sol (pas de projectile voyageur).
    - [ ] Délai d'attente avant impact.
    - [ ] Explosion de zone (AOE) à la fin du timer.

### B. Effets (Impact)
- [ ] **Physique :** Applique un recul (Knockback) visible sur les ennemis.
- [ ] **Glace :** Les ennemis touchés ralentissent (changement de `CurrentSpeed`).
- [ ] **Foudre (Chain) :**
    - [ ] Le projectile touche un ennemi.
    - [ ] Un *nouveau* projectile spawn et va vers un ennemi proche.
    - [ ] Vérifier que le nombre de rebonds respecte la stat `ChainCount`.
- [ ] **Nécrotique :** Tuer un ennemi a une chance de faire apparaître un objet (Minion placeholder).

### C. Modificateurs & Stats
- [ ] **Multicast :**
    - [ ] Bolt : Tire plusieurs projectiles en éventail (Spread).
    - [ ] Orbit : Ajoute des orbes au cercle.
- [ ] **Pierce :** Le Bolt traverse X ennemis avant de disparaître.
- [ ] **Homing :** Les projectiles courbent leur trajectoire vers l'ennemi le plus proche.
- [ ] **Taille (Giant) :** Les projectiles sont visuellement plus gros.

---

## 3. 💀 Ennemis & Vagues
*Scripts : `EnemyManager`, `WaveManager`, `EnemyController`*

- [ ] **Spawning :** Les ennemis apparaissent hors champ (ou au bord).
- [ ] **Steering (Navigation) :**
    - [ ] Les ennemis contournent les murs (Raycasts actifs).
    - [ ] Les ennemis s'écartent les uns des autres (ne forment pas un seul point).
- [ ] **Comportements Spécifiques :**
    - [ ] **Squelette (Mêlée) :** Fonce droit sur le joueur.
    - [ ] **Mage (Range) :** S'arrête à distance pour tirer. Recule si le joueur approche (Flee).
- [ ] **Vagues :**
    - [ ] Le type d'ennemis change avec le temps (selon config WaveManager).
    - [ ] Les événements "Timed Spawn" (Boss/Elite) se déclenchent au bon timing.
- [ ] **Mort :**
    - [ ] Disparition propre (Retour au Pool).
    - [ ] Drop d'une Gemme d'XP.
    - [ ] Affichage du `DamageText` (Chiffre flottant).

---

## 4. 📈 Progression & UI
*Scripts : `LevelManager`, `LevelUpUI`, `UpgradeData`*

### A. Expérience
- [ ] **Collecte :** Les gemmes volent vers le joueur quand on approche (`MagnetArea`).
- [ ] **Jauge :** La barre d'XP du HUD se remplit.
- [ ] **Level Up :** Le jeu se met en pause, le menu apparaît.

### B. Menu de Draft (Cartes)
- [ ] **Affichage :** 3 cartes sont proposées avec Titre, Icône, Description et Rareté.
- [ ] **Description Dynamique :**
    - [ ] Vérifier le comparatif (ex: "Dégâts : 10 -> <color=green>15</color>").
    - [ ] Vérifier l'affichage du gain de niveau lié à la rareté (ex: "+3 Niveaux").
- [ ] **Boutons Méta :**
    - [ ] **Reroll :** Change les 3 cartes (consomme un stock).
    - [ ] **Ban :** Retire une carte du pool pour toujours.

### C. Gestion de l'Inventaire
- [ ] **Nouveau Sort :**
    - [ ] Si emplacement libre : Ajout direct dans le HUD.
    - [ ] Si inventaire plein : L'UI demande quel sort remplacer.
- [ ] **Amélioration (Doublon) :**
    - [ ] Si on choisit une Forme déjà possédée : L'UI propose de l'améliorer ou de créer un doublon.
- [ ] **Modificateurs :**
    - [ ] Clic sur une carte Modificateur -> L'UI demande sur quel sort l'appliquer.
    - [ ] Si le sort a un slot libre : Ajout direct.
    - [ ] Si le sort est plein : Menu de remplacement ("Quel mod retirer ?").
    - [ ] **Incompatibilité :** Impossible de mettre un Mod sur une Forme qui ne le supporte pas (Message d'erreur ou grisé).

### D. HUD
- [ ] **Slots :** Les icônes des sorts, effets et modificateurs s'affichent.
- [ ] **Niveaux :** Les petits textes "Lvl X" sont à jour.
- [ ] **Vie :** La barre de vie descend quand on prend des dégâts et remonte avec le soin.

---

## 5. 🌍 Monde & POI
*Scripts : `PointOfInterest`, `DestructiblePOI`, `ZonePOI`*

- [ ] **Cristal de Soin :** Se casse sous les tirs -> Rend des PV.
- [ ] **Coffre :** Se casse -> Lâche une pluie de gemmes.
- [ ] **Autel (Rune) :**
    - [ ] Se charge quand on reste dans la zone (ou qu'on tue des ennemis).
    - [ ] Une fois chargé -> Ouvre le menu de récompense (Draft forcé).
- [ ] **Collisions :** Les projectiles du joueur touchent les POI, mais les projectiles ennemis/joueur s'arrêtent contre les murs.

---

## 6. 🐛 Outils de Debug
*Script : `GameDirector`*

- [ ] **Touche `²` :** La console s'ouvre.
- [ ] **Kill All :** Nettoie l'écran (test de performance massif).
- [ ] **+1 Level :** Déclenche le Level Up instantanément (test UI).
- [ ] **God Mode :** Le joueur ne prend plus de dégâts.
- [ ] **Timescale :** Le jeu accélère (x2, x5) sans bug physique majeur.