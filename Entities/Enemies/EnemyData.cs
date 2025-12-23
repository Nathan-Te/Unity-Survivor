using UnityEngine;

public enum EnemyType { Melee, Ranged, Charger }

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "SO/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identit�")]
    public string enemyName = "Squelette";
    public GameObject prefab;
    public EnemyType type = EnemyType.Melee;

    [Header("Type")]
    public bool isBoss = false;

    [Header("Stats de Base")]
    public float baseHp = 30f;
    public float baseDamage = 10f;
    public float baseSpeed = 4f;
    public int xpDropAmount = 10;
    public int scoreValue = 10; // Valeur de score de base pour le système arcade

    [Header("Comportement")]
    public float stopDistance = 0f; // S'arr�te � X m�tres (pour les tireurs)
    public float fleeDistance = 0f; // Fuit si le joueur est � moins de X m�tres

    [Header("Attaque (Pour Tireurs)")]
    public GameObject projectilePrefab; // Si null, c'est du corps � corps
    public float attackCooldown = 2f;

    [Header("Physique")]
    public float mass = 1f;
    public float pushPower = 2f;

    [Header("VFX de Mort")]
    [Tooltip("VFX instantané à la mort de l'ennemi (optionnel)")]
    public GameObject deathVfxPrefab;
    [Tooltip("Durée du VFX en secondes")]
    public float deathVfxDuration = 1.5f;
    [Tooltip("Multiplicateur de scale basé sur la taille de l'ennemi (0 = pas de scale, 1 = scale exact)")]
    [Range(0f, 2f)]
    public float vfxScaleMultiplier = 1f;
    [Tooltip("Scale de base du VFX (appliqué avant le multiplicateur)")]
    public float vfxBaseScale = 1f;
}