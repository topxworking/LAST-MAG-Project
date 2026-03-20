using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private Slider _healthBar;
    [SerializeField] private TextMeshProUGUI _healthText;

    [Header("Ammo")]
    [SerializeField] private TextMeshProUGUI _ammoText;
    [SerializeField] private GameObject _reloadRoot;
    [SerializeField] private Slider _reloadBar;
    [SerializeField] private TextMeshProUGUI _reloadLabel;

    [Header("Wave & Score")]
    [SerializeField] private TextMeshProUGUI _waveText;
    [SerializeField] private TextMeshProUGUI _enemyCountText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _waveAnnouncementText;

    [Header("Boss Bar")]
    [SerializeField] private GameObject _bossBarRoot;
    [SerializeField] private Slider _bossHealthBar;
    [SerializeField] private TextMeshProUGUI _bossNameText;

    [Header("Game Over")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private TextMeshProUGUI _finalScoreText;
    [SerializeField] private TextMeshProUGUI _finalWaveText;
    [SerializeField] private Button _restartButton;

    private Coroutine _reloadBarCoroutine;

    [Header("Crosshair")]
    [SerializeField] private Image _crosshairDot;
    [SerializeField] private Image _crosshairAimRing;

    private void Awake()
    {
        _gameOverPanel.SetActive(false);
        _bossBarRoot.SetActive(false);
        _waveAnnouncementText.text = "";
        if (_reloadRoot) _reloadRoot.SetActive(false);

        _restartButton.onClick.AddListener(() => GameManager.instance?.RestartGame());
    }

    private void OnEnable()
    {
        EventManager.OnPlayerHealthChanged += UpdateHealthBar;
        EventManager.OnAmmoChanged += UpdateAmmoDisplay;
        EventManager.OnReloadStarted += StartReloadBar;
        EventManager.OnReloadFinished += FinishReloadBar;
        EventManager.OnWaveStarted += UpdateWaveDisplay;
        EventManager.OnBossWaveStarted += HandleBossWave;
        EventManager.OnBossDefeated += HideBossBar;
        EventManager.OnEnemyCountChanged += UpdateEnemyCount;
        EventManager.OnScoreChanged += UpdateScore;
    }

    private void OnDisable()
    {
        EventManager.OnPlayerHealthChanged -= UpdateHealthBar;
        EventManager.OnAmmoChanged -= UpdateAmmoDisplay;
        EventManager.OnReloadStarted -= StartReloadBar;
        EventManager.OnReloadFinished -= FinishReloadBar;
        EventManager.OnWaveStarted -= UpdateWaveDisplay;
        EventManager.OnBossWaveStarted -= HandleBossWave;
        EventManager.OnBossDefeated -= HideBossBar;
        EventManager.OnEnemyCountChanged -= UpdateEnemyCount;
        EventManager.OnScoreChanged -= UpdateScore;
    }

    private void UpdateHealthBar(float current, float max)
    {
        float fillAmount = current / max;
        _healthBar.value = fillAmount;
        _healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    private void UpdateAmmoDisplay(int current, int max)
    {
        if (_ammoText == null) return;

        _ammoText.text = $"{current} / {max}";
        _ammoText.color = current <= 5 ? Color.red
                        : current <= 10 ? new Color(1f, 0.6f, 0f)
                        : Color.white;
    }

    private void StartReloadBar(float duration)
    {
        if (_reloadRoot == null) return;
        _reloadRoot.SetActive(true);
        if (_reloadLabel) _reloadLabel.text = "RELOADING...";

        if (_reloadBarCoroutine != null) StopCoroutine(_reloadBarCoroutine);
        _reloadBarCoroutine = StartCoroutine(AnimateReloadBar(duration));
    }

    private IEnumerator AnimateReloadBar(float duration)
    {
        float elapsed = 0f;
        if (_reloadBar) _reloadBar.value = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (_reloadBar) _reloadBar.value = elapsed / duration;
            yield return null;
        }
        if (_reloadBar) _reloadBar.value = 1f;
    }

    private void FinishReloadBar()
    {
        if (_reloadBarCoroutine != null) StopCoroutine(_reloadBarCoroutine);
        if (_reloadRoot) _reloadRoot.SetActive(false);
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
        if (_bossNameText) _bossNameText.text = $"BOSS — Wave {wave}";
        if (_bossHealthBar) _bossHealthBar.value = 1f;
    }

    private void HideBossBar() => _bossBarRoot.SetActive(false);

    private void UpdateEnemyCount(int count)
    {
        if (_enemyCountText) _enemyCountText.text = $"Enemies: {count}";
    }

    private void UpdateScore(int score)
    {
        if (_scoreText) _scoreText.text = $"Score: {score:N0}";
    }

    public void ShowGameOver(int score, int wave)
    {
        _gameOverPanel.SetActive(true);
        _finalScoreText.text = $"Score: {score:N0}";
        _finalWaveText.text  = $"Survived to Wave {wave}";
    }

    public void UpdateBossHealthBar(float current, float max)
    {
        if (_bossHealthBar != null)
            _bossHealthBar.value = current / max;
    }
}
