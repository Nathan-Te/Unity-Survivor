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

        // Optimisation : On d�sactive ce script (Update) tant qu'il n'est pas attir� !
        // Le mouvement ne sert � rien tant qu'elle est au sol.
        this.enabled = false;
    }

    public void AttractTo(Transform target)
    {
        if (_isAttracted) return;

        _isAttracted = true;
        _target = target;

        // On r�active le script pour que l'Update tourne enfin
        this.enabled = true;
    }

    private void Update()
    {
        // Don't update if game is restarting or not playing
        if (GameStateController.Instance != null && !GameStateController.Instance.IsPlaying)
        {
            return;
        }

        // Plus besoin de v�rifier _isAttracted ici, car si le script est enabled, c'est qu'on est attir�
        if (_target == null)
        {
            Collect(); // S�curit� si le joueur meurt ou dispara�t
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