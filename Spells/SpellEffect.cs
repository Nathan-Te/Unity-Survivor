using UnityEngine;

public enum ElementType { Physical, Fire, Ice, Lightning, Necrotic }

[CreateAssetMenu(menuName = "Spells/Effect")]
public class SpellEffect : ScriptableObject
{
    [Header("Identité")]
    public string effectName;
    public ElementType element;
    public Color tintColor = Color.white; // Pour colorer le prefab

    [Header("Impact")]
    public float baseDamage = 10f;
    public float damageMultiplier = 1.0f; // Physique = 1.2, Glace = 0.8
    public float knockbackForce = 0f;     // Physique = 5, autres = 0

    [Header("Status / Spécial")]
    public bool applyBurn;
    public bool applySlow;
    public bool chainLightning;
    public bool spawnMinionOnDeath;

    // Area of Effect (Explosion à l'impact)
    public float aoeRadius = 0f;
}