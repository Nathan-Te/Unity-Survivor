using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    private Animator _animator;
    private EnemyController _controller;
    private Renderer _renderer; // Pour vérifier la visibilité
    private Vector3 _lastPosition;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private float _lastSpeedValue = -1f;
    private int _frameOffset;

    // Seuils de distance (à ajuster selon ta caméra)
    private const float DIST_HIGH_QUALITY = 1f;
    private const float DIST_MED_QUALITY = 2f;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _controller = GetComponentInParent<EnemyController>();
        _renderer = GetComponentInChildren<Renderer>(); // Trouve le mesh
        _lastPosition = transform.position;

        // Offset aléatoire pour désynchroniser les updates
        _frameOffset = Random.Range(0, 10);

        // Culling : C'est la base, mais on va aller plus loin
        _animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
    }

    private void Update()
    {
        // 1. Calcul Distance Joueur
        float distToPlayer = Vector3.Distance(transform.position, PlayerController.Instance.transform.position);

        // 2. Logique LOD (Throttling)
        int updateInterval = 1;

        if (distToPlayer > DIST_MED_QUALITY) updateInterval = 5; // Très loin : 10fps
        else if (distToPlayer > DIST_HIGH_QUALITY) updateInterval = 3; // Moyen : 20fps

        // Si ce n'est pas le tour de cet ennemi, on sort
        if ((Time.frameCount + _frameOffset) % updateInterval != 0) return;

        // 3. Mise à jour (Seulement si visible ou proche)
        if (_renderer.isVisible || distToPlayer < DIST_HIGH_QUALITY)
        {
            float distanceMoved = (transform.position - _lastPosition).magnitude;
            // On compense le temps écoulé (dt * interval) pour avoir la vitesse réelle
            float currentSpeed = distanceMoved / (Time.deltaTime * updateInterval);

            _lastPosition = transform.position;

            if (Mathf.Abs(currentSpeed - _lastSpeedValue) > 0.05f)
            {
                _animator.SetFloat(SpeedHash, currentSpeed);
                _lastSpeedValue = currentSpeed;
            }
        }
    }

    public void TriggerAttackAnimation()
    {
        // Les attaques sont prioritaires, on les joue toujours
        _animator.SetTrigger(AttackHash);
    }

    public void OnAttackFrame()
    {
        if (_controller != null) _controller.SpawnProjectile();
    }
}