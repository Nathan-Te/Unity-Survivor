using UnityEngine;
using TMPro;

public enum DamageType
{
    Normal,
    Critical,
    Burn,
    Poison // For future expansion
}

public class DamageText : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh; // Si 3D World Space
                                                   // OU [SerializeField] private TextMeshProUGUI textMesh; // Si Canvas Overlay

    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Damage Type Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color criticalColor = Color.red;
    [SerializeField] private Color burnColor = new Color(1f, 0.5f, 0f); // Orange

    private float _timer;
    private Color _startColor;

    private void Awake()
    {
        if (!textMesh) textMesh = GetComponent<TextMeshPro>();
        _startColor = textMesh.color;
    }

    public void Initialize(float damage, Vector3 position, DamageType damageType = DamageType.Normal)
    {
        transform.position = position + Vector3.up * 1.5f; // Un peu au dessus
        transform.localScale = Vector3.one; // Reset taille

        textMesh.text = Mathf.RoundToInt(damage).ToString();

        // Set color based on damage type
        switch (damageType)
        {
            case DamageType.Critical:
                textMesh.color = criticalColor;
                transform.localScale = Vector3.one * 1.5f; // Plus gros
                break;
            case DamageType.Burn:
                textMesh.color = burnColor;
                break;
            case DamageType.Normal:
            default:
                textMesh.color = normalColor;
                break;
        }

        textMesh.alpha = 1f;

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