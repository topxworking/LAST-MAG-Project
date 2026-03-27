using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Playing, Upgrading, BossFight, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.Playing;

    public int TotalKills { get; private set; }
    public int MeleeKills { get; private set; }
    public int RangedKills { get; private set; }
    public int FlyingKills { get; private set; }
    public int BossKills { get; private set; }

    public int Score { get; private set; }
    public int CurrentWave { get; private set; }
    public bool IsBossWave  => CurrentWave % 10 == 0 && CurrentWave > 0;

    [Header("References")]
    [SerializeField] private WaveManager   _waveManager;
    [SerializeField] private UIManager     _uiManager;
    [SerializeField] private UpgradePanel  _upgradePanel;
    [SerializeField] private InputReader _inputReader;

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
    }

    private void OnEnable()
    {
        EventManager.OnEnemyKilled    += AddScore;
        EventManager.OnWaveCompleted  += HandleWaveCompleted;
        EventManager.OnBossDefeated   += HandleBossDefeated;
        EventManager.OnPlayerDied     += HandlePlayerDied;
        EventManager.OnUpgradeMenuClosed += ResumeAfterUpgrade;
    }

    private void OnDisable()
    {
        EventManager.OnEnemyKilled -= AddScore;
        EventManager.OnWaveCompleted -= HandleWaveCompleted;
        EventManager.OnBossDefeated -= HandleBossDefeated;
        EventManager.OnPlayerDied -= HandlePlayerDied;
        EventManager.OnUpgradeMenuClosed -= ResumeAfterUpgrade;
    }

    private void Start()
    {
        if (_waveManager == null)
            _waveManager = FindFirstObjectByType<WaveManager>();

        CurrentWave = 0;
        Score = 0;
        StartNextWave();
    }

    public void RegisterKill(EnemyType type)
    {
        TotalKills++;
        switch (type)
        {
            case EnemyType.Melee: MeleeKills++; break;
            case EnemyType.Ranged: RangedKills++; break;
            case EnemyType.Flying: FlyingKills++; break;
            case EnemyType.FlyingElite: FlyingKills++; break;
            case EnemyType.Boss: BossKills++; break;
        }
    }

    private void StartNextWave()
    {
        CurrentWave++;
        if (IsBossWave)
        {
            CurrentState = GameState.BossFight;
            EventManager.RaiseBossWaveStarted(CurrentWave);
        }
        else
        {
            CurrentState = GameState.Playing;
        }
        _waveManager.StartWave(CurrentWave);
        EventManager.RaiseWaveStarted(CurrentWave);
    }

    private void HandleWaveCompleted(int wave)
    {
        CurrentState = GameState.Upgrading;
        Time.timeScale = 0f;

        if (_upgradePanel != null)
        {
            _upgradePanel.Show();
            EventManager.RaiseUpgradeMenuOpened();
        }
        else
        {
            Time.timeScale = 1f;
            StartNextWave();
        }
    }

    private void HandleBossDefeated()
    {
        AddScore(500);       
    }

    private void ResumeAfterUpgrade()
    {
        Time.timeScale = 1f;
        StartNextWave();
    }

    private void HandlePlayerDied()
    {
        CurrentState = GameState.GameOver;
        Time.timeScale = 0f;
        _uiManager.ShowGameOver(Score, CurrentWave, TotalKills);
    }

    private void AddScore(int value)
    {
        Score += value;
        EventManager.RaiseScoreChanged(Score);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        if (_inputReader != null)
        {
            _inputReader.EnablePlayerInput();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        EventManager.ClearAllEvents();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;

        if (_inputReader != null)
        {
            _inputReader.EnablePlayerInput();
        }

        EventManager.ClearAllEvents();
        SceneManager.LoadScene("MainMenu");
    }
}
