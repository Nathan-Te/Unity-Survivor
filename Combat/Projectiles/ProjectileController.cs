using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main controller for projectiles - handles initialization, movement, and collision detection.
/// Delegates damage and chain reactions to specialized components.
/// </summary>
public class ProjectileController : MonoBehaviour
{
    private SpellDefinition _def;
    private GameObject _sourcePrefab;

    // Stratégie active (Pattern Strategy)
    private IMotionStrategy _motionStrategy;

    private int _hitCount;
    private bool _isHostile;

    private HashSet<int> _hitTargets = new HashSet<int>();

    private static MaterialPropertyBlock _propBlock;
    private Renderer _renderer;

    private bool _shouldDestroyEffect;

    // Sub-components for specialized functionality
    private ProjectileDamageHandler _damageHandler;
    private ProjectileChainReaction _chainReaction;

    public SpellDefinition Definition => _def;

    private void Awake()
    {
        // On récupère le renderer une seule fois à la création
        _renderer = GetComponentInChildren<Renderer>();

        // Get or add sub-components
        _damageHandler = GetComponent<ProjectileDamageHandler>();
        if (_damageHandler == null)
            _damageHandler = gameObject.AddComponent<ProjectileDamageHandler>();

        _chainReaction = GetComponent<ProjectileChainReaction>();
        if (_chainReaction == null)
            _chainReaction = gameObject.AddComponent<ProjectileChainReaction>();
    }

    public void Initialize(SpellDefinition def, Vector3 direction, GameObject sourcePrefab, int index = 0, int totalCount = 1)
    {
        _def = def;
        _sourcePrefab = sourcePrefab; // Use the actual prefab passed from the pool, not def.Prefab
        _hitCount = 0;
        _isHostile = false;
        _hitTargets.Clear();
        _shouldDestroyEffect = false;

        // 1. Choix de la Stratégie
        if (_def.Form.tags.HasFlag(SpellTag.Smite))
        {
            _motionStrategy = new SmiteMotion(_def.Form.impactDelay);
            // On n'oriente pas le Smite, il est fixe au sol là où il a spawn
        }
        else if (_def.Form.tags.HasFlag(SpellTag.Orbit))
        {
            _motionStrategy = new OrbitMotion(_def.Duration, _def.Speed, index, totalCount);
            // Position initiale immédiate pour éviter le glitch visuel
            _motionStrategy.Update(this, 0f);
        }
        else
        {
            transform.forward = direction;
            _motionStrategy = new LinearMotion(transform.position, _def.Range, _def.Speed, _def.IsHoming, false);
        }

        // 2. Visuels
        transform.localScale = Vector3.one * def.Size;

        // --- CORRECTION COULEUR (Optimisée) ---
        // Au lieu de toucher à .material (qui crée une copie et une fuite mémoire),
        // on utilise SetPropertyBlock.
        if (_renderer != null && def.Effect.tintColor != Color.white)
        {
            if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

            _renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor("_BaseColor", def.Effect.tintColor); // Utilise "_BaseColor" pour URP
            _renderer.SetPropertyBlock(_propBlock);
        }
    }

    public void InitializeEnemyProjectile(float damage, GameObject sourcePrefab)
    {
        _def = new SpellDefinition();
        _def.Damage = damage;
        _def.Speed = 10f;
        _def.Range = 30f;
        _def.Pierce = 0;
        _def.Effect = null;

        _sourcePrefab = sourcePrefab;
        _isHostile = true;
        _hitCount = 0;
        _hitTargets.Clear();
        _shouldDestroyEffect = true;

        // Mouvement linéaire simple pour l'ennemi
        transform.forward = transform.forward; // Déjà orienté par l'ennemi
        _motionStrategy = new LinearMotion(transform.position, 30f, 10f, false, true);
    }

    public void ManualUpdate(float dt)
    {
        if (_def == null) return;

        // Note : J'ai remplacé Time.deltaTime par 'dt' partout
        if (_motionStrategy != null)
        {
            _motionStrategy.Update(this, dt);
        }
    }

    // --- LOGIQUE PUBLIQUE POUR LES STRATÉGIES ---

    public void TriggerSmiteExplosion()
    {
        _damageHandler.ApplyAreaDamage(transform.position, _def);
        Despawn();
    }

    public void Despawn()
    {
        if (_shouldDestroyEffect && _def?.Effect != null)
        {
            Destroy(_def.Effect);
            _def.Effect = null;
        }

        if (ProjectilePool.Instance != null)
        {
            ProjectilePool.Instance.ReturnToPool(gameObject, _sourcePrefab);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- COLLISIONS ---

    private void OnTriggerEnter(Collider other)
    {
        if (_def.Form != null && _def.Form.tags.HasFlag(SpellTag.Smite)) return;

        // OPTIMISATION : Utiliser les layers au lieu de GetComponent
        int layer = other.gameObject.layer;

        if (_isHostile)
        {
            if (layer == LayerMask.NameToLayer("Player"))
            {
                int targetID = other.gameObject.GetInstanceID();
                if (_hitTargets.Contains(targetID)) return;
                _hitTargets.Add(targetID);

                if (other.TryGetComponent<PlayerController>(out var player))
                {
                    other.GetComponent<PlayerController>().TakeDamage(_def.Damage);
                    Despawn();
                }
            }
            else if (layer == LayerMask.NameToLayer("Obstacle"))
            {
                Despawn();
            }
            return;
        }

        // Pour les projectiles joueur
        if (layer == LayerMask.NameToLayer("Enemy"))
        {
            if (EnemyManager.Instance.TryGetEnemyByCollider(other, out EnemyController enemy))
            {
                int enemyID = enemy.GetInstanceID();
                if (_hitTargets.Contains(enemyID)) return;
                _hitTargets.Add(enemyID);
                ApplyHit(enemy);
                _hitCount++;

                if (_def.Effect.aoeRadius > 0 || _hitCount > _def.Pierce)
                    Despawn();
            }
        }
        else if (layer == LayerMask.NameToLayer("Destructible"))
        {
            other.GetComponent<IDamageable>()?.TakeDamage(_def.Damage);
            _hitCount++;
            if (_def.Effect.aoeRadius <= 0 && _hitCount > _def.Pierce) Despawn();
        }
        else if (layer == LayerMask.NameToLayer("Obstacle"))
        {
            if (_def.Effect.aoeRadius > 0)
                _damageHandler.ApplyAreaDamage(transform.position, _def);
            Despawn();
        }
    }

    private void ApplyHit(EnemyController target)
    {
        // Delegate to damage handler
        _damageHandler.ApplyHit(target, _def);

        // Handle chain reaction if enabled
        if (_def.ChainCount > 0)
        {
            _chainReaction.HandleChainReaction(target, _def);
        }
    }
}
