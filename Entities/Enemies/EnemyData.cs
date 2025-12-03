using UnityEngine;

public enum EnemyType { Melee, Ranged, Charger }

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "SO/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identité")]
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

    [Header("Comportement")]
    public float stopDistance = 0f; // S'arrête à X mètres (pour les tireurs)
    public float fleeDistance = 0f; // Fuit si le joueur est à moins de X mètres

    [Header("Attaque (Pour Tireurs)")]
    public GameObject projectilePrefab; // Si null, c'est du corps à corps
    public float attackCooldown = 2f;

    [Header("Physique")]
    public float mass = 1f;
    public float pushPower = 2f;
}