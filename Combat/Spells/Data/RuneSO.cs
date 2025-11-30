using UnityEngine;

public enum RuneType { Form, Effect, Modifier }

public abstract class RuneSO : ScriptableObject
{
    [Header("Rune Info")]
    public string runeName;
    [TextArea] public string description;
    public Sprite icon;

    public abstract RuneType Type { get; }

    // NOUVEAU : On passe la rune actuelle (null si c'est un nouveau sort)
    // pour pouvoir comparer "Anciennes Stats -> Nouvelles Stats"
    public abstract string GetDescription(Rune currentRune, Rarity rarity);

    // Helper pour formater le texte "Val1 -> Val2" en couleur
    protected string FormatStat(string label, float current, float next, string suffix = "")
    {
        // Si pas de changement, on affiche juste la valeur
        if (Mathf.Abs(current - next) < 0.01f) return $"{label}: {current}{suffix}";

        // Sinon : "Dégâts: 10 -> 15" (avec 15 en vert)
        return $"{label}: {current}{suffix} <color=#00FF00>-> {next}{suffix}</color>";
    }

    protected string FormatIntStat(string label, int current, int next)
    {
        if (current == next) return $"{label}: {current}";
        return $"{label}: {current} <color=#00FF00>-> {next}</color>";
    }
}