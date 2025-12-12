using System.Collections.Generic;
using UnityEngine;

public class DamageTextPool : Singleton<DamageTextPool>
{
    [SerializeField] private DamageText textPrefab;
    [SerializeField] private int initialSize = 50;

    // NOUVEAU : Limite stricte pour éviter l'explosion mémoire (+37k objets)
    [SerializeField] private int maxActiveTexts = 100;

    private Queue<DamageText> _pool = new Queue<DamageText>();
    private List<DamageText> _activeTexts = new List<DamageText>(); // Pour le nettoyage

    protected override void Awake()
    {
        base.Awake();

        if (Instance == this)
        {
            // ⭐ CORRECTION : Créer ET ajouter au pool directement
            for (int i = 0; i < initialSize; i++)
            {
                DamageText t = Instantiate(textPrefab, transform);
                t.gameObject.SetActive(false);
                _pool.Enqueue(t);
            }
        }
    }

    // ⭐ AJOUT : Nettoyage à la destruction
    protected override void OnDestroy()
    {
        _pool?.Clear();
        _activeTexts?.Clear();

        base.OnDestroy();
    }

    private DamageText CreateNew()
    {
        DamageText t = Instantiate(textPrefab, transform);
        t.gameObject.SetActive(false);
        return t; // On ne l'ajoute pas à la queue ici, c'est fait via ReturnToPool ou Init
    }

    // Initialisation correcte au démarrage
    private void Start()
    {
        // Assurez-vous que la queue est remplie au départ
        if (_pool.Count == 0)
        {
            for (int i = 0; i < initialSize; i++)
            {
                DamageText t = CreateNew();
                _pool.Enqueue(t);
            }
        }
    }

    public void Spawn(float damage, Vector3 position, bool isCritical = false)
    {
        // 1. SÉCURITÉ : Si on a déjà trop de textes actifs, on recycle le plus vieux !
        if (_activeTexts.Count >= maxActiveTexts)
        {
            DamageText oldText = _activeTexts[0];
            _activeTexts.RemoveAt(0);
            oldText.Initialize(damage, position, isCritical);
            _activeTexts.Add(oldText);
            return;
        }

        // 2. Comportement normal
        DamageText t;
        if (_pool.Count > 0)
        {
            t = _pool.Dequeue();
        }
        else
        {
            t = CreateNew();
        }

        t.Initialize(damage, position, isCritical);
        _activeTexts.Add(t);
    }

    public void ReturnToPool(DamageText t)
    {
        if (_activeTexts.Contains(t))
        {
            _activeTexts.Remove(t);
        }

        t.gameObject.SetActive(false);
        _pool.Enqueue(t);
    }

    public void ClearAll()
    {
        // Désactiver tous les textes actifs
        foreach (var text in _activeTexts)
        {
            if (text != null)
            {
                text.gameObject.SetActive(false);
                _pool.Enqueue(text);
            }
        }
        _activeTexts.Clear();

        Debug.Log($"[DamageTextPool] {_pool.Count} textes nettoyés");
    }
}