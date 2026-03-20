using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private Slider          _healthBar;
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private TextMeshProUGUI _waveText;
    [SerializeField] private TextMeshProUGUI _enemyCountText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _waveAnnouncementText;

    [Header("Boss Bar")]
    [SerializeField] private GameObject      _bossBarRoot;
    [SerializeField] private Slider          _bossHealthBar;
    [SerializeField] private TextMeshProUGUI _bossNameText;

    [Header("Game Over")]
    [SerializeField] private GameObject      _gameOverPanel;
    [SerializeField] private TextMeshProUGUI _finalScoreText;
    [SerializeField] private TextMeshProUGUI _finalWaveText;
    [SerializeField] private Button          _restartButton;

    [Header("Crosshair")]
    [SerializeField] private Image           _crosshairDot;
    [SerializeField] private Image           _crosshairAimRing;

    private void Awake()
    {
        _gameOverPanel.SetActive(false);
        _bossBarRoot.SetActive(false);
        _waveAnnouncementText.text = "";

        _restartButton.onClick.AddListener(() => GameManager.instance?.RestartGame());
    }

    private void OnEnable()
    {
        EventManager.OnPlayerHealthChanged  += UpdateHealthBar;
        EventManager.OnWaveStarted          += UpdateWaveDisplay;
        EventManager.OnBossWaveStarted      += HandleBossWave;
        EventManager.OnBossDefeated         += HideBossBar;
        EventManager.OnEnemyCountChanged    += UpdateEnemyCount;
        EventManager.OnScoreChanged         += UpdateScore;
        EventManager.OnUpgradeMenuOpened    += () => _crosshairDot?.gameObject.SetActive(false);
        EventManager.OnUpgradeMenuClosed    += () => _crosshairDot?.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        EventManager.OnPlayerHealthChanged  -= UpdateHealthBar;
        EventManager.OnWaveStarted          -= UpdateWaveDisplay;
        EventManager.OnBossWaveStarted      -= HandleBossWave;
        EventManager.OnBossDefeated         -= HideBossBar;
        EventManager.OnEnemyCountChanged    -= UpdateEnemyCount;
        EventManager.OnScoreChanged         -= UpdateScore;
        EventManager.OnUpgradeMenuOpened -= () => _crosshairDot?.gameObject.SetActive(false);
        EventManager.OnUpgradeMenuClosed -= () => _crosshairDot?.gameObject.SetActive(true);
    }

    private void UpdateHealthBar(float current, float max)
    {
        float fillAmount = current / max;
        _healthBar.value = fillAmount;
        _healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";

        Debug.Log($"Health Updated: {current}/{max} (Fill: {fillAmount})");
    }

    private void UpdateWaveDisplay(int wave)
    {
        if (_waveText != null)
            _waveText.text = $"Wave {wave}";

        StartCoroutine(ShowWaveAnnouncement(wave));
    }

    private IEnumerator ShowWaveAnnouncement(int wave)
    {
        bool isBoss = wave % 10 == 0 && wave > 0;
        _waveAnnouncementText.text  = isBoss ? $"BOSS WAVE {wave} " : $"— Wave {wave} —";
        _waveAnnouncementText.color = isBoss ? Color.red : Color.white;

        yield return new WaitForSecondsRealtime(2.5f);
        _waveAnnouncementText.text = "";
    }

    private void HandleBossWave(int wave)
    {
        _bossBarRoot.SetActive(true);
        if (_bossNameText != null)
            _bossNameText.text = $"BOSS — Wave {wave}";
        _bossHealthBar.value = 1f;
    }

    private void HideBossBar() => _bossBarRoot.SetActive(false);

    private void UpdateEnemyCount(int count)
    {
        _enemyCountText.text = $"Enemies: {count}";
    }

    private void UpdateScore(int score)
    {
        _scoreText.text = $"Score: {score:N0}";
    }

    public void ShowGameOver(int score, int wave)
    {
        _gameOverPanel.SetActive(true);
        _finalScoreText.text = $"Score: {score:N0}";
        _finalWaveText.text  = $"Survived to Wave {wave}";
    }
}
