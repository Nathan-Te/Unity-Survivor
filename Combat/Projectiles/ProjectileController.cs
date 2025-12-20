using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
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

    // Pour les chaînes : ennemis déjà touchés dans toute la chaîne (pour éviter les boucles)
    private HashSet<int> _chainHitTargets = new HashSet<int>();

    // Pour les Orbit : cooldown par ennemi pour éviter les hits multiples
    private Dictionary<int, float> _orbitHitCooldowns = new Dictionary<int, float>();
    private const float ORBIT_HIT_COOLDOWN = 0.5f; // 0.5 secondes entre chaque hit sur le même ennemi

    private static MaterialPropertyBlock _propBlock;
    private Renderer _renderer;
    private TrailRenderer _trail; // Cached to avoid GetComponent allocations

    private bool _shouldDestroyEffect;

    // Sub-components for specialized functionality
    private ProjectileDamageHandler _damageHandler;
    private ProjectileChainReaction _chainReaction;

    public SpellDefinition Definition => _def;
    public IMotionStrategy MotionStrategy => _motionStrategy;
    public bool IsOrbit => _def?.Form != null && _def.Form.tags.HasFlag(SpellTag.Orbit);

    private void Awake()
    {
        // Cache components once at creation (not every time we spawn from pool)
        _renderer = GetComponentInChildren<Renderer>();
        _trail = GetComponent<TrailRenderer>();
        if (_trail == null)
        {
            _trail = GetComponentInChildren<TrailRenderer>();
        }

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
        _chainHitTargets.Clear();
        _shouldDestroyEffect = false;

        // Clear trail to prevent visual artifacts when reusing from pool
        if (_trail != null)
        {
            _trail.Clear();
        }

        // 1. Choix de la Stratégie
        if (_def.Form.tags.HasFlag(SpellTag.Smite))
        {
            // Use timing from SpellDefinition (configured per Form+Effect combination)
            float impactDelay = _def.SmiteImpactDelay;
            float vfxDelay = _def.SmiteVfxSpawnDelay;
            float lifetime = _def.SmiteLifetime;

            // Default vfxDelay to impactDelay if not set
            if (vfxDelay == 0f) vfxDelay = impactDelay;

            _motionStrategy = new SmiteMotion(impactDelay, vfxDelay, lifetime);
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

        // 2. Visuels - Simply multiply prefab's localScale by size multiplier
        // Get base scale from prefab
        float prefabBaseScale = sourcePrefab != null ? sourcePrefab.transform.localScale.x : 1f;

        // Apply size: multiply prefab's base scale by the size multiplier
        transform.localScale = Vector3.one * prefabBaseScale * def.Size;

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

    /// <summary>
    /// Initialize a chained projectile with a list of enemies already hit in the chain
    /// </summary>
    public void InitializeWithChain(SpellDefinition def, Vector3 direction, GameObject sourcePrefab, HashSet<int> chainHitTargets)
    {
        // Normal initialization
        Initialize(def, direction, sourcePrefab);

        // Copy the chain hit list (create a new HashSet to avoid reference issues)
        _chainHitTargets = new HashSet<int>(chainHitTargets);
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

        // Décrémenter les cooldowns de hit pour les Orbit
        if (IsOrbit && _orbitHitCooldowns.Count > 0)
        {
            var keys = new System.Collections.Generic.List<int>(_orbitHitCooldowns.Keys);
            foreach (var enemyID in keys)
            {
                _orbitHitCooldowns[enemyID] -= dt;
                if (_orbitHitCooldowns[enemyID] <= 0f)
                {
                    _orbitHitCooldowns.Remove(enemyID);
                }
            }
        }
    }

    // --- LOGIQUE PUBLIQUE POUR LES STRATÉGIES ---

    /// <summary>
    /// Spawns the impact VFX for Smite at the right moment (called by SmiteMotion)
    /// </summary>
    public void TriggerSmiteVfx()
    {
        if (_def.ImpactVfxPrefab != null && VFXPool.Instance != null)
        {
            VFXPool.Instance.Spawn(_def.ImpactVfxPrefab, transform.position, Quaternion.identity, 2f);
        }
    }

    /// <summary>
    /// Triggers explosion/damage for Smite (called by SmiteMotion)
    /// </summary>
    public void TriggerSmiteExplosion()
    {
        // Don't spawn VFX here - it's handled separately by TriggerSmiteVfx with precise timing
        _damageHandler.ApplyAreaDamage(transform.position, _def, spawnVfx: false);
        // NOTE: Don't despawn here anymore - SmiteMotion handles lifetime
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
        // Smite ignore toutes les collisions (explose sur timer)
        if (_def.Form != null && _def.Form.tags.HasFlag(SpellTag.Smite))
            return;

        // OPTIMISATION : Utiliser les layers au lieu de GetComponent
        int layer = other.gameObject.layer;

        // Orbit ignore seulement les Obstacles (peut toucher les ennemis et les destructibles)
        if (IsOrbit)
        {
            if (layer == LayerMask.NameToLayer("Obstacle"))
                return;
        }

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

                // Comportement spécial pour les Orbit
                if (IsOrbit)
                {
                    // Vérifier si on peut toucher cet ennemi (cooldown écoulé)
                    if (_orbitHitCooldowns.ContainsKey(enemyID))
                        return; // Encore en cooldown pour cet ennemi

                    // Appliquer le hit
                    ApplyHit(enemy);

                    // Ajouter un cooldown pour cet ennemi
                    _orbitHitCooldowns[enemyID] = ORBIT_HIT_COOLDOWN;

                    // Les Orbit ne se despawn jamais sur collision
                    return;
                }

                // Comportement normal pour les autres projectiles
                if (_hitTargets.Contains(enemyID)) return;

                // Si c'est un projectile chainé, ignorer les ennemis déjà touchés dans la chaîne
                if (_def.ChainCount > 0 && _chainHitTargets.Contains(enemyID))
                {
                    return; // Traverse this enemy without hitting it
                }

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
        int targetID = target.GetInstanceID();

        // Add this target to the chain hit list BEFORE handling chain (to prevent immediate loops)
        if (_def.ChainCount > 0)
        {
            _chainHitTargets.Add(targetID);
        }

        // Delegate to damage handler
        _damageHandler.ApplyHit(target, _def);

        // Handle chain reaction if enabled
        if (_def.ChainCount > 0)
        {
            // Pass the updated chain hit list to the chain reaction
            _chainReaction.HandleChainReaction(target, _def, _chainHitTargets);
        }
    }
}
