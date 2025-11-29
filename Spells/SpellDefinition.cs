[System.Serializable]
public class SpellDefinition
{
    public SpellForm Form;
    public SpellEffect Effect;

    // Stats
    public float Damage;
    public float Cooldown;
    public float Speed;
    public float Size;
    public float Range;
    public float Duration;
    public int Count;
    public int Pierce;
    
    // --- AJOUTER CECI ---
    public TargetingMode Mode;
    public bool RequiresLoS;
    // --------------------

    public bool IsHoming;
    public SpellDefinition() { }
}