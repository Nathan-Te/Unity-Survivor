# Devlog : Unity Survivor - État d'Avancement

## 1. Architecture & Performance (Core Tech)
Le socle technique est conçu pour supporter des centaines d'unités simultanées (Horde Survivor).

* **Job System & Burst Compiler :** Déplacement des ennemis géré via `IJobParallelForTransform` pour des performances maximales.
* **Optimisation des Collisions :** Utilisation d'un `Dictionary` de cache pour éviter les `GetComponent` coûteux lors des impacts.
* **Object Pooling :** Système générique pour recycler les objets et éviter l'allocation mémoire (GC).
    * `EnemyPool` (Support multi-prefabs).
    * `ProjectilePool`.
    * `GemPool` (XP).
    * `DamageTextPool` (UI World Space).
* **Design Pattern "Data-Driven" :** Tout le contenu (Sorts, Ennemis, Vagues) est défini via des `ScriptableObjects`, sans toucher au code.

## 2. Système de Combat Modulaire (Spell System)
Architecture flexible permettant de composer des sorts à la volée.

### Structure des Données
Un sort n'est plus un objet figé, mais un assemblage de **Runes** :
* **SpellForm (Le Conteneur) :** Définit le pattern de tir et le mouvement (Ex: *Bolt, Nova, Orbit, Smite*).
* **SpellEffect (L'Impact) :** Définit l'élément et l'effet au contact (Ex: *Feu, Glace, Physique, Foudre, Nécromancie*).
* **SpellModifier (Le Boost) :** Altère les statistiques (Ex: *Multicast, Pierce, Giant, Homing*).

### Logique Runtime
* **SpellBuilder :** Classe statique qui combine les stats de base des Runes + les bonus accumulés pour générer une `SpellDefinition`.
* **ProjectileMotion (Strategy Pattern) :** Le contrôleur de projectile délègue son mouvement à une stratégie dédiée :
    * `LinearMotion` : Mouvement droit avec option Homing (Guidage).
    * `OrbitMotion` : Rotation autour du joueur (répartition symétrique).
    * `SmiteMotion` : Apparition retardée sur zone (Météore).
* **Système de Tags :** Bitmask pour gérer les incompatibilités (ex: empêcher "Pierce" sur un "Smite").
* **Mécaniques Spéciales :**
    * **Chain Lightning :** Rebond dynamique entre ennemis proches.
    * **Nécromancie :** Chance de spawn un minion à la mort de la cible.

## 3. Progression & RPG (Stats & Rareté)
Système de montée en puissance infini basé sur l'accumulation de stats.

* **Rareté Pondérée :** Les Runes ont une rareté (Commun à Légendaire) qui définit la puissance du boost lors de l'acquisition.
* **Runes Dynamiques :** Chaque Rune stockée par le joueur possède un `Level` et des `AccumulatedStats`. Chaque carte choisie ajoute ses stats à la rune existante.
* **Stat Upgrades :** Possibilité d'améliorer les stats passives du joueur (Vitesse, PV Max, Ramassage, Dégâts Globaux).
* **XP & Level Up :** Courbe d'XP exponentielle configurable par paliers (`XpTier`).

## 4. Monde & Ennemis (Game Loop)
Gestion du rythme et de l'environnement.

* **Wave Manager :** Gestionnaire de vagues configurable.
    * Spawns aléatoires pondérés (Horde).
    * Spawns fixes chronométrés (Élites/Boss).
* **IA Ennemis (Steering) :**
    * Évitement d'obstacles (Raycasts).
    * Comportements variés : Mêlée (Fonceur), Tireur (Fuit si trop près, s'arrête à distance), Chargeur.
* **Points d'Intérêt (POI) :** Système interactif sur la carte.
    * `DestructiblePOI` : Cristaux de soin, Coffres à butin.
    * `ZonePOI` : Autels (chargement par présence ou kills).
    * **Rune Altar :** Permet de déclencher un draft de récompense filtré (ex: "Uniquement des Runes Épiques").
* **La Carte Infinie :** Chunking
   * Le joueur se déplace actuellement sur un plan infini
   * Implémenter le repositionnement dynamique (Tapis roulant) pour donner l'illusion de l'infini.
   * Génératon des obstacles procéduraux (Arbres, Ruines) dans les chunks pour valider le Steering des ennemis ainsi que de Decals et éléments de décors.

## 5. Interface Utilisateur (UI/UX)
* **HUD Joueur :**
    * Slots de sorts actifs avec visualisation des Runes (Forme/Effet/Mods) et de leurs niveaux.
    * Barre de vie et d'XP.
* **Level Up (Draft) :**
    * Tirage de 3 cartes aléatoires.
    * **Flow intelligent :** Si l'inventaire est plein ou si on améliore un modificateur, l'UI passe en mode "Ciblage" pour demander au joueur où appliquer l'amélioration.
    * Options de **Reroll** et de **Ban**.
    * Description dynamique des cartes (Comparatif "Stat Actuelle -> Stat Future").
* **Feedbacks :**
    * Floating Damage Text (Poolé).
    * Barre de vie de Boss.

## 6. Outils de Développement (Tools)
* **Wave Manager Dashboard :** Fenêtre Editor personnalisée pour configurer les vagues et les ennemis sans passer par l'Inspecteur par défaut.
* **Game Director :** Console de debug runtime (Touche `²`) pour tester rapidement (God Mode, Kill All, Level Up, Time Scale).
