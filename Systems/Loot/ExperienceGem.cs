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

        // Optimisation : On désactive ce script (Update) tant qu'il n'est pas attiré !
        // Le mouvement ne sert à rien tant qu'elle est au sol.
        this.enabled = false;
    }

    public void AttractTo(Transform target)
    {
        if (_isAttracted) return;

        _isAttracted = true;
        _target = target;

        // On réactive le script pour que l'Update tourne enfin
        this.enabled = true;
    }

    private void Update()
    {
        // Plus besoin de vérifier _isAttracted ici, car si le script est enabled, c'est qu'on est attiré
        if (_target == null)
        {
            Collect(); // Sécurité si le joueur meurt ou disparaît
            return;
        }

        // Mouvement fluide
        transform.position = Vector3.MoveTowards(transform.position, _target.position, flySpeed * Time.deltaTime);

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

        if (GemPool.Instance != null)
        {
            GemPool.Instance.ReturnToPool(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}