using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Form")]
public class SpellForm : ScriptableObject
{
    [Header("Identité")]
    public string formName;
    public string description;
    public GameObject prefab;
    public Sprite icon;

    // --- AJOUTER CECI ---
    [Header("Stratégie de Ciblage")]
    public TargetingMode targetingMode = TargetingMode.Nearest;
    public bool requiresLineOfSight = true;
    // --------------------

    [Header("Compatibilité")]
    public SpellTag tags;

    [Header("Pattern de Tir")]
    public float baseCooldown = 1f;
    public int baseCount = 1;
    public float baseSpread = 0f;

    [Header("Spécifique Mouvement")]
    public float baseSpeed = 20f;
    public float impactDelay = 0f;
    public float baseDuration = 5f;

    [Header("Équilibrage")]
    [Range(0f, 1f)] public float procCoefficient = 1.0f;
}