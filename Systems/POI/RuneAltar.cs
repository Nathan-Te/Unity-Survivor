using UnityEngine;

public class RuneAltar : ZonePOI
{
    [Header("Reward")]
    [SerializeField] private Rarity rewardRarity = Rarity.Rare;

    protected override void GrantReward()
    {
        // On appelle l'UI de Level Up mais en mode "Reward Chest"
        // Cela demande d'adapter LevelUpUI pour accepter un trigger externe.

        // Solution simple temporaire : Donner mass XP pour forcer un level up
        // LevelManager.Instance.AddExperience(1000); 

        // Solution propre : Appeler une méthode spéciale (à créer)
        var ui = FindFirstObjectByType<LevelUpUI>();
        if (ui != null)
        {
            // Hack pour l'instant : On déclenche un level up visuel
            // Idéalement: ui.OpenRewardMenu(rewardRarity);
            Debug.Log("Rune Altar Completed! (Connecter l'UI ici)");
            LevelManager.Instance.AddExperience(LevelManager.Instance.experienceToNextLevel); // Force Level Up
        }
    }
}