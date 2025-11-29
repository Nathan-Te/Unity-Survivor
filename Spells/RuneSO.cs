using UnityEngine;

public enum RuneType { Form, Effect, Modifier }

public abstract class RuneSO : ScriptableObject
{
    [Header("Rune Info")]
    public string runeName;
    [TextArea] public string description;
    public Sprite icon;

    // Pour savoir où la placer dans l'UI
    public abstract RuneType Type { get; }

    // Pour l'affichage dans le menu d'upgrade
    public abstract string GetLevelUpDescription(int nextLevel);
}