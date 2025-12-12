using UnityEngine;

// Enum simplifié pour l'inspecteur
public enum RewardFilter { Any, Form, Effect, Modifier, Stat }

public class RuneAltar : ZonePOI
{
    [Header("Reward Configuration")]
    [SerializeField] private Rarity minRarity = Rarity.Common;
    [SerializeField] private RewardFilter specificType = RewardFilter.Any;

    protected override void GrantReward()
    {
        var ui = FindFirstObjectByType<LevelUpUI>();
        if (ui != null)
        {
            Debug.Log($"Rune Altar: Opening Draft (Min: {minRarity}, Filter: {specificType})");

            // On appelle la nouvelle méthode de l'UI avec nos filtres
            ui.ShowRewardDraft(minRarity, specificType);
        }
    }
}