# FormEffectPrefabMapping Editor - Audio Guide

Guide complet pour utiliser l'éditeur custom du FormEffectPrefabMapping avec les nouvelles fonctionnalités audio.

## 🎯 Vue d'ensemble

L'éditeur custom de `FormEffectPrefabMapping` a été mis à jour pour faciliter la gestion de l'audio directement dans l'Inspector.

### ✨ Nouvelles Fonctionnalités Audio

✅ **Section Audio** - Visible pour chaque entrée (Form + Effect)
✅ **Sliders de Volume** - Ajustez le volume de 0-100% directement
✅ **Boutons Preview (▶)** - Écoutez les sons dans l'éditeur
✅ **Statistiques Audio** - Comptage automatique des entrées avec sons
✅ **Recherche Audio** - Filtrez par nom de clip audio
✅ **Résumé Visuel** - Icônes et infos audio dans le résumé

## 📋 Interface de l'Éditeur

### Header Section
```
┌─────────────────────────────────────────┐
│ Form-Effect Prefab Mapping              │
│ ℹ️  Définissez les combinaisons...      │
├─────────────────────────────────────────┤
│ Recherche: [_________] [✕] [Matrice]   │
│ Aller à l'index: [__] [Aller]          │
├─────────────────────────────────────────┤
│ Statistiques                            │
│ Total: 25 | Valides: 23 | Invalides: 2 │
│ 🔊 Audio: 18 | Cast: 15 | Impact: 16   │
└─────────────────────────────────────────┘
```

### Entry Card (avec Audio)
```
┌─────────────────────────────────────────┐
│ #0                                   [X]│
│ Form:       [LinearForm         ▼]     │
│ Effect:     [FireEffect         ▼]     │
│ Prefab:     [FireBoltPrefab     ▼]     │
│ Impact VFX: [FireExplosion      ▼]     │
│                                         │
│ 🔊 Audio Settings                       │
│ Cast:   [fire_cast.wav      ▼] Vol: [━━━━━━━━━━] 80% [▶] │
│ Impact: [fire_impact.wav    ▼] Vol: [━━━━━━━━━━] 100% [▶]│
│ ♪ Cast (80%) | Impact (100%)           │
│                                         │
│ ✓ LinearForm + FireEffect → FireBolt   │
│   | VFX: FireExplosion | 🔊 Audio: Cast+Impact │
└─────────────────────────────────────────┘
```

## 🎵 Utilisation de la Section Audio

### 1. Assigner un Son de Cast

**Étapes** :
1. Cliquez sur le champ "Cast:"
2. Sélectionnez un AudioClip dans votre projet
3. Le slider de volume apparaît automatiquement
4. Ajustez le volume (0-100%)
5. Cliquez sur [▶] pour prévisualiser

**Exemple** :
```
Cast: fire_woosh.wav
Volume: 80%
```

### 2. Assigner un Son d'Impact

**Étapes** :
1. Cliquez sur le champ "Impact:"
2. Sélectionnez un AudioClip
3. Ajustez le volume avec le slider
4. Testez avec le bouton [▶]

**Exemple** :
```
Impact: fire_explosion.wav
Volume: 100%
```

### 3. Prévisualiser les Sons

Le bouton **[▶]** permet d'écouter le son dans l'éditeur Unity :
- ✅ Arrête automatiquement le son précédent
- ✅ Affiche le nom du clip dans la console
- ✅ Indique le volume in-game (le preview Unity ne supporte pas le volume custom)

**Console Output** :
```
[Audio Preview] Playing: fire_cast.wav (Volume in-game: 80%)
```

## 🔍 Recherche et Filtrage

### Rechercher par Nom de Son

La barre de recherche supporte maintenant les noms d'AudioClips :

**Exemples** :
- `fire` → Trouve toutes les entrées avec des sons contenant "fire"
- `explosion` → Trouve les entrées avec "explosion" dans leurs sons
- `cast` → Trouve les entrées qui ont des sons de cast nommés "cast"

### Filtres Supportés

La recherche fonctionne sur :
- ✅ Form name
- ✅ Effect name
- ✅ Prefab name
- ✅ VFX name
- ✅ **Cast Sound name** ⭐
- ✅ **Impact Sound name** ⭐
- ✅ Index number

## 📊 Statistiques Audio

La section "Statistiques" affiche maintenant :

```
Total: 25 | Valides: 23 | Invalides: 2
🔊 Audio: 18 entrées | Cast: 15 | Impact: 16
```

**Interprétation** :
- **Audio: 18** → 18 entrées ont au moins un son (cast ou impact)
- **Cast: 15** → 15 entrées ont un son de cast assigné
- **Impact: 16** → 16 entrées ont un son d'impact assigné

## 💡 Workflows Recommandés

### Workflow 1 : Configuration Basique

Pour un nouveau sort (Linear + Fire) :

1. **Créer l'entrée**
   - Form: LinearForm
   - Effect: FireEffect
   - Prefab: FireBoltPrefab
   - VFX: FireExplosionVFX

2. **Ajouter l'audio**
   - Cast: `fire_cast.wav` (80%)
   - Impact: `fire_explosion.wav` (100%)

3. **Tester**
   - Cliquez [▶] sur chaque son
   - Vérifiez qu'ils correspondent au spell

4. **Valider**
   - Le résumé affiche : `🔊 Audio: Cast+Impact`

### Workflow 2 : Ajustement de Volume en Masse

Pour équilibrer tous les sorts Fire :

1. **Rechercher** : `FireEffect`
2. Pour chaque résultat :
   - Ajuster `castVolume` à 0.7 (70%)
   - Ajuster `impactVolume` à 0.9 (90%)
3. Sauvegarder (Ctrl+S)

### Workflow 3 : Vérification Audio

Pour s'assurer que tous les sorts ont de l'audio :

1. Regarder les **Statistiques** :
   - Si `Audio < Total`, il manque des sons
2. Chercher les entrées sans audio :
   - Scrollez la liste
   - Les entrées sans son affichent : `💡 Assignez des sons...`
3. Assignez les sons manquants

## 🎨 Indicateurs Visuels

### Couleurs d'Entrée

| Couleur | Signification |
|---------|---------------|
| **Vert clair** | Entrée valide (Form + Effect + Prefab OK) |
| **Rouge clair** | Entrée invalide (champs manquants) |
| **Jaune** | Entrée sélectionnée (via "Aller à") |

### Icônes et Symboles

| Symbole | Signification |
|---------|---------------|
| 🔊 | Section Audio Settings |
| ▶ | Bouton Preview (play sound) |
| ♪ | Résumé des sons assignés |
| 💡 | Info : Assignez des sons |
| ✓ | Entrée valide et complète |
| ⚠ | Warning : Entrée incomplète |

### Messages de Résumé

**Aucun son** :
```
💡 Assignez des sons pour activer l'audio de ce sort
```

**Cast uniquement** :
```
♪ Cast (80%)
```

**Impact uniquement** :
```
♪ Impact (100%)
```

**Les deux** :
```
♪ Cast (80%) | Impact (100%)
```

**Dans le résumé final** :
```
✓ LinearForm + FireEffect → FireBolt | VFX: FireExplosion | 🔊 Audio: Cast+Impact
```

## 🐛 Troubleshooting

### Le bouton [▶] ne joue pas le son

**Causes possibles** :
1. AudioClip non assigné correctement
2. Fichier audio corrompu
3. Unity en mode Play (arrêtez le play mode)

**Solution** :
- Vérifiez que l'AudioClip est bien assigné
- Testez le clip dans l'Inspector Unity normal
- Redémarrez Unity si nécessaire

### Le volume ne change pas dans le preview

C'est **normal** ! Le système de preview Unity ne supporte pas le volume custom. Le volume défini sera appliqué **en jeu uniquement**.

Le message dans la console indique le volume qui sera utilisé :
```
[Audio Preview] Playing: fire_cast.wav (Volume in-game: 80%)
```

### Les statistiques audio ne se mettent pas à jour

**Solution** :
- Fermez et réouvrez l'Inspector
- Ou modifiez n'importe quel champ pour forcer un refresh

## 💾 Raccourcis Clavier

| Raccourci | Action |
|-----------|--------|
| **Ctrl+S** | Sauvegarder les modifications |
| **Ctrl+D** | Dupliquer l'entrée sélectionnée (dans Inspector) |
| **Delete** | Supprimer l'entrée sélectionnée (dans Inspector) |

## 📚 Bonnes Pratiques

### Nommage des Sons

✅ **Bon** :
```
fire_cast.wav
fire_impact.wav
ice_cast.wav
ice_shatter.wav
```

❌ **Mauvais** :
```
sound1.wav
audio_file.wav
new_sound_final_v2.wav
```

### Organisation des Volumes

**Recommandations** :
- **Cast sounds** : 70-90% (moins fort que l'impact)
- **Impact sounds** : 90-100% (effet dramatique)
- **Sorts furtifs** : 50-70% (discrets)
- **Sorts puissants** : 90-100% (imposants)

### Workflow de Test

1. **Éditeur** : Preview avec [▶]
2. **Jeu** : Test en Play mode
3. **Ajustement** : Retour à l'éditeur, ajuster volumes
4. **Validation** : Re-test en jeu

## 🎯 Exemples Pratiques

### Fire Bolt (Linear + Fire)
```
Cast Sound:   fire_woosh.wav (80%)
Impact Sound: fire_explosion.wav (100%)
```

### Ice Smite (Smite + Ice)
```
Cast Sound:   ice_summon.wav (90%)
Impact Sound: ice_shatter.wav (85%)
```

### Lightning Nova (Nova + Lightning)
```
Cast Sound:   lightning_burst.wav (100%)
Impact Sound: electric_crackle.wav (90%)
```

### Stealth Arrow (Linear + Physical)
```
Cast Sound:   arrow_whoosh.wav (60%)
Impact Sound: arrow_thud.wav (70%)
```

---

**Dernière mise à jour** : 2025-12-29
**Version** : 2.0 (Audio Integration)
