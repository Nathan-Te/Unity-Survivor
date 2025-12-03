using System;
using System.Collections.Generic;
using UnityEngine;

public class SpellManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private int maxSpellSlots = 4;
    public int MaxSlots => maxSpellSlots;

    [Header("Inventaire Actif")]
    [SerializeField] private List<SpellSlot> activeSlots = new List<SpellSlot>();

    private Transform _playerTransform;
    public event Action OnInventoryUpdated;

    private void Start()
    {
        if (PlayerController.Instance != null)
            _playerTransform = PlayerController.Instance.transform;

        foreach (var slot in activeSlots)
        {
            slot.ForceInit();
            slot.currentCooldown = 0.5f;
        }
        OnInventoryUpdated?.Invoke();
    }

    private void Update()
    {
        if (_playerTransform == null) return;
        for (int i = 0; i < activeSlots.Count; i++) ProcessSlot(activeSlots[i]);
    }

    private void ProcessSlot(SpellSlot slot)
    {
        if (slot.Definition == null) return;

        slot.currentCooldown -= Time.deltaTime;
        if (slot.currentCooldown <= 0f)
        {
            if (AttemptAttack(slot.Definition))
                slot.currentCooldown = slot.Definition.Cooldown;
        }
    }

    // --- LOGIQUE DE TIR RESTAURÉE ---

    private bool AttemptAttack(SpellDefinition def)
    {
        Vector3 targetPos = Vector3.zero;
        bool hasTarget = false;

        if (PlayerController.Instance.IsManualAiming)
        {
            targetPos = PlayerController.Instance.MouseWorldPosition;
            hasTarget = true;
        }
        else
        {
            // On tire depuis la hauteur du torse (+1m) pour éviter le sol
            Vector3 scanOrigin = _playerTransform.position + Vector3.up;

            Transform target = EnemyManager.Instance.GetTarget(
                scanOrigin,
                def.Range,
                def.Mode,
                def.Effect.aoeRadius,
                def.RequiresLoS
            );

            if (target != null)
            {
                targetPos = target.position;
                hasTarget = true;
            }
        }

        if (hasTarget)
        {
            Fire(targetPos, def);
            return true;
        }
        return false;
    }

    private void Fire(Vector3 targetPos, SpellDefinition def)
    {
        Vector3 dirToTarget = (targetPos - _playerTransform.position).normalized;
        dirToTarget.y = 0;

        int count = def.Count;
        float spread = def.Spread;

        bool isFullCircle = Mathf.Abs(spread - 360f) < 0.1f;
        float angleStep = (count > 1) ? (isFullCircle ? spread / count : spread / (count - 1)) : 0;
        float startAngle = count > 1 ? -spread / 2f : 0;
        if (isFullCircle) startAngle = 0f;

        for (int i = 0; i < count; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);
            Vector3 finalDir = rotation * dirToTarget;

            Vector3 spawnPos;

            if (def.Form.tags.HasFlag(SpellTag.Smite))
            {
                Vector3 currentTarget = targetPos;

                // Si c'est un tir supplémentaire (Multicast) ET qu'on est en Auto-Aim
                // -> On essaie de viser un autre ennemi
                if (i > 0 && !PlayerController.Instance.IsManualAiming)
                {
                    Vector3 scanOrigin = _playerTransform.position + Vector3.up;

                    // On force le mode RANDOM pour arroser la zone
                    Transform extraTarget = EnemyManager.Instance.GetTarget(
                        scanOrigin,
                        def.Range,
                        TargetingMode.Random,
                        def.Effect.aoeRadius,
                        def.RequiresLoS
                    );

                    if (extraTarget != null)
                    {
                        currentTarget = extraTarget.position;
                    }
                    else
                    {
                        // Si pas d'autre ennemi trouvé (ex: il n'en reste qu'un), 
                        // on tape à côté de la cible principale pour ne pas tout empiler
                        Vector2 rnd = UnityEngine.Random.insideUnitCircle * 3.0f;
                        currentTarget += new Vector3(rnd.x, 0, rnd.y);
                    }
                }
                // Si c'est le tir principal ou en visée manuelle avec Multicast
                // -> On ajoute juste un peu de dispersion aléatoire
                else if (count > 1)
                {
                    Vector2 rnd = UnityEngine.Random.insideUnitCircle * 2.0f;
                    currentTarget += new Vector3(rnd.x, 0, rnd.y);
                }

                spawnPos = currentTarget;
            }
            else
            {
                // Cas BOLT / NOVA / ORBIT : Spawn DEPUIS le joueur
                spawnPos = _playerTransform.position + Vector3.up + finalDir * 0.5f;
            }

            GameObject p = ProjectilePool.Instance.Get(def.Form.prefab, spawnPos, Quaternion.LookRotation(finalDir));
            if (p.TryGetComponent<ProjectileController>(out var ctrl))
            {
                ctrl.Initialize(def, finalDir, i, count);
            }
        }
    }

    // --- GESTION INVENTAIRE (AVEC UPGRADES) ---

    public bool CanAddSpell() => activeSlots.Count < maxSpellSlots;
    public List<SpellSlot> GetSlots() => activeSlots;

    public bool HasForm(SpellForm form)
    {
        foreach (var slot in activeSlots)
        {
            if (slot.formRune != null && slot.formRune.Data == form) return true;
        }
        return false;
    }

    // 1. AJOUT NOUVEAU SORT + UPGRADE INITIALE
    public void AddNewSpellWithUpgrade(SpellForm form, RuneDefinition upgradeDef)
    {
        if (!CanAddSpell()) return;

        SpellSlot newSlot = new SpellSlot();

        newSlot.formRune = new Rune(form); // Niveau 1
        newSlot.formRune.InitializeWithStats(upgradeDef); // Application stats piochées

        var defaultEffect = Resources.Load<SpellEffect>("Spells/Effects/Physical");
        if (defaultEffect == null) defaultEffect = ScriptableObject.CreateInstance<SpellEffect>();
        newSlot.effectRune = new Rune(defaultEffect);

        newSlot.ForceInit();
        activeSlots.Add(newSlot);
        OnInventoryUpdated?.Invoke();
    }

    // 2. UPGRADE FORME EXISTANTE
    public void UpgradeExistingForm(SpellForm form, RuneDefinition upgradeDef)
    {
        foreach (var slot in activeSlots)
        {
            if (slot.formRune != null && slot.formRune.Data == form)
            {
                slot.formRune.ApplyUpgrade(upgradeDef);
                slot.RecalculateStats();
                OnInventoryUpdated?.Invoke();
                return;
            }
        }
    }

    // 3. REMPLACEMENT SORT
    public void ReplaceSpell(SpellForm newForm, int slotIndex, RuneDefinition upgradeDef)
    {
        if (slotIndex < 0 || slotIndex >= activeSlots.Count) return;
        SpellSlot slot = activeSlots[slotIndex];

        slot.formRune = new Rune(newForm);
        slot.formRune.InitializeWithStats(upgradeDef);

        // Reset Effet et Mods pour éviter les conflits
        var defaultEffect = Resources.Load<SpellEffect>("Spells/Effects/Physical");
        if (defaultEffect == null) defaultEffect = ScriptableObject.CreateInstance<SpellEffect>();
        slot.effectRune = new Rune(defaultEffect);

        slot.modifierRunes = new Rune[2]; // Reset mods

        slot.RecalculateStats();
        slot.currentCooldown = 0.5f;
        OnInventoryUpdated?.Invoke();
    }

    // 4. APPLICATION EFFET (Nouveau ou Upgrade)
    public void ApplyEffectToSlot(SpellEffect effectSO, int slotIndex, RuneDefinition upgradeDef)
    {
        if (slotIndex < 0 || slotIndex >= activeSlots.Count) return;
        SpellSlot slot = activeSlots[slotIndex];

        // Si c'est le même effet, on l'améliore
        if (slot.effectRune.Data == effectSO)
        {
            slot.effectRune.ApplyUpgrade(upgradeDef);
        }
        else
        {
            // Si c'est un nouveau, on remplace et on applique l'upgrade
            slot.effectRune = new Rune(effectSO);
            slot.effectRune.InitializeWithStats(upgradeDef);
        }

        slot.RecalculateStats();
        OnInventoryUpdated?.Invoke();
    }

    // 5. APPLICATION MODIFICATEUR
    public bool TryApplyModifierToSlot(SpellModifier mod, int slotIndex, int replaceIndex, RuneDefinition upgradeDef)
    {
        if (slotIndex < 0 || slotIndex >= activeSlots.Count) return false;
        SpellSlot slot = activeSlots[slotIndex];

        if (mod.requiredTag != SpellTag.None && !slot.formRune.AsForm.tags.HasFlag(mod.requiredTag)) return false;

        // A. Upgrade Existant (On garde le niveau et on monte)
        for (int i = 0; i < slot.modifierRunes.Length; i++)
        {
            if (slot.modifierRunes[i] != null && slot.modifierRunes[i].Data == mod)
            {
                slot.modifierRunes[i].ApplyUpgrade(upgradeDef); // Level ++
                slot.RecalculateStats();
                OnInventoryUpdated?.Invoke();
                return true;
            }
        }

        // B. Ajout Nouveau (Slot vide) -> Start Level 1
        for (int i = 0; i < slot.modifierRunes.Length; i++)
        {
            if (slot.modifierRunes[i] == null || slot.modifierRunes[i].Data == null)
            {
                slot.modifierRunes[i] = new Rune(mod);
                slot.modifierRunes[i].InitializeWithStats(upgradeDef); // Level 1
                slot.RecalculateStats();
                OnInventoryUpdated?.Invoke();
                return true;
            }
        }

        // C. Remplacement Forcé -> Start Level 1
        if (replaceIndex != -1 && replaceIndex < slot.modifierRunes.Length)
        {
            slot.modifierRunes[replaceIndex] = new Rune(mod);
            slot.modifierRunes[replaceIndex].InitializeWithStats(upgradeDef); // <--- CORRECTION ICI (C'était ApplyUpgrade)
            slot.RecalculateStats();
            OnInventoryUpdated?.Invoke();
            return true;
        }

        return false; // Inventaire plein
    }

    // 6. UPGRADE SLOT SPÉCIFIQUE (Forme)
    // Appelée par l'UI quand on clique sur un slot contenant déjà la même forme
    public void UpgradeSpellAtSlot(int slotIndex, RuneDefinition upgradeDef)
    {
        if (slotIndex < 0 || slotIndex >= activeSlots.Count) return;

        SpellSlot slot = activeSlots[slotIndex];

        if (slot.formRune != null)
        {
            slot.formRune.ApplyUpgrade(upgradeDef);
            slot.RecalculateStats();

            // C'est la ligne clé qui manquait : on notifie l'UI !
            OnInventoryUpdated?.Invoke();
        }
    }
}