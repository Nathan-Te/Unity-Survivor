using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh; // Si 3D World Space
                                                   // OU [SerializeField] private TextMeshProUGUI textMesh; // Si Canvas Overlay

    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float fadeDuration = 1f;

    private float _timer;
    private Color _startColor;

    private void Awake()
    {
        if (!textMesh) textMesh = GetComponent<TextMeshPro>();
        _startColor = textMesh.color;
    }

    public void Initialize(float damage, Vector3 position, bool isCritical = false)
    {
        transform.position = position + Vector3.up * 1.5f; // Un peu au dessus
        transform.localScale = Vector3.one; // Reset taille

        textMesh.text = Mathf.RoundToInt(damage).ToString();
        textMesh.color = isCritical ? Color.red : Color.white; // Couleur critique
        textMesh.alpha = 1f;

        if (isCritical) transform.localScale = Vector3.one * 1.5f; // Plus gros

        _timer = fadeDuration;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        // Mouvement vers le haut
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

        // Fade Out
        _timer -= Time.deltaTime;
        float alpha = _timer / fadeDuration;
        textMesh.alpha = alpha;

        if (_timer <= 0f)
        {
            DamageTextPool.Instance.ReturnToPool(this);
        }
    }
}