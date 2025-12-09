using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator _animator;
    private PlayerController _controller;

    // Paramètres
    private static readonly int InputXHash = Animator.StringToHash("InputX");
    private static readonly int InputZHash = Animator.StringToHash("InputZ");
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

    // Pour le lissage circulaire
    private Vector3 _currentLocalDir = Vector3.forward;
    [SerializeField] private float smoothSpeed = 10f; // Vitesse de rotation de l'anim

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _controller = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        if (_controller == null) return;

        Vector2 globalInput = _controller.MoveInput;
        bool isMoving = globalInput.sqrMagnitude > 0.01f;

        // 1. Conversion Monde -> Local
        Vector3 globalMoveDir = new Vector3(globalInput.x, 0, globalInput.y);
        Vector3 targetLocalDir = transform.InverseTransformDirection(globalMoveDir);

        // 2. LISSAGE CIRCULAIRE (C'est la clé !)
        // Si on bouge, on fait tourner le vecteur actuel vers la cible
        // RotateTowards garantit qu'on garde la magnitude (la longueur) du vecteur, donc on ne passe pas par 0
        if (isMoving)
        {
            _currentLocalDir = Vector3.RotateTowards(
                _currentLocalDir,
                targetLocalDir,
                smoothSpeed * Time.deltaTime,
                0f
            );

            // On normalise pour être sûr d'être toujours à fond sur le cercle extérieur du Blend Tree
            _currentLocalDir.Normalize();
        }

        // 3. Envoi à l'Animator
        // On n'utilise PLUS le dampTime de Unity (on met 0), car on a déjà lissé nous-mêmes
        _animator.SetFloat(InputXHash, _currentLocalDir.x, 0f, Time.deltaTime);
        _animator.SetFloat(InputZHash, _currentLocalDir.z, 0f, Time.deltaTime);

        // 4. Gestion de l'arrêt
        _animator.SetBool(IsMovingHash, isMoving);
    }
}