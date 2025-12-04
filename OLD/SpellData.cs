using UnityEngine;

[CreateAssetMenu(fileName = "NewSpell", menuName = "SO/Spell Data")]
public class SpellData : ScriptableObject
{
    [Header("Identité")]
    public string spellName;
    public GameObject projectilePrefab; // Le visuel (boule, flèche...)

    [Header("Ciblage")]
    public TargetingMode targetingMode = TargetingMode.Nearest; // Par défaut
    public bool requiresLineOfSight = true; // Est-ce qu'on tire à travers les murs ? (ex: Tir Spectral = false)

    [Header("Stats de Combat")]
    public float damage = 10f;
    public float speed = 20f;
    public float cooldown = 0.5f;
    public float range = 20f;   // Distance max avant disparition
    public float size = 1f;     // Echelle du projectile
    public int pierceCount = 0; // Combien d'ennemis ça traverse (0 = détruit au 1er impact)

    [Header("Zone (Si Targeting Mode = Density)")]
    public float explosionRadius = 0f; // Sert à détecter la densité du groupe
}