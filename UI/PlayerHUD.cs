using System.Collections.Generic;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private GameObject slotUIPrefab;

    private void Start()
    {
        SpellManager sm = FindFirstObjectByType<SpellManager>();
        if (sm != null)
        {
            sm.OnInventoryUpdated += RefreshUI;
            RefreshUI(); // Premier appel
        }
    }

    private void RefreshUI()
    {
        // Nettoyage
        foreach (Transform child in slotsContainer) Destroy(child.gameObject);

        SpellManager sm = FindFirstObjectByType<SpellManager>();
        if (sm == null) return;

        List<SpellSlot> slots = sm.GetSlots();
        for (int i = 0; i < slots.Count; i++)
        {
            GameObject obj = Instantiate(slotUIPrefab, slotsContainer);
            if (obj.TryGetComponent<SpellSlotUI>(out var ui))
            {
                // Pas de manager passé ici, donc non-cliquable (juste affichage)
                ui.Initialize(slots[i], i, null);
            }
        }
    }
}