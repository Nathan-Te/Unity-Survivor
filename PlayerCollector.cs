using UnityEngine;

public class PlayerCollector : MonoBehaviour
{
    [Header("Settings")]
    public float magnetRadius = 3f;
    public LayerMask lootLayer; // Crée un Layer "Loot" !

    private Collider[] _hitBuffer = new Collider[20];
    private float _timer;

    private void Update()
    {
        // Scan 10 fois par seconde seulement (Optimisation)
        _timer += Time.deltaTime;
        if (_timer >= 0.1f)
        {
            ScanForGems();
            _timer = 0f;
        }
    }

    private void ScanForGems()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, magnetRadius, _hitBuffer, lootLayer);

        for (int i = 0; i < count; i++)
        {
            if (_hitBuffer[i].TryGetComponent<ExperienceGem>(out var gem))
            {
                // On dit à la gemme : "Viens à moi !"
                gem.AttractTo(transform);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, magnetRadius);
    }
}