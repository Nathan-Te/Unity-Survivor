using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Modifier")]
public class SpellModifier : ScriptableObject
{
    [Header("Identité")]
    public string modifierName;
    [TextArea] public string description;

    [Header("Restriction")]
    public SpellTag requiredTag; // Si "None", s'applique à tout

    [Header("Multiplicateurs de Stats")]
    public float damageMult = 1f;
    public float cooldownMult = 1f; // Frenzy = 0.8
    public float sizeMult = 1f;     // Giant = 1.5
    public float speedMult = 1f;
    public float durationMult = 1f;

    [Header("Additions")]
    public int addCount = 0;   // Multicast +1
    public int addPierce = 0;  // Pierce +1
    public bool enableHoming;  // Homing
}