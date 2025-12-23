using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBarUI : Singleton<BossHealthBarUI>
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI bossNameText;

    private EnemyController _currentBoss;

    protected override void Awake()
    {
        base.Awake();

        // Always initialize to default state (hidden)
        // This ensures proper initialization even after scene reload
        if (panel != null)
        {
            panel.SetActive(false);
        }
        _currentBoss = null;
    }

    public void Show(EnemyController boss)
    {
        _currentBoss = boss;
        panel.SetActive(true);
        bossNameText.text = boss.Data.enemyName;
        hpSlider.maxValue = boss.Data.baseHp; // Ou boss.currentHp si scal�
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
            // Mise � jour fluide (Lerp possible ici)
            hpSlider.value = _currentBoss.currentHp;

            if (_currentBoss.currentHp <= 0) Hide();
        }
    }
}