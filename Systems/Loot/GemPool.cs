using System.Collections.Generic;
using UnityEngine;

public class GemPool : Singleton<GemPool>
{
    [Header("Configuration")]
    [SerializeField] private GameObject gemPrefab;
    [SerializeField] private int maxActiveGems = 400; // Limite dure pour �viter le crash

    // File pour le recyclage standard
    private Queue<GameObject> _inactivePool = new Queue<GameObject>();

    // Liste des gemmes actives pour g�rer la limite (Fusion/Despawn)
    private List<ExperienceGem> _activeGems = new List<ExperienceGem>();

    protected override void OnDestroy()
    {
        _activeGems.Clear();
        _inactivePool.Clear();

        base.OnDestroy();
    }

    public void Spawn(Vector3 position, int xpValue)
    {
        ExperienceGem gemScript;

        // CAS 1 : Limite atteinte -> On recycle la plus vieille (FIFO)
        if (_activeGems.Count >= maxActiveGems)
        {
            gemScript = _activeGems[0];
            _activeGems.RemoveAt(0);
        }
        // CAS 2 : R�cup�ration du pool inactif
        else if (_inactivePool.Count > 0)
        {
            GameObject obj = _inactivePool.Dequeue();
            obj.SetActive(true);
            gemScript = obj.GetComponent<ExperienceGem>();
        }
        // CAS 3 : Cr�ation d'une nouvelle
        else
        {
            // On instancie sous le Manager pour garder la hi�rarchie propre
            GameObject newObj = Instantiate(gemPrefab, transform);
            gemScript = newObj.GetComponent<ExperienceGem>();
        }

        // --- CORRECTION : On applique la position ICI pour tous les cas ---
        gemScript.transform.position = position;
        // ----------------------------------------------------------------

        // Initialisation et Ajout � la liste active
        gemScript.Initialize(xpValue);
        _activeGems.Add(gemScript);
    }

    public void ReturnToPool(GameObject gemObj)
    {
        if (gemObj.TryGetComponent<ExperienceGem>(out var script))
        {
            // On l'enl�ve de la liste active
            if (_activeGems.Contains(script))
            {
                _activeGems.Remove(script);
            }
        }

        gemObj.SetActive(false);
        _inactivePool.Enqueue(gemObj);
    }

    public void ClearAll()
    {
        foreach (var gem in _activeGems)
        {
            if (gem != null && gem.gameObject != null)
                Destroy(gem.gameObject);
        }
        _activeGems.Clear();

        while (_inactivePool.Count > 0)
        {
            GameObject obj = _inactivePool.Dequeue();
            if (obj != null) Destroy(obj);
        }

        Debug.Log("[GemPool] Pool vid�");
    }
}