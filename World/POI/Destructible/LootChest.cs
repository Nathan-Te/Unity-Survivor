using UnityEngine;

public class LootChest : DestructiblePOI
{
    [Header("Reward")]
    [SerializeField] private int xpAmount = 500;
    [SerializeField] private int gemCount = 10;
    [SerializeField] private int goldAmount = 50;
    [SerializeField] private int goldCoinCount = 5;

    protected override void GrantReward()
    {
        // Spawn XP gems
        if (GemPool.Instance != null)
        {
            for (int i = 0; i < gemCount; i++)
            {
                Vector3 randomOffset = Random.insideUnitSphere * 2f;
                randomOffset.y = 0.5f;
                GemPool.Instance.Spawn(transform.position + randomOffset, xpAmount / gemCount);
            }
        }

        // Spawn Gold coins
        if (GoldPool.Instance != null && goldAmount > 0)
        {
            for (int i = 0; i < goldCoinCount; i++)
            {
                Vector3 randomOffset = Random.insideUnitSphere * 2f;
                randomOffset.y = 0.5f;
                GoldPool.Instance.Spawn(transform.position + randomOffset, goldAmount / goldCoinCount);
            }
        }
    }
}