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
    private int _frameOffset; // Pour désynchroniser les ennemis

    // Seuils de distance pour le LOD (à ajuster selon votre caméra)
    private float DIST_HIGH_QUALITY = 4f;
    private float DIST_MED_QUALITY = 10f;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _controller = GetComponentInParent<EnemyController>();
        _renderer = GetComponentInChildren<Renderer>(); // Trouve le mesh pour isVisible
        _lastPosition = transform.position;

        // Offset aléatoire pour éviter que tous les ennemis calculent à la même frame
        _frameOffset = Random.Range(0, 10);

        // Culling de base Unity (arrête l'anim si hors caméra)
        _animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

        DIST_HIGH_QUALITY = DIST_HIGH_QUALITY * DIST_HIGH_QUALITY;
        DIST_MED_QUALITY = DIST_MED_QUALITY * DIST_MED_QUALITY;
    }

    private void Update()
    {
        if (PlayerController.Instance == null) return;

        // 1. Calcul Distance Joueur
        // Calcul vectoriel simple (juste des additions et multiplications)
        float distSqrToPlayer = (transform.position - PlayerController.Instance.transform.position).sqrMagnitude;
        //float distToPlayer = Vector3.Distance(transform.position, PlayerController.Instance.transform.position);

        // 2. Logique LOD (Throttling)
        int updateInterval = 1; // Par défaut : chaque frame

        if (distSqrToPlayer > DIST_MED_QUALITY) updateInterval = 6; // Très loin : 1 update toutes les 6 frames
        else if (distSqrToPlayer > DIST_HIGH_QUALITY) updateInterval = 3; // Moyen : 1 update toutes les 3 frames

        // Si ce n'est pas le tour de cet ennemi, on sort (économie CPU)
        if ((Time.frameCount + _frameOffset) % updateInterval != 0) return;

        // 3. Mise à jour (Seulement si visible ou très proche)
        if (_renderer != null && (_renderer.isVisible || distSqrToPlayer < DIST_HIGH_QUALITY))
        {
            float distanceMoved = (transform.position - _lastPosition).magnitude;

            // On compense le temps écoulé (dt * interval) pour avoir la vitesse réelle
            // Sinon l'animation serait 3x ou 6x trop rapide car on a sauté des frames
            float currentSpeed = distanceMoved / (Time.deltaTime * updateInterval);

            _lastPosition = transform.position;

            // Optimisation SetFloat : on n'envoie que si ça change vraiment
            if (Mathf.Abs(currentSpeed - _lastSpeedValue) > 0.05f)
            {
                _animator.SetFloat(SpeedHash, currentSpeed);
                _lastSpeedValue = currentSpeed;
            }
        }
    }

    public void TriggerAttackAnimation()
    {
        // Les attaques sont prioritaires, on les joue toujours immédiatement
        _animator.SetTrigger(AttackHash);
    }

    public void OnAttackFrame()
    {
        if (_controller != null) _controller.SpawnProjectile();
    }
}