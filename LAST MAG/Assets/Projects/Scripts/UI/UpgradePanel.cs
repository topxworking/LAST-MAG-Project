using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradePanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private TextMeshProUGUI _pointsText;
    [SerializeField] private TextMeshProUGUI _waveText;

    [Header("Stat Labels")]
    [SerializeField] private TextMeshProUGUI _healthLabel;
    [SerializeField] private TextMeshProUGUI _speedLabel;
    [SerializeField] private TextMeshProUGUI _damageLabel;
    [SerializeField] private TextMeshProUGUI _fireRateLabel;
    [SerializeField] private TextMeshProUGUI _reloadSpeedLabel;

    [Header("Upgrade Buttons")]
    [SerializeField] private Button _upgradeHealthBtn;
    [SerializeField] private Button _upgradeSpeedBtn;
    [SerializeField] private Button _upgradeDamageBtn;
    [SerializeField] private Button _upgradeFireRateBtn;
    [SerializeField] private Button _upgradeReloadSpeedBtn;
    [SerializeField] private Button _continueBtn;

    private PlayerHealth _playerHealth;
    private PlayerStats _stats;
    private int _pointsEarnedThisWave = 2;

    private void Awake()
    {
        _panel.SetActive(false);

        _upgradeHealthBtn.onClick.AddListener(UpgradeHealth);
        _upgradeSpeedBtn.onClick.AddListener(UpgradeSpeed);
        _upgradeDamageBtn.onClick.AddListener(UpgradeDamage);
        _upgradeFireRateBtn.onClick.AddListener(UpgradeFireRate);
        _upgradeReloadSpeedBtn.onClick.AddListener(UpgradeJump);
        _continueBtn.onClick.AddListener(Close);
    }

    private void Start()
    {
        _playerHealth = FindFirstObjectByType<PlayerHealth>();
        _stats        = _playerHealth.Stats;
    }

    public void Show()
    {
        if (_stats == null)
        {
            var ph = FindFirstObjectByType<PlayerHealth>();
            if (ph != null) _stats = ph.Stats;
        }

        _stats.UpgradePoints += _pointsEarnedThisWave;
        _panel.SetActive(true);
        RefreshUI();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Close()
    {
        _panel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
        EventManager.RaiseUpgradeMenuClosed();
    }

    private void UpgradeHealth()
    {
        _stats.UpgradeHealth();
        EventManager.RaisePlayerHealthChanged(_stats.CurrentHealth, _stats.MaxHealth);
        RefreshUI();
    }
    private void UpgradeSpeed()    { _stats.UpgradeSpeed(); RefreshUI(); }
    private void UpgradeDamage()   { _stats.UpgradeDamage(); RefreshUI(); }
    private void UpgradeFireRate() { _stats.UpgradeFireRate(); RefreshUI(); }
    private void UpgradeJump()     { _stats.UpgradeReloadSpeed(); RefreshUI(); }

    private void RefreshUI()
    {
        int wave = GameManager.instance != null ? GameManager.instance.CurrentWave : 0;
        _waveText.text    = $"Wave {wave} Complete!";
        _pointsText.text  = $"{_stats.UpgradePoints:D2}";

        _healthLabel.text = $"{_stats.MaxHealth:F0}";
        _speedLabel.text = $"{_stats.MoveSpeed:F1}";
        _damageLabel.text = $"{_stats.Damage:F0}";
        _fireRateLabel.text = $"{(1f / _stats.FireRate):F1}/s";
        _reloadSpeedLabel.text = $"{_stats.ReloadTime:F1}s";

        bool hasPoints = _stats.UpgradePoints > 0;
        _upgradeHealthBtn.interactable = hasPoints;
        _upgradeSpeedBtn.interactable = hasPoints;
        _upgradeDamageBtn.interactable = hasPoints;
        _upgradeFireRateBtn.interactable = hasPoints;
        _upgradeReloadSpeedBtn.interactable = hasPoints;
    }
}
