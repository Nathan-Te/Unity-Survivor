using UnityEngine;

/// <summary>
/// Safe MonoBehaviour base class that automatically skips Update/FixedUpdate/LateUpdate during scene loading.
/// Use this instead of MonoBehaviour for scripts that access singletons in their Update methods.
/// </summary>
public abstract class MonoBehaviourSafe : MonoBehaviour
{
    private void Update()
    {
        // Skip update during scene loading to prevent null reference errors
        if (SingletonGlobalState.IsSceneLoading) return;

        SafeUpdate();
    }

    private void FixedUpdate()
    {
        // Skip fixed update during scene loading
        if (SingletonGlobalState.IsSceneLoading) return;

        SafeFixedUpdate();
    }

    private void LateUpdate()
    {
        // Skip late update during scene loading
        if (SingletonGlobalState.IsSceneLoading) return;

        SafeLateUpdate();
    }

    /// <summary>
    /// Override this instead of Update(). Will not be called during scene loading.
    /// </summary>
    protected virtual void SafeUpdate() { }

    /// <summary>
    /// Override this instead of FixedUpdate(). Will not be called during scene loading.
    /// </summary>
    protected virtual void SafeFixedUpdate() { }

    /// <summary>
    /// Override this instead of LateUpdate(). Will not be called during scene loading.
    /// </summary>
    protected virtual void SafeLateUpdate() { }
}
