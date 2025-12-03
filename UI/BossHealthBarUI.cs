using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBarUI : MonoBehaviour
{
    public static BossHealthBarUI Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI bossNameText;

    private EnemyController _currentBoss;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Show(EnemyController boss)
    {
        _currentBoss = boss;
        panel.SetActive(true);
        bossNameText.text = boss.Data.enemyName;
        hpSlider.maxValue = boss.Data.baseHp; // Ou boss.currentHp si scalé
        hpSlider.value = boss.currentHp;
    }

    public void Hide()
    {
        _currentBoss = null;
        panel.SetActive(false);
    }

    private void Update()
    {
        if (_currentBoss != null)
        {
            // Mise à jour fluide (Lerp possible ici)
            hpSlider.value = _currentBoss.currentHp;

            if (_currentBoss.currentHp <= 0) Hide();
        }
    }
}