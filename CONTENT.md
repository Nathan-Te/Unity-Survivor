# 📜 Catalogue de Contenu - SoulWeaver

Ce document recense tous les ScriptableObjects (Données) du jeu pour faciliter l'équilibrage global.

---

## 1. Runes - Formes (Mouvement)
*Définit le pattern de tir et le comportement physique.*

| Nom (Asset) | Description | CD (s) | Count | Pierce | Vitesse | Spécial | Tags Supportés |
| :--- | :--- | :---: | :---: | :---: | :---: | :--- | :--- |
| **Bolt** | Tir standard rapide | 0.4 | 1 | 0 | 20 | Proc Coeff: 1.0 | All |
| **Nova** | Explosion circulaire | 1.5 | 8 | 0 | 15 | Spread: 360° | -Homing |
| **Orbit** | Bouclier rotatif | 5.0 | 3 | 999 | 10 | Durée: 5s | -Homing |
| **Smite** | Météore ciblé | 1.2 | 1 | N/A | N/A | Delay: 0.5s | Smite |
| *[Nouveau]* | ... | ... | ... | ... | ... | ... | ... |

---

## 2. Runes - Effets (Impact)
*Définit l'élément, les dégâts et les statuts.*

| Nom (Asset) | Élément | Dégâts Base | Status | Spécial (Chain/Minion) | Upgrades (Exemple) |
| :--- | :--- | :---: | :--- | :--- | :--- |
| **Physical** | Physique | 10 | Knockback++ | - | Dégâts bruts |
| **Fire** | Feu | 5 | Burn (DoT) | - | Durée du Burn |
| **Ice** | Glace | 5 | Slow | - | Force du Slow |
| **Lightning** | Foudre | 8 | - | Chain: 3 rebonds | +Rebonds, +Range |
| **Necrotic** | Nécro | 12 | - | Minion Chance: 10% | +Chance, +Stats Minion |

---

## 3. Runes - Modificateurs (Boost)
*Altère les statistiques du sort.*

| Nom (Asset) | Description | Stats Base (Niveau 1) | Restriction Tag |
| :--- | :--- | :--- | :--- |
| **Multicast** | Ajoute des projectiles | Count +1, Spread +15 | SupportsMulticast |
| **Pierce** | Traverse les ennemis | Pierce +1 | SupportsPierce |
| **Heavy** | Gros dégâts, lent | Dégâts +50%, Vitesse -20% | - |
| **Sniper** | Rapide et loin | Vitesse +50%, Range +10m | Projectile |
| **Homing** | Guidage automatique | Homing = True | SupportsHoming |

---

## 4. Stats Passives (StatUpgrades)
*Améliorations globales du personnage.*

| Nom | Stat Ciblée | Commun | Rare | Épique | Légendaire |
| :--- | :--- | :---: | :---: | :---: | :---: |
| **Haste** | Move Speed | +10% | +15% | +25% | +50% |
| **Vitality** | Max Health | +20 | +40 | +80 | +150 |
| **Magnet** | Magnet Area | +20% | +30% | +50% | +100% |
| **Might** | Global Damage | +10% | +20% | +35% | +60% |
| **Quick** | Global Cooldown | +5% | +10% | +15% | +25% |

---

## 5. Bestiaire (Ennemis)
*Configuration des `EnemyData`.*

| Nom | Type | HP | Dégâts | Vitesse | XP | Comportement (Stop/Flee) |
| :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| **Squelette** | Melee | 30 | 10 | 4 | 10 | Fonce droit |
| **Archer** | Ranged | 20 | 15 | 5 | 15 | Stop: 8m / Flee: 4m |
| **Chien** | Charger | 40 | 12 | 9 | 20 | Fonce droit (Rapide) |
| **Golem** | Elite | 300 | 40 | 2 | 100 | Immunisé Knockback |
| **[Boss]** | Boss | 5000 | 50 | 3 | 1000 | Patterns spéciaux |

---

## 6. Vagues (WaveManager)
*Déroulement de la partie (Timeline).*

* **Vague 1 (00:00 - 01:00)**
    * **Horde :** Squelette (100%)
    * **Events :** Aucun.
* **Vague 2 (01:00 - 02:00)**
    * **Horde :** Squelette (70%), Archer (30%)
    * **Events :** 01:30 -> Spawn 1 Golem (Elite).
* **Vague 3 (02:00 - 03:00)**
    * **Horde :** Squelette (50%), Archer (30%), Chien (20%)
    * **Events :** 02:50 -> Spawn BOSS.