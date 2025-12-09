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

    public void Spawn(Vector3 position, int xpValue)
    {
        ExperienceGem gemScript;

        // CAS 1 : On a atteint la limite max de gemmes en jeu
        if (_activeGems.Count >= maxActiveGems)
        {
            // OPTIMISATION : FUSION (VACUUM)
            // Au lieu d'instancier, on prend la gemme active la plus vieille (index 0)
            // ou celle qui est le plus loin du joueur (plus coûteux à calculer).
            // Ici, on prend la plus ancienne (FIFO) pour la performance O(1).

            gemScript = _activeGems[0];

            // On la déplace "téléportation" vers le nouveau monstre mort
            // Note : Dans un jeu avancé, on ajouterait la valeur de l'ancienne gemme
            // à une "Super Gemme" pour ne pas perdre d'XP.
            // Pour l'instant, on recycle juste le contenant.

            // On la retire du début de la liste et on la remettra à la fin
            _activeGems.RemoveAt(0);

            // On réinitialise sa position
            gemScript.transform.position = position;
        }
        // CAS 2 : On pioche dans le pool inactif
        else if (_inactivePool.Count > 0)
        {
            GameObject obj = _inactivePool.Dequeue();
            obj.SetActive(true);
            gemScript = obj.GetComponent<ExperienceGem>();
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
}