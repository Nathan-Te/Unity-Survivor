using UnityEngine;

public enum ElementType { Physical, Fire, Ice, Lightning, Necrotic }

[CreateAssetMenu(menuName = "Spells/Effect")]
public class SpellEffect : ScriptableObject
{
    [Header("Identité")]
    public string effectName;
    public ElementType element;
    public Color tintColor = Color.white;

    [Header("Impact")]
    public float baseDamage = 10f;
    public float damageMultiplier = 1.0f;
    public float knockbackForce = 0f;

    [Header("Status / Spécial")]
    public bool applyBurn;
    public bool applySlow;

    [Header("Foudre (Chain)")]
    public int baseChainCount = 0; // Nombre de rebonds
    public float chainRange = 8f;  // Distance de recherche du rebond
    public float chainDamageReduction = 0.7f; // Dégâts réduits à chaque rebond (x0.7)

    [Header("Nécrotique (Minions)")]
    [Range(0f, 1f)] public float minionSpawnChance = 0f; // 0.0 à 1.0 (100%)
    public GameObject minionPrefab; // Le squelette allié à faire spawn

    [Header("Zone")]
    public float aoeRadius = 0f;
}