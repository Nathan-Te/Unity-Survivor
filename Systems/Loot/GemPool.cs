using System.Collections.Generic;
using UnityEngine;

public class GemPool : MonoBehaviour
{
    public static GemPool Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private GameObject gemPrefab;
    [SerializeField] private int maxActiveGems = 400; // Limite dure pour éviter le crash

    // File pour le recyclage standard
    private Queue<GameObject> _inactivePool = new Queue<GameObject>();

    // Liste des gemmes actives pour gérer la limite (Fusion/Despawn)
    private List<ExperienceGem> _activeGems = new List<ExperienceGem>();

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        _activeGems.Clear();
        _inactivePool.Clear();
        Instance = null;
    }

    public void Spawn(Vector3 position, int xpValue)
    {
        ExperienceGem gemScript;

        // CAS 1 : On a atteint la limite max de gemmes en jeu
        if (_activeGems.Count >= maxActiveGems)
        {
            gemScript = _activeGems[0];
            _activeGems.RemoveAt(0);
            gemScript.transform.position = position;
        }
        // CAS 2 : On pioche dans le pool inactif
        else if (_inactivePool.Count > 0)
        {
            GameObject obj = _inactivePool.Dequeue();
            obj.SetActive(true);
            gemScript = obj.GetComponent<ExperienceGem>();
            gemScript.transform.position = position;
        }
        // CAS 3 : On crée du neuf
        else
        {
            GameObject newObj = Instantiate(gemPrefab, transform);
            gemScript = newObj.GetComponent<ExperienceGem>();
        }

        // Initialisation et Ajout à la liste active
        gemScript.Initialize(xpValue);
        _activeGems.Add(gemScript);
    }

    public void ReturnToPool(GameObject gemObj)
    {
        if (gemObj.TryGetComponent<ExperienceGem>(out var script))
        {
            // On l'enlève de la liste active
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

        Debug.Log("[GemPool] Pool vidé");
    }
}