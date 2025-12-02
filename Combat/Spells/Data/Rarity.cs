using UnityEngine;

public enum Rarity { Common, Rare, Epic, Legendary }

public static class RarityUtils
{
    public static int GetLevelBoost(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return 1;
            case Rarity.Rare: return 2;
            case Rarity.Epic: return 3;
            case Rarity.Legendary: return 5;
            default: return 1;
        }
    }

    public static float GetPowerBoost(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return 1.0f;
            case Rarity.Rare: return 1.5f;
            case Rarity.Epic: return 2.0f;
            case Rarity.Legendary: return 3.0f;
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
            case Rarity.Legendary: return new Color(1f, 0.6f, 0f); // Or
            default: return Color.white;
        }
    }

    public static Rarity GetRandomRarity()
    {
        return GetRandomRarityAtLeast(Rarity.Common);
    }

    // NOUVEAU : Tirage pondéré avec seuil minimum
    public static Rarity GetRandomRarityAtLeast(Rarity minRarity)
    {
        // Poids arbitraires (Common=60, Rare=25, Epic=10, Leg=5)
        float wCommon = minRarity <= Rarity.Common ? 60f : 0f;
        float wRare = minRarity <= Rarity.Rare ? 25f : 0f;
        float wEpic = minRarity <= Rarity.Epic ? 10f : 0f;
        float wLeg = minRarity <= Rarity.Legendary ? 5f : 0f;

        float totalWeight = wCommon + wRare + wEpic + wLeg;
        float roll = Random.Range(0, totalWeight);

        if (roll < wCommon) return Rarity.Common;
        roll -= wCommon;

        if (roll < wRare) return Rarity.Rare;
        roll -= wRare;

        if (roll < wEpic) return Rarity.Epic;

        return Rarity.Legendary;
    }
}