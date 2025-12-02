using UnityEngine;

public abstract class PointOfInterest : MonoBehaviour
{
    [Header("POI Settings")]
    public string poiName;
    public bool isCompleted = false;

    // Appelé quand le POI est validé (détruit, chargé, etc.)
    protected virtual void CompletePOI()
    {
        if (isCompleted) return;
        isCompleted = true;
        Debug.Log($"POI {poiName} completed!");
        GrantReward();

        // Désactivation visuelle ou destruction après délai
        Destroy(gameObject, 1f);
    }

    protected abstract void GrantReward();
}