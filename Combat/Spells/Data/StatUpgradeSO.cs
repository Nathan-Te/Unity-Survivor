using UnityEngine;

[CreateAssetMenu(menuName = "Stats/Stat Upgrade")]
public class StatUpgradeSO : RuneSO
{
    // On considère ça comme un Modifier global
    public override RuneType Type => RuneType.Modifier;

    [Header("Statistique Ciblée")]
    public StatType targetStat;

    // Note : Remplis les listes "Common Upgrades", "Rare Upgrades", etc. dans l'inspecteur.
    // Utilise le champ 'Stats > Stat Value' pour définir le montant (ex: 0.1 pour +10% vitesse).
}