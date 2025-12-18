using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main spell manager - coordinates inventory and casting.
/// Delegates inventory management to SpellInventory and casting to SpellCaster.
/// </summary>
public class SpellManager : MonoBehaviour
{
    public static SpellManager Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private int maxSpellSlots = 4;

    [Header("Inventaire Actif")]
    [SerializeField] private List<SpellSlot> activeSlots = new List<SpellSlot>();

    [Header("Defaults")]
    [SerializeField] private SpellEffect defaultEffectAsset;

    // Sub-components
    private SpellInventory _inventory;
    private SpellCaster _caster;

    // Public accessors
    public int MaxSlots => _inventory != null ? _inventory.MaxSlots : maxSpellSlots;
    public event Action OnInventoryUpdated
    {
        add { if (_inventory != null) _inventory.OnInventoryUpdated += value; }
        remove { if (_inventory != null) _inventory.OnInventoryUpdated -= value; }
    }

    private void Awake()
    {
        Instance = this;

        // Get or add sub-components
        _inventory = GetComponent<SpellInventory>();
        if (_inventory == null)
            _inventory = gameObject.AddComponent<SpellInventory>();

        _caster = GetComponent<SpellCaster>();
        if (_caster == null)
            _caster = gameObject.AddComponent<SpellCaster>();

        // Initialize inventory with our SerializeField references
        _inventory.Initialize(maxSpellSlots, activeSlots, defaultEffectAsset);
    }

    private void Start()
    {
        _inventory.InitializeSlots();
    }

    // === INVENTORY DELEGATION ===

    public bool CanAddSpell() => _inventory != null && _inventory.CanAddSpell();
    public List<SpellSlot> GetSlots() => _inventory != null ? _inventory.GetSlots() : new List<SpellSlot>();
    public bool HasForm(SpellForm form) => _inventory != null && _inventory.HasForm(form);

    public void AddNewSpellWithUpgrade(SpellForm form, RuneDefinition upgradeDef)
    {
        if (_inventory != null) _inventory.AddNewSpellWithUpgrade(form, upgradeDef);
    }

    public void UpgradeExistingForm(SpellForm form, RuneDefinition upgradeDef)
    {
        if (_inventory != null) _inventory.UpgradeExistingForm(form, upgradeDef);
    }

    public void ReplaceSpell(SpellForm newForm, int slotIndex, RuneDefinition upgradeDef)
    {
        if (_inventory != null) _inventory.ReplaceSpell(newForm, slotIndex, upgradeDef);
    }

    public bool ApplyEffectToSlot(SpellEffect effectSO, int slotIndex, RuneDefinition upgradeDef)
    {
        if (_inventory != null)
            return _inventory.ApplyEffectToSlot(effectSO, slotIndex, upgradeDef);
        return false;
    }

    public bool TryApplyModifierToSlot(SpellModifier mod, int slotIndex, int replaceIndex, RuneDefinition upgradeDef)
    {
        return _inventory != null && _inventory.TryApplyModifierToSlot(mod, slotIndex, replaceIndex, upgradeDef);
    }

    public void UpgradeSpellAtSlot(int slotIndex, RuneDefinition upgradeDef)
    {
        if (_inventory != null) _inventory.UpgradeSpellAtSlot(slotIndex, upgradeDef);
    }

    /// <summary>
    /// Recalculates all spell stats (used when global player stats change)
    /// </summary>
    public void RecalculateAllSpells()
    {
        if (_inventory != null) _inventory.RecalculateAllSlots();
    }
}
