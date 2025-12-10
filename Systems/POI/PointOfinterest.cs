using UnityEngine;

public abstract class PointOfInterest : MonoBehaviour
{
    [Header("POI Settings")]
    public string poiName;
    public bool isCompleted = false;

    // NOUVEAU : Gestion centrale de l'ID et de la persistance
    protected string _uniqueID;

    public void Initialize(string id)
    {
        _uniqueID = id;

        // Vérification immédiate : Est-ce que ce POI a déjà été validé dans cette partie ?
        if (WorldStateManager.Instance != null && WorldStateManager.Instance.IsInteracted(_uniqueID))
        {
            // Si oui, on le marque comme complété et on le désactive
            isCompleted = true;
            OnAlreadyCompleted(); // Hook pour comportement personnalisé
            gameObject.SetActive(false);
        }
    }

    // Permet aux enfants de faire un truc spécial si déjà complété (ex: visuel différent)
    protected virtual void OnAlreadyCompleted() { }

    protected virtual void CompletePOI()
    {
        if (isCompleted) return;
        isCompleted = true;

        Debug.Log($"POI {poiName} completed!");

        // 1. Donner la récompense
        GrantReward();

        // 2. Sauvegarder l'état
        if (!string.IsNullOrEmpty(_uniqueID) && WorldStateManager.Instance != null)
        {
            WorldStateManager.Instance.RegisterInteraction(_uniqueID);
        }

        // 3. Désactivation (Délai optionnel pour laisser jouer un son/VFX)
        Destroy(gameObject, 0.5f);
    }

    protected abstract void GrantReward();
}