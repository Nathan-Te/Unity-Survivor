using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }

    // Dictionnaire : Prefab ID -> File d'attente d'objets
    private Dictionary<int, Queue<GameObject>> _pools = new Dictionary<int, Queue<GameObject>>();

    private void Awake()
    {
        Instance = this;
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int key = prefab.GetInstanceID();

        // Si le pool n'existe pas pour ce prefab, on le crée
        if (!_pools.ContainsKey(key))
        {
            _pools.Add(key, new Queue<GameObject>());
        }

        // Si on a un objet en réserve, on le sort
        if (_pools[key].Count > 0)
        {
            GameObject obj = _pools[key].Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // Sinon on instancie (et on l'ajoute au parent pour garder la hiérarchie propre)
            GameObject newObj = Instantiate(prefab, position, rotation, transform);
            return newObj;
        }
    }

    public void ReturnToPool(GameObject obj, GameObject originalPrefab)
    {
        obj.SetActive(false);
        int key = originalPrefab.GetInstanceID();

        if (!_pools.ContainsKey(key))
        {
            _pools.Add(key, new Queue<GameObject>());
        }

        _pools[key].Enqueue(obj);
    }
}