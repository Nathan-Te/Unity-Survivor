using UnityEngine;

public enum Rarity { Common, Rare, Epic, Legendary }

public static class RarityUtils
{
    // NOUVEAU : La rareté définit le saut de niveau
    public static int GetLevelBoost(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return 1;
            case Rarity.Rare: return 2;
            case Rarity.Epic: return 3;
            case Rarity.Legendary: return 5; // Jackpot !
            default: return 1;
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
        float roll = Random.value;
        if (roll < 0.60f) return Rarity.Common;
        if (roll < 0.85f) return Rarity.Rare;
        if (roll < 0.95f) return Rarity.Epic;
        return Rarity.Legendary;
    }
}