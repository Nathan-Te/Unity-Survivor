using UnityEngine;

public class FloatingAnimation : MonoBehaviour
{
    [Header("Mouvement (Bobbing)")]
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float floatHeight = 0.5f;

    [Header("Rotation")]
    [SerializeField] private float rotateSpeed = 50f;

    [Header("Scale (Pulsation)")]
    [SerializeField] private float pulseSpeed = 0f; // Mettre 0 pour désactiver
    [SerializeField] private float pulseAmount = 0.2f;

    private Vector3 _startPos;
    private Vector3 _startScale;

    private void Start()
    {
        _startPos = transform.localPosition;
        _startScale = transform.localScale;
    }

    private void Update()
    {
        // Position (Haut/Bas)
        float newY = _startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.localPosition = new Vector3(_startPos.x, newY, _startPos.z);

        // Rotation
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);

        // Pulsation
        if (pulseSpeed > 0)
        {
            float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = _startScale * scale;
        }
    }
}