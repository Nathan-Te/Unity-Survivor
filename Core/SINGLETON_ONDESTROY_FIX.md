# Singleton OnDestroy Fix - Documentation

## Problème Identifié

Lors du déchargement de scène (ex: Game Over → Main Menu), des erreurs se produisaient:

```
[Singleton] An instance of 'ProjectilePool' is needed but none exists in the scene.
[Singleton] An instance of 'EnemyManager' is needed but none exists in the scene.
```

### Cause Racine

Certains scripts accédaient à des **singletons de scène** via `.Instance` dans leur méthode `OnDestroy()`. Durant le déchargement de scène:

1. Unity détruit tous les GameObjects de la scène
2. Les singletons de scène (non-DontDestroyOnLoad) sont détruits
3. D'autres scripts appellent `OnDestroy()` et tentent d'accéder à `SomeManager.Instance`
4. Le getter du Singleton appelle `FindFirstObjectByType<T>()` et log une erreur si non trouvé

## Solution Appliquée

### Pattern Incorrect ❌

```csharp
private void OnDestroy()
{
    // WRONG: Triggers Singleton getter which logs error if instance doesn't exist
    if (EnemyManager.Instance != null)
    {
        EnemyManager.Instance.OnEnemyKilled -= OnEnemyKilled;
    }
}
```

### Pattern Correct ✅

```csharp
private void OnDestroy()
{
    // CORRECT: Direct FindFirstObjectByType doesn't log errors
    var enemyManager = FindFirstObjectByType<EnemyManager>();
    if (enemyManager != null)
    {
        enemyManager.OnEnemyKilled -= OnEnemyKilled;
    }
}
```

## Fichiers Corrigés

### 1. MemoryManager.cs

**Méthodes**: `ClearAllPools()`, `DestroyAllPools()`

**Avant**:
```csharp
if (ProjectilePool.Instance != null)
{
    ProjectilePool.Instance.ClearAll();
}
```

**Après**:
```csharp
var projectilePool = FindFirstObjectByType<ProjectilePool>();
if (projectilePool != null)
{
    projectilePool.ClearAll();
}
```

### 2. ArcadeScoreSystem.cs

**Ligne**: 260-269

**Avant**:
```csharp
protected override void OnDestroy()
{
    if (EnemyManager.Instance != null)
    {
        EnemyManager.Instance.OnEnemyKilledWithScore -= OnEnemyKilled;
    }
    base.OnDestroy();
}
```

**Après**:
```csharp
protected override void OnDestroy()
{
    // Use FindFirstObjectByType to avoid Singleton getter error during scene unload
    var enemyManager = FindFirstObjectByType<EnemyManager>();
    if (enemyManager != null)
    {
        enemyManager.OnEnemyKilledWithScore -= OnEnemyKilled;
    }
    base.OnDestroy();
}
```

### 3. ZonePOI.cs

**Ligne**: 37-48

**Avant**:
```csharp
private void OnDestroy()
{
    if (chargeByKills && EnemyManager.Instance != null)
    {
        EnemyManager.Instance.OnEnemyDeathPosition -= OnEnemyDeath;
    }
}
```

**Après**:
```csharp
private void OnDestroy()
{
    // Use FindFirstObjectByType to avoid Singleton getter error during scene unload
    if (chargeByKills)
    {
        var enemyManager = FindFirstObjectByType<EnemyManager>();
        if (enemyManager != null)
        {
            enemyManager.OnEnemyDeathPosition -= OnEnemyDeath;
        }
    }
}
```

## Règles à Suivre

### ❌ N'UTILISEZ JAMAIS `.Instance` dans `OnDestroy()`

Si le singleton est de type **scene-based** (pas DontDestroyOnLoad):
- `ProjectilePool`
- `EnemyManager`
- `EnemyPool`
- `DamageTextPool`
- `GemPool`
- `VFXPool`
- `MinionPool`
- `MinionManager`
- `WorldStateManager`
- `MapManager`
- `ArcadeScoreSystem`
- Etc.

### ✅ UTILISEZ `FindFirstObjectByType<T>()`

```csharp
private void OnDestroy()
{
    var manager = FindFirstObjectByType<MySceneBasedManager>();
    if (manager != null)
    {
        // Safe to access
        manager.UnsubscribeEvent();
    }
}
```

### ✅ OU Vérifiez avec Null-Safe Operator (pour DontDestroyOnLoad singletons)

```csharp
private void OnDestroy()
{
    // Safe for DontDestroyOnLoad singletons (GameStateController, ProgressionManager, etc.)
    if (GameStateController.Instance != null)
    {
        GameStateController.Instance.OnStateChanged -= HandleStateChanged;
    }
}
```

**Mais attention**: Le getter va quand même logger une erreur si l'instance n'existe pas. Préférez `FindFirstObjectByType` partout pour éviter les logs.

## Différence entre Types de Singletons

### Scene-Based Singletons
- Détruits lors du déchargement de scène
- DOIVENT utiliser `FindFirstObjectByType` dans `OnDestroy()`
- Exemples: `EnemyManager`, `ProjectilePool`, `ArcadeScoreSystem`

### DontDestroyOnLoad Singletons
- Persistent entre les scènes
- Peuvent utiliser `.Instance` dans `OnDestroy()` mais préférez `FindFirstObjectByType`
- Exemples: `ProgressionManager`, `GameStateController`, `MemoryManager`

## Vérification du Code

Pour trouver tous les usages potentiellement problématiques:

```bash
# Rechercher .Instance dans OnDestroy
grep -r "\.Instance" --include="*.cs" | grep -i "ondestroy" -A 5
```

## Résumé des Corrections

| Fichier | Ligne | Singleton Accédé | Status |
|---------|-------|------------------|--------|
| `MemoryManager.cs` | 95-126 | ProjectilePool, GemPool, EnemyPool, DamageTextPool | ✅ Corrigé |
| `MemoryManager.cs` | 129-153 | ProjectilePool, GemPool, EnemyPool, DamageTextPool | ✅ Corrigé |
| `ArcadeScoreSystem.cs` | 260-269 | EnemyManager | ✅ Corrigé |
| `ZonePOI.cs` | 37-48 | EnemyManager | ✅ Corrigé |

## Test de Validation

1. Lancez une partie
2. Jouez jusqu'à la mort (Game Over)
3. Cliquez sur "Main Menu"
4. **Vérifiez**: Aucune erreur de type `[Singleton] An instance of 'X' is needed` ne doit apparaître
5. **Vérifiez**: La console affiche uniquement les logs normaux de nettoyage

## Conclusion

Ces corrections garantissent un déchargement de scène propre sans erreurs de Singleton. Le pattern `FindFirstObjectByType` est la méthode recommandée pour accéder aux singletons dans `OnDestroy()` car elle ne génère pas d'erreur si l'objet n'existe pas.
