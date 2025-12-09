using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }

    // Dictionnaire : Prefab ID -> File d'attente d'objets
    private Dictionary<int, Queue<GameObject>> _pools = new Dictionary<int, Queue<GameObject>>();

    private List<ProjectileController> _activeProjectiles = new List<ProjectileController>();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        // BOUCLE OPTIMISÉE
        // On évite le foreach pour ne pas générer de garbage
        float dt = Time.deltaTime;
        for (int i = _activeProjectiles.Count - 1; i >= 0; i--)
        {
            // Sécurité si un projectile a été détruit brutalement
            if (_activeProjectiles[i] == null || !_activeProjectiles[i].gameObject.activeSelf)
            {
                _activeProjectiles.RemoveAt(i);
                continue;
            }

            _activeProjectiles[i].ManualUpdate(dt);
        }
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int key = prefab.GetInstanceID();
        if (!_pools.ContainsKey(key)) _pools.Add(key, new Queue<GameObject>());

        GameObject obj;
        if (_pools[key].Count > 0)
        {
            obj = _pools[key].Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab, position, rotation, transform);
        }

        // ENREGISTREMENT POUR UPDATE
        if (obj.TryGetComponent<ProjectileController>(out var ctrl))
        {
            _activeProjectiles.Add(ctrl);
        }

        return obj;
    }

    public void ReturnToPool(GameObject obj, GameObject originalPrefab)
    {
        // DÉSINSCRIPTION DE L'UPDATE
        if (obj.TryGetComponent<ProjectileController>(out var ctrl))
        {
            _activeProjectiles.Remove(ctrl);
        }

        obj.SetActive(false);
        int key = originalPrefab.GetInstanceID();
        if (!_pools.ContainsKey(key)) _pools.Add(key, new Queue<GameObject>());
        _pools[key].Enqueue(obj);
    }
}