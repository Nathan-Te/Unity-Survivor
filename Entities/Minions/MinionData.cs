using UnityEngine;

/// <summary>
/// ScriptableObject defining ghost minion stats and behavior.
/// Ghost minions fly in straight lines toward enemies and explode on contact.
/// </summary>
[CreateAssetMenu(fileName = "New Ghost Minion", menuName = "Survivor/Ghost Minion Data")]
public class MinionData : ScriptableObject
{
    [Header("Minion Info")]
    public string minionName = "Necrotic Ghost";

    [Header("Movement")]
    [Tooltip("Base movement speed (can be upgraded via FlatMinionSpeed)")]
    public float baseMoveSpeed = 5f;

    [Header("Explosion")]
    [Tooltip("Base explosion radius (can be upgraded via FlatMinionExplosionRadius)")]
    public float baseExplosionRadius = 3f;
    [Tooltip("Base explosion damage (can be upgraded via FlatMinionDamage)")]
    public float baseExplosionDamage = 15f;
    [Tooltip("VFX to spawn on explosion")]
    public GameObject explosionVfxPrefab;

    [Header("Lifetime")]
    [Tooltip("Lifetime in seconds before ghost despawns (5 = recommended)")]
    public float duration = 5f;

    [Header("Visual")]
    public GameObject prefab;
}
