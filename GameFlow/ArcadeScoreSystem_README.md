# Arcade Score System - Guide de Configuration

## 📊 Vue d'ensemble

Le système de score arcade avec combo dynamique permet de créer une progression challengeante où les paliers supérieurs deviennent de plus en plus difficiles à maintenir.

## ⚙️ Paramètres

### Settings
- **Combo Mode** :
  - **Arcade** : Timer à 0 → Combo reset à 0, Multiplicateur à x1
  - **Modern** : Timer à 0 → Combo baisse d'un palier (plus permissif)

### Combo Timer - Base Settings
- **Combo Timer Max** : Durée maximale du timer (ex: 3 secondes)
- **Base Decay Rate** : Vitesse de décroissance au palier 0 (ex: 1 = 1 seconde par seconde)
- **Base Time Added Per Kill** : Temps ajouté par kill au palier 0 (ex: 1.5 secondes)

### Multiplier Settings
- **Combo Thresholds** : Paliers de combo (ex: [0, 5, 10, 20, 50, 100])
- **Multipliers** : Multiplicateurs de score (ex: [x1, x1.5, x2, x3, x5, x10])

### Dynamic Difficulty Per Tier
- **Decay Rate Multipliers** : Vitesse de décroissance par palier
  - Valeur > 1 = Plus rapide (plus difficile)
  - Exemple : [1, 1.2, 1.5, 2, 2.5, 3]
  - Au palier 5 (x10), le timer se vide 3x plus vite !

- **Time Added Multipliers** : Temps ajouté par kill par palier
  - Valeur < 1 = Moins de temps (plus difficile)
  - Exemple : [1, 0.9, 0.8, 0.7, 0.6, 0.5]
  - Au palier 5 (x10), on gagne seulement 50% du temps de base !

### Visual Feedback
- **Clamp Timer To Max** : Empêche le timer de dépasser le maximum

## 🎯 Exemples de Configuration

### Configuration Équilibrée (Recommandé)
```
Palier  Combo  Multi   Decay   TimeAdded   Difficulté
0       0      x1.0    x1.0    x1.0        Facile
1       5      x1.5    x1.2    x0.9        Normal
2       10     x2.0    x1.5    x0.8        Moyen
3       20     x3.0    x2.0    x0.7        Difficile
4       50     x5.0    x2.5    x0.6        Très difficile
5       100    x10.0   x3.0    x0.5        Expert
```

**Calcul d'exemple au palier 5 :**
- Base Decay Rate = 1.0
- Decay au palier 5 = 1.0 × 3.0 = 3.0/s (timer se vide 3x plus vite)
- Base Time Added = 1.5s
- Time Added au palier 5 = 1.5 × 0.5 = 0.75s (on gagne 2x moins de temps)
- **Résultat** : Il faut tuer beaucoup plus vite pour maintenir le combo !

### Configuration Hardcore
```
Palier  Combo  Multi   Decay   TimeAdded
0       0      x1.0    x1.0    x1.0
1       3      x1.5    x1.5    x0.85
2       7      x2.0    x2.0    x0.7
3       15     x3.0    x3.0    x0.55
4       30     x5.0    x4.0    x0.4
5       60     x10.0   x5.0    x0.25
```

### Configuration Casual
```
Palier  Combo  Multi   Decay   TimeAdded
0       0      x1.0    x1.0    x1.0
1       10     x1.5    x1.1    x0.95
2       20     x2.0    x1.2    x0.9
3       40     x3.0    x1.4    x0.85
4       80     x5.0    x1.6    x0.8
5       150    x10.0   x1.8    x0.75
```

## 💡 Conseils de Game Design

### Progression Exponentielle
Plus le multiplicateur est élevé, plus la difficulté doit augmenter de façon exponentielle.

**Formule recommandée :**
- Decay Rate = 1 + (palier × 0.4)
- Time Added = 1 - (palier × 0.1)

### Équilibrage
1. **Testez avec des joueurs** : Observez où ils cassent le combo
2. **Ajustez progressivement** : Petits changements de 0.1-0.2
3. **Le palier 3-4 est critique** : C'est là que ça devient vraiment difficile
4. **Le dernier palier doit être quasi-impossible** : Seuls les meilleurs joueurs doivent l'atteindre

### Mode Arcade vs Modern
- **Arcade** : Pour les joueurs hardcore, récompense la perfection
- **Modern** : Pour un public plus large, permet de "sentir" les hauts paliers sans frustration

## 🎮 Intégration dans le jeu

1. Créez un GameObject "ArcadeScoreSystem"
2. Ajoutez le script ArcadeScoreSystem
3. Configurez les paramètres selon votre game design
4. Testez et itérez !

## 📈 Métriques à surveiller

- **Combo moyen atteint** : La plupart des joueurs doivent atteindre le palier 2-3
- **Combo max atteint** : Les bons joueurs doivent atteindre le palier 4-5
- **Durée moyenne d'un combo** : Environ 30-60 secondes pour les bons joueurs
- **Taux de rupture de combo** : Ne doit pas être frustrant (< 50% des tentatives)
