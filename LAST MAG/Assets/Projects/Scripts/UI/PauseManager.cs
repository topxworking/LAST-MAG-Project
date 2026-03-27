using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance { get; private set; }
    public bool IsPaused { get; private set; }

    [Header("References")]
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private GameObject _pausePanel;

    private void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;

        Time.timeScale = 1f;
        IsPaused = false;
        if (_pausePanel != null) _pausePanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (_inputReader != null)
        {
            _inputReader.OnPauseStarted += TogglePause;
        }
    }

    private void OnDisable()
    {
        if (_inputReader != null)
        {
            _inputReader.OnPauseStarted -= TogglePause;
        }
    }

    public void TogglePause()
    {
        if (GameManager.instance != null)
        {
            if (GameManager.instance.CurrentState == GameState.GameOver ||
                GameManager.instance.CurrentState == GameState.Upgrading)
                return;
        }

        IsPaused = !IsPaused;

        Time.timeScale = IsPaused ? 0f : 1f;
        Cursor.lockState = IsPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = IsPaused;

        if (_pausePanel != null)
            _pausePanel.SetActive(IsPaused);

        if (IsPaused)
            _inputReader.DisablePlayerInput();
        else
            _inputReader.EnablePlayerInput();
    }

    public void Resume()
    {
        if (IsPaused) TogglePause();
    }
}
