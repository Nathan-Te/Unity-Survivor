using System.Collections.Generic;
using UnityEngine;

public class WorldStateManager : MonoBehaviour
{
    public static WorldStateManager Instance { get; private set; }

    // Dictionnaire : ID Unique de l'objet -> Est-il détruit/activé ?
    private HashSet<string> _interactedObjects = new HashSet<string>();

    private void Awake()
    {
        Instance = this;
    }

    public bool IsInteracted(string id)
    {
        return _interactedObjects.Contains(id);
    }

    public void RegisterInteraction(string id)
    {
        if (!_interactedObjects.Contains(id))
        {
            _interactedObjects.Add(id);
            // Debug.Log($"WorldState: Saved interaction for {id}");
        }
    }
}