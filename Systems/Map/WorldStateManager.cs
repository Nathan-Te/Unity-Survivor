using System.Collections.Generic;
using UnityEngine;

public class WorldStateManager : Singleton<WorldStateManager>
{
    // Dictionnaire : ID Unique de l'objet -> Est-il d�truit/activ� ?
    private HashSet<string> _interactedObjects = new HashSet<string>();

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