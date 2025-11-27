using UnityEngine;

public class ExperienceGem : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float flySpeed = 15f;

    private int _xpValue;
    private bool _isAttracted;
    private Transform _target;

    public void Initialize(int value)
    {
        _xpValue = value;
        _isAttracted = false;
        _target = null;
        // Optionnel : Ajouter une petite animation de pop ou une couleur selon la valeur
    }

    public void AttractTo(Transform target)
    {
        if (_isAttracted) return; // Déjà capturée
        _isAttracted = true;
        _target = target;
    }

    private void Update()
    {
        if (!_isAttracted || _target == null) return;

        // Mouvement vers le joueur (Accélération simple)
        transform.position = Vector3.MoveTowards(transform.position, _target.position, flySpeed * Time.deltaTime);

        // Collecte
        if (Vector3.Distance(transform.position, _target.position) < 0.5f)
        {
            Collect();
        }
    }

    private void Collect()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.AddExperience(_xpValue);
        }

        // Retour au pool
        GemPool.Instance.ReturnToPool(this.gameObject);
    }
}