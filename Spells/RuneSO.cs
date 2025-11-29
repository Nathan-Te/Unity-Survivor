using UnityEngine;

public enum RuneType { Form, Effect, Modifier }

public abstract class RuneSO : ScriptableObject
{
    [Header("Rune Info")]
    public string runeName;
    [TextArea] public string description;
    public Sprite icon;

    public abstract RuneType Type { get; }

    // MODIFICATION : On ne prend plus que le niveau cible
    public abstract string GetLevelUpDescription(int level);
}