using System.Collections.Generic;
using UnityEngine;

public class LevelUpUI : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private GameObject panel; // Le panneau global (à activer/désactiver)
    [SerializeField] private Transform cardsContainer; // Là où on spawn les cartes (Horizontal Layout)
    [SerializeField] private GameObject cardPrefab; // Le prefab avec le script UpgradeCard

    [Header("Contenu (Database temporaire)")]
    // Plus tard, on chargera ça dynamiquement via Resources ou un LootTable
    [SerializeField] private List<SpellForm> availableSpells;
    [SerializeField] private List<SpellModifier> availableModifiers;

    private void Start()
    {
        // On s'abonne à l'événement de Level Up
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelUp.AddListener(ShowLevelUp);
        }

        panel.SetActive(false);
    }

    public void ShowLevelUp()
    {
        // 1. Pause le jeu
        Time.timeScale = 0f;
        panel.SetActive(true);

        // 2. Nettoyer les anciennes cartes
        foreach (Transform child in cardsContainer)
        {
            Destroy(child.gameObject);
        }

        // 3. Générer 3 options aléatoires
        List<UpgradeData> options = GenerateOptions(3);

        // 4. Créer les cartes
        foreach (var option in options)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardsContainer);
            if (cardObj.TryGetComponent<UpgradeCard>(out var card))
            {
                card.Initialize(option, this);
            }
        }
    }

    public void SelectUpgrade(UpgradeData upgrade)
    {
        // Application de l'upgrade
        ApplyUpgrade(upgrade);

        // Reprise du jeu
        panel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void ApplyUpgrade(UpgradeData upgrade)
    {
        var spellManager = FindFirstObjectByType<SpellManager>();

        if (upgrade.Type == UpgradeType.NewSpell)
        {
            spellManager.AddSpell(upgrade.TargetForm);
        }
        else if (upgrade.Type == UpgradeType.Modifier)
        {
            spellManager.AddModifier(upgrade.TargetModifier);
        }
    }

    private List<UpgradeData> GenerateOptions(int count)
    {
        List<UpgradeData> picks = new List<UpgradeData>();

        for (int i = 0; i < count; i++)
        {
            // 50% chance Sort, 50% chance Modif (si dispo)
            bool pickSpell = Random.value > 0.5f;

            if (pickSpell && availableSpells.Count > 0)
            {
                var randomForm = availableSpells[Random.Range(0, availableSpells.Count)];
                picks.Add(new UpgradeData(randomForm));
            }
            else if (availableModifiers.Count > 0)
            {
                var randomMod = availableModifiers[Random.Range(0, availableModifiers.Count)];
                picks.Add(new UpgradeData(randomMod));
            }
        }
        return picks;
    }
}