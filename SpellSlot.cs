using UnityEngine;

[System.Serializable] // Pour voir et éditer la liste dans l'inspecteur
public class SpellSlot
{
    public SpellData spellData;

    // État interne (caché dans l'inspecteur)
    [HideInInspector] public float currentCooldown;

    // Constructeur utile pour ajouter des sorts dynamiquement plus tard (Level Up)
    public SpellSlot(SpellData data)
    {
        spellData = data;
        currentCooldown = 0f;
    }
}