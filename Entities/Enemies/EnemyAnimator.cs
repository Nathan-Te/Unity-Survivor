using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    private Animator _animator;
    private EnemyController _controller;
    private Vector3 _lastPosition;

    // Optimisation : On cache les Hashes pour ne pas utiliser de strings à chaque frame
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private float _lastSpeedValue = -1f;

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        // On cherche le controller sur le parent (la racine de l'ennemi)
        _controller = GetComponentInParent<EnemyController>();

        _lastPosition = transform.position;

        _animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
    }

    private void Update()
    {
        // 1. Calcul de la vitesse réelle
        // Le Job System déplace le Transform, on mesure juste la distance parcourue
        float distanceMoved = (transform.position - _lastPosition).magnitude;
        float currentSpeed = distanceMoved / Time.deltaTime;
        _lastPosition = transform.position;

        // OPTIMISATION : On ne met à jour l'Animator que si la vitesse change significativement (> 0.05f)
        if (Mathf.Abs(currentSpeed - _lastSpeedValue) > 0.05f)
        {
            _animator.SetFloat(SpeedHash, currentSpeed);
            _lastSpeedValue = currentSpeed;
        }
    }

    // Appelé par le Controller pour lancer l'anim
    public void TriggerAttackAnimation()
    {
        _animator.SetTrigger(AttackHash);
    }

    // --- ANIMATION EVENT ---
    // À placer dans l'onglet Animation sur la frame précise du tir
    public void OnAttackFrame()
    {
        if (_controller != null)
        {
            _controller.SpawnProjectile(); // Le tir part maintenant !
        }
    }
}