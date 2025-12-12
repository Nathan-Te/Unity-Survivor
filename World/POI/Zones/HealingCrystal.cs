using UnityEngine;

public class HealingCrystal : DestructiblePOI
{
    [Header("Reward")]
    [SerializeField] private float healAmount = 50f;

    protected override void GrantReward()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.Heal(healAmount);
        }
    }
}