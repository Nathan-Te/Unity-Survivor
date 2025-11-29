using UnityEngine;

public enum Rarity { Common, Rare, Epic, Legendary }

public static class RarityUtils
{
    public static float GetMultiplier(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return 1.0f;
            case Rarity.Rare: return 1.25f;      // +25% stats
            case Rarity.Epic: return 1.5f;       // +50% stats
            case Rarity.Legendary: return 2.0f;  // +100% stats (Double)
            default: return 1.0f;
        }
    }

    public static Color GetColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return Color.white;
            case Rarity.Rare: return new Color(0.3f, 0.6f, 1f); // Bleu
            case Rarity.Epic: return new Color(0.6f, 0.2f, 1f); // Violet
            case Rarity.Legendary: return new Color(1f, 0.6f, 0f); // Orange/Or
            default: return Color.white;
        }
    }

    // Pour générer une rareté aléatoire (Pondérée)
    public static Rarity GetRandomRarity()
    {
        float roll = Random.value; // 0.0 à 1.0

        if (roll < 0.60f) return Rarity.Common;    // 60%
        if (roll < 0.85f) return Rarity.Rare;      // 25%
        if (roll < 0.95f) return Rarity.Epic;      // 10%
        return Rarity.Legendary;                   // 5%
    }
}