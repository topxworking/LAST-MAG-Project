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
    [SerializeField] private TextMeshProUGUI _finalKillsText;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _mainMenuButton;

    [Header("Settings UI")]
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private Slider _sensSlider;
    [SerializeField] private Slider _volumeSlider;

    [Header("Menu Groups")]
    [SerializeField] private GameObject _pauseMenuButtonsRoot;

    private Coroutine _reloadBarCoroutine;

    [Header("Countdown")]
    [SerializeField] private GameObject _countdownRoot;
    [SerializeField] private TextMeshProUGUI _countdownText;

    private void Awake()
    {
        _gameOverPanel.SetActive(false);
        _bossBarRoot.SetActive(false);
        _waveAnnouncementText.text = "";
        if (_reloadRoot) _reloadRoot.SetActive(false);      

        _restartButton.onClick.AddListener(() => GameManager.instance?.RestartGame());
        _mainMenuButton.onClick.AddListener(() => GameManager.instance?.MainMenu());
    }

    private void Start()
    {
        StartCoroutine(SetupSlidersDelayed());
    }

    private IEnumerator SetupSlidersDelayed()
    {
        yield return null;

        if (SettingsManager.instance != null)
        {
            if (_sensSlider)
            {
                _sensSlider.minValue = 0.1f;
                _sensSlider.maxValue = 5f;
                _sensSlider.value = SettingsManager.instance.MouseSensitivity;
                _sensSlider.onValueChanged.RemoveAllListeners();
                _sensSlider.onValueChanged.AddListener(val => SettingsManager.instance.SetMouseSensitivity(val));
            }

            if (_volumeSlider)
            {
                float savedVol = PlayerPrefs.GetFloat("MasterVol", 0.75f);
                _volumeSlider.minValue = 0.0001f;
                _volumeSlider.maxValue = 1f;
                _volumeSlider.value = savedVol;
                _volumeSlider.onValueChanged.RemoveAllListeners();
                _volumeSlider.onValueChanged.AddListener(val => SettingsManager.instance.SetMasterVolume(val));
            }
        }
        else
        {
            Debug.LogError("UIManager: SettingsManager.instance is null! Make sure SettingsManager is in the scene.");
        }
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
        EventManager.OnCountdownTick += ShowCountdown;
        EventManager.OnCountdownFinished += HideCountdown;
        EventManager.OnBossHealthChanged += UpdateBossHealthBar;
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
        EventManager.OnCountdownTick -= ShowCountdown;
        EventManager.OnCountdownFinished -= HideCountdown;
        EventManager.OnBossHealthChanged -= UpdateBossHealthBar;
    }

    private void UpdateHealthBar(float current, float max)
    {
        float fillAmount = current / max;
        _healthBar.value = fillAmount;
        _healthText.text = $"{Mathf.CeilToInt(current):D3}";
    }

    private void UpdateAmmoDisplay(int current, int max)
    {
        if (_ammoText == null) return;

        _ammoText.text = $"{current:D2}";
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
            _waveText.text = $"{wave:D2}";

        StartCoroutine(ShowWaveAnnouncement(wave));
    }

    Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }

    private IEnumerator ShowWaveAnnouncement(int wave)
    {
        bool isBoss = wave % 10 == 0 && wave > 0;
        _waveAnnouncementText.text  = isBoss ? $"BOSS WAVE {wave} " : $"— WAVE {wave} INITIATED —";
        _waveAnnouncementText.color = isBoss ? Hex("#720A00") : Hex("#E8920A");

        yield return new WaitForSecondsRealtime(2.5f);
        _waveAnnouncementText.text = "";
    }

    private void HandleBossWave(int wave)
    {
        if (_bossBarRoot != null)
        {
            _bossBarRoot.SetActive(true);
            if (_bossNameText) _bossNameText.text = $"BOSS — WAVE {wave}";
            if (_bossHealthBar) _bossHealthBar.value = 1f;
        }
    }

    private void HideBossBar() => _bossBarRoot.SetActive(false);

    private void UpdateEnemyCount(int count)
    {
        if (_enemyCountText) _enemyCountText.text = $"{count:D2}";
    }

    private void UpdateScore(int score)
    {
        if (_scoreText) _scoreText.text = $"{score:N0}".PadLeft(7, '0');
    }

    public void ShowGameOver(int score, int wave, int kills)
    {
        _gameOverPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _finalScoreText.text = $"{score:N0}".PadLeft(7, '0');
        _finalWaveText.text  = $"WAVE {wave:D2}";
        _finalKillsText.text = $"{kills:D2}";
    }

    private void UpdateBossHealthBar(float current, float max)
    {
        if (_bossHealthBar != null)
            _bossHealthBar.value = current / max;
    }

    private void ShowCountdown(int sec)
    {
        if (_countdownRoot) _countdownRoot.SetActive(true);
        if (_countdownText) _countdownText.text = sec.ToString();

        FinishReloadBar();
    }

    private void HideCountdown()
    {
        if (_countdownRoot) _countdownRoot.SetActive(false);
    }

    public void ToggleSettings()
    {
        bool isSettingsOpening = !_settingsPanel.activeSelf;
        _settingsPanel.SetActive(isSettingsOpening);

        if (_pauseMenuButtonsRoot != null)
        {
            _pauseMenuButtonsRoot.SetActive(!isSettingsOpening);
        }
    }
}
