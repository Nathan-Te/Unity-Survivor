using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Form")]
public class SpellForm : ScriptableObject
{
    [Header("Identité")]
    public string formName;
    public string description;
    public Sprite icon; // Pour l'UI plus tard
    public GameObject prefab; // Le visuel (Boule, Météore, etc.)

    [Header("Compatibilité")]
    public SpellTag tags; // Ex: Projectile | SupportsPierce

    [Header("Pattern de Tir")]
    public float baseCooldown = 1f;
    public int baseCount = 1;     // Nb projectiles (1 pour Bolt, 8 pour Nova)
    public float baseSpread = 0f; // Dispersion en degrés (360 pour Nova)

    [Header("Spécifique Mouvement")]
    public float baseSpeed = 20f; // Pour Bolt/Orbit
    public float impactDelay = 0f; // Pour Smite (temps de chute)
    public float baseDuration = 5f; // Pour Orbit (temps avant disparition)

    [Header("Équilibrage")]
    [Range(0f, 1f)] public float procCoefficient = 1.0f; // 1.0 pour Bolt, 0.2 pour Nova
}