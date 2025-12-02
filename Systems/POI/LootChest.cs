using UnityEngine;

public class LootChest : DestructiblePOI
{
    [Header("Reward")]
    [SerializeField] private int xpAmount = 500;
    [SerializeField] private int gemCount = 10;

    protected override void GrantReward()
    {
        // Spawn plein de gemmes
        for (int i = 0; i < gemCount; i++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * 2f;
            randomOffset.y = 0.5f;
            GemPool.Instance.Spawn(transform.position + randomOffset, xpAmount / gemCount);
        }

        // TODO: Ajouter une monnaie "Gold" plus tard
    }
}