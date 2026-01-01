# Localisation des Minions

## Clés de Localisation à Ajouter

Si vous souhaitez afficher des informations sur les minions dans l'UI, voici les clés recommandées à ajouter dans `en.json` et `fr.json` :

### en.json
```json
{
  "entries": [
    // ... existing entries ...

    // Minion HUD
    { "key": "HUD_MINIONS", "value": "Minions: {0}/{1}" },
    { "key": "HUD_MINIONS_ACTIVE", "value": "Active Minions: {0}" },

    // Minion Types
    { "key": "MINION_TYPE_MELEE", "value": "Melee" },
    { "key": "MINION_TYPE_RANGED", "value": "Ranged" },

    // Minion Names (exemples)
    { "key": "MINION_SKELETON_WARRIOR", "value": "Skeleton Warrior" },
    { "key": "MINION_SKELETON_ARCHER", "value": "Skeleton Archer" },
    { "key": "MINION_GHOST", "value": "Ghost" },
    { "key": "MINION_WRAITH", "value": "Wraith" },

    // Minion Stats (pour les descriptions d'upgrade)
    { "key": "STAT_MINION_CHANCE", "value": "Minion Spawn Chance" },
    { "key": "STAT_MAX_MINIONS", "value": "Max Minions" },
    { "key": "STAT_MINION_DAMAGE", "value": "Minion Damage" },
    { "key": "STAT_MINION_DURATION", "value": "Minion Duration" },

    // Minion Tooltips
    { "key": "TOOLTIP_MINION_SPAWN", "value": "+{0}% chance to summon a minion on kill" },
    { "key": "TOOLTIP_MAX_MINIONS", "value": "+{0} maximum active minions" }
  ]
}
```

### fr.json
```json
{
  "entries": [
    // ... existing entries ...

    // Minion HUD
    { "key": "HUD_MINIONS", "value": "Minions : {0}/{1}" },
    { "key": "HUD_MINIONS_ACTIVE", "value": "Minions actifs : {0}" },

    // Minion Types
    { "key": "MINION_TYPE_MELEE", "value": "Mêlée" },
    { "key": "MINION_TYPE_RANGED", "value": "À distance" },

    // Minion Names (exemples)
    { "key": "MINION_SKELETON_WARRIOR", "value": "Guerrier Squelette" },
    { "key": "MINION_SKELETON_ARCHER", "value": "Archer Squelette" },
    { "key": "MINION_GHOST", "value": "Fantôme" },
    { "key": "MINION_WRAITH", "value": "Spectre" },

    // Minion Stats (pour les descriptions d'upgrade)
    { "key": "STAT_MINION_CHANCE", "value": "Chance d'invocation" },
    { "key": "STAT_MAX_MINIONS", "value": "Minions max" },
    { "key": "STAT_MINION_DAMAGE", "value": "Dégâts des minions" },
    { "key": "STAT_MINION_DURATION", "value": "Durée des minions" },

    // Minion Tooltips
    { "key": "TOOLTIP_MINION_SPAWN", "value": "+{0}% de chance d'invoquer un minion à la mort" },
    { "key": "TOOLTIP_MAX_MINIONS", "value": "+{0} minions actifs maximum" }
  ]
}
```

## Utilisation dans MinionData

Si vous voulez que le nom du minion soit localisé, modifiez `MinionData.cs` :

```csharp
[Header("Minion Info")]
[Tooltip("JSON key for localized minion name (e.g., 'MINION_SKELETON_WARRIOR')")]
public string minionNameKey = "MINION_SKELETON_WARRIOR";

/// <summary>
/// Gets the localized name of this minion
/// </summary>
public string GetLocalizedName()
{
    if (!string.IsNullOrEmpty(minionNameKey))
    {
        return SimpleLocalizationHelper.Get(minionNameKey, name);
    }
    return name; // Fallback to asset name
}
```

## Utilisation dans l'UI

### Afficher le nombre de minions actifs

```csharp
using SurvivorGame.Localization;
using TMPro;

public class MinionCountUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI minionCountText;

    private void Update()
    {
        if (MinionManager.Instance != null)
        {
            int current = MinionManager.Instance.ActiveMinionCount;
            // Vous devez récupérer maxMinions depuis SpellDefinition ou PlayerStats
            int max = 10; // Exemple

            minionCountText.text = SimpleLocalizationHelper.FormatText("HUD_MINIONS", current, max);
        }
    }
}
```

### Afficher dans les descriptions d'upgrades

Si vous utilisez le système de génération automatique de descriptions (voir `RuneDescriptionGenerator.cs`), ajoutez ces stats :

```csharp
// Dans RuneDescriptionGenerator.cs (hypothétique extension)
public static string GenerateMinionDescription(RuneStats stats)
{
    StringBuilder sb = new StringBuilder();

    if (stats.FlatMinionChance > 0)
    {
        int percentage = Mathf.RoundToInt(stats.FlatMinionChance * 100);
        sb.Append(SimpleLocalizationHelper.FormatText("TOOLTIP_MINION_SPAWN", percentage));
        sb.Append("\n");
    }

    if (stats.FlatMaxMinions > 0)
    {
        sb.Append(SimpleLocalizationHelper.FormatText("TOOLTIP_MAX_MINIONS", stats.FlatMaxMinions));
        sb.Append("\n");
    }

    return sb.ToString();
}
```

## Exemple de Configuration dans RuneDefinition

### Upgrade Common (Necrotic Effect)
```
Description:
- Key: "UPGRADE_NECROTIC_COMMON_1"
- EN: "+10% Minion Spawn Chance\n+1 Max Minions"
- FR: "+10% chance d'invocation\n+1 minion maximum"

Stats:
- FlatMinionChance: 0.1
- FlatMaxMinions: 1
```

### Upgrade Legendary (Necrotic Effect)
```
Description:
- Key: "UPGRADE_NECROTIC_LEGENDARY_1"
- EN: "+25% Minion Spawn Chance\n+3 Max Minions\n+50% Minion Damage"
- FR: "+25% chance d'invocation\n+3 minions maximum\n+50% dégâts des minions"

Stats:
- FlatMinionChance: 0.25
- FlatMaxMinions: 3
- DamageMult: 0.5 (si on veut booster les dégâts du spell qui spawn les minions)
```

## Notes

1. **Les noms de minions dans MinionData ne sont PAS encore localisés** par défaut. C'est une extension optionnelle.
2. **Les descriptions d'upgrade utilisent LocalizedString** (système existant)
3. **Si vous ajoutez un HUD pour afficher les minions**, utilisez les clés ci-dessus
4. **Les clés sont optionnelles** - Le système fonctionne sans localisation (fallback sur noms d'assets)

## Prochaines Étapes (Optionnel)

Si vous voulez une localisation complète des minions :

1. Ajouter les clés JSON ci-dessus dans `en.json` et `fr.json`
2. Modifier `MinionData.cs` pour utiliser `minionNameKey` au lieu de `minionName`
3. Créer un `MinionCountUI` pour afficher le nombre actif de minions
4. Ajouter les stats de minions dans `RuneDescriptionGenerator` (auto-génération)

---

**Note** : Le système de minions fonctionne sans localisation. Cette documentation est pour une extension future.
