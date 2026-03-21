using System.Collections;
using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _muzzle;
    [SerializeField] private Transform _aimTarget;

    [Header("Reload SFX")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _reloadClip;
    [SerializeField] private AudioClip _emptyClip;

    private PlayerHealth _health;
    private PlayerStats _stats;

    private bool _isShooting;
    private bool _isReloading;
    private bool _countdownLock = true;
    private float _nextFireTime;

    private void Awake()
    {
        _health = GetComponent<PlayerHealth>();
    }

    private void Start()
    {
        _stats = _health.Stats;
        EventManager.RaiseAmmoChanged(_stats.CurrentAmmo, _stats.MagazineSize);
    }

    private void OnCountdownTick(int sec) => _countdownLock = true;
    private void OnCountdownDone() => _countdownLock = false;

    private void OnEnable()
    {
        EventManager.OnUpgradeMenuOpened += HandleMenuOpen;
        EventManager.OnUpgradeMenuClosed += HandleMenuClose;
        EventManager.OnPlayerDied += HandleDied;
        EventManager.OnCountdownTick += OnCountdownTick;
        EventManager.OnCountdownFinished += OnCountdownDone;
    }

    private void OnDisable()
    {
        EventManager.OnUpgradeMenuOpened -= HandleMenuOpen;
        EventManager.OnUpgradeMenuClosed -= HandleMenuClose;
        EventManager.OnPlayerDied -= HandleDied;
        EventManager.OnCountdownTick -= OnCountdownTick;
        EventManager.OnCountdownFinished -= OnCountdownDone;
    }

    private void Update()
    {
        if (_isReloading || _countdownLock) return;
        if (_isShooting && Time.time >= _nextFireTime)
            TryFire();
    }

    public void StartShooting() => _isShooting = true;
    public void StopShooting() => _isShooting = false;

    public void SetAiming(bool v) { }

    public void RequestReload()
    {
        if (_isReloading) return;
        if (_stats.CurrentAmmo == _stats.MagazineSize) return;
        StartCoroutine(ReloadRoutine());
    }

    private void TryFire()
    {
        if (_stats.CurrentAmmo <= 0)
        {
            PlaySound(_emptyClip);
            _isShooting = false;
            StartCoroutine(ReloadRoutine());
            return;
        }

        _nextFireTime = Time.time + _stats.FireRate;
        _stats.CurrentAmmo--;
        EventManager.RaiseAmmoChanged(_stats.CurrentAmmo, _stats.MagazineSize);

        SpawnBullet();
    }

    private void SpawnBullet()
    {
        Vector3 direction = _muzzle.forward;

        if (_aimTarget != null)
        {
            Ray ray = new Ray(_aimTarget.position, _aimTarget.forward);
            Vector3 targetPoint = ray.GetPoint(_stats.BulletRange);

            if (Physics.Raycast(ray, out RaycastHit hit, _stats.BulletRange))
                targetPoint = hit.point;

            direction = (targetPoint - _muzzle.position).normalized;
        }

        Bullet bullet = PoolManager.instance.GetBullet(
            _muzzle.position, Quaternion.LookRotation(direction));
        bullet.Initialize(_stats.Damage, _stats.BulletSpeed, _stats.BulletRange, false);
    }

    private IEnumerator ReloadRoutine()
    {
        _isReloading = true;
        _isShooting = false;

        EventManager.RaiseReloadStarted(_stats.ReloadTime);
        PlaySound(_reloadClip);

        yield return new WaitForSeconds(_stats.ReloadTime);

        _stats.CurrentAmmo = _stats.MagazineSize;
        _isReloading = false;

        EventManager.RaiseReloadFinished();
        EventManager.RaiseAmmoChanged(_stats.CurrentAmmo, _stats.MagazineSize);
    }

    private void HandleMenuOpen()
    {
        _isShooting = false;
        StopAllCoroutines();
        _isReloading = false;

        _nextFireTime = float.MaxValue;
    }

    private void HandleMenuClose()
    {
        _isShooting = false;
        _isReloading = false;
        _nextFireTime = Time.time + 0.5f;

        EventManager.RaiseAmmoChanged(_stats.CurrentAmmo, _stats.MagazineSize);
    }

    private void HandleDied()
    {
        _isShooting = false;
        _isReloading = false;
        StopAllCoroutines();
    }

    private void PlaySound(AudioClip clip)
    {
        if (_audioSource != null && clip != null)
            _audioSource.PlayOneShot(clip);
    }
}
