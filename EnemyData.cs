using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "SO/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identité")]
    public string enemyName = "Squelette";
    public GameObject prefab; // Utile pour le spawner plus tard

    [Header("Stats de Base")]
    public float baseHp = 30f;
    public float baseDamage = 10f; // Dégâts par seconde ou par coup
    public float baseSpeed = 4f;

    [Header("Physique")]
    public float mass = 1f;
    public float pushPower = 2f; // Force avec laquelle il pousse les autres
}