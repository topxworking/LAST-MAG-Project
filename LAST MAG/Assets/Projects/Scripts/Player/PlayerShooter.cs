using System.Collections;
using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _muzzle;
    [SerializeField] private Transform _aimTarget;
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private LineRenderer _tracerPrefab;

    [Header("Recoil (Camera)")]
    [SerializeField] private Transform _cameraRoot;
    [SerializeField] private float _recoilUp = 3f;
    [SerializeField] private float _recoilSide = 0.8f;
    [SerializeField] private float _recoilRecovery = 6f;
    [SerializeField] private float _maxRecoilPitch = 20f;

    [Header("Aim Modifier")]
    [SerializeField] private float _aimFireRateMult = 1.3f;
    [SerializeField] private float _aimSpreadMult = 0.35f;

    [Header("Tracer")]
    [SerializeField] private float _tracerDuration = 0.04f;

    [Header("Hit Effect")]
    [SerializeField] private GameObject _hitVFX;
    [SerializeField] private LayerMask _hitLayers = ~0;

    [Header("Crosshair")]
    [SerializeField] private CrosshairController _crosshair;

    [Header("Audio")]
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioClip _shootSound;
    [SerializeField] private AudioClip _reloadSound;
    [SerializeField] private float _originalReloadSoundDuration = 2.0f;

    private PlayerController _controller;

    private PlayerHealth _health;
    private PlayerStats _stats;

    private bool _isShooting;
    private bool _isAiming;
    private bool _isReloading;
    private bool _countdownLock = true;
    private float _nextFireTime;

    private float _currentRecoilPitch;
    private float _currentRecoilYaw;
    private float _recoilPitchTarget;
    private float _recoilYawTarget;
    private int _shotCount;

    private void Awake() => _health = GetComponent<PlayerHealth>();

    private void Start()
    {
        _controller = GetComponent<PlayerController>();

        _stats = _health.Stats;
        EventManager.RaiseAmmoChanged(_stats.CurrentAmmo, _stats.MagazineSize);
    }

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

        if (_recoilPitchTarget > 0f)
        {
            _recoilPitchTarget = Mathf.Max(0f, _recoilPitchTarget - (Time.deltaTime * _recoilRecovery));
        }

        if (!_isShooting) _shotCount = Mathf.Max(0, _shotCount - 1);
        if (_isShooting && Time.time >= _nextFireTime) TryFire();
    }

    public void StartShooting() => _isShooting = true;
    public void StopShooting() { _isShooting = false; _shotCount = 0; }
    public void SetAiming(bool v) => _isAiming = v;

    public void RequestReload()
    {
        if (_isReloading || _stats.CurrentAmmo == _stats.MagazineSize || _countdownLock) return;

        StartCoroutine(ReloadRoutine());
    }

    private void TryFire()
    {
        if (_stats.CurrentAmmo <= 0)
        {
            _isShooting = false;

            if (!_countdownLock)
                StartCoroutine(ReloadRoutine());

            return;
        }

        if (_crosshair != null) _crosshair.OnFire();

        float fireRate = _isAiming ? _stats.FireRate * _aimFireRateMult : _stats.FireRate;

        _nextFireTime = Time.time + fireRate;
        _stats.CurrentAmmo--;
        EventManager.RaiseAmmoChanged(_stats.CurrentAmmo, _stats.MagazineSize);

        Vector3 origin = _aimTarget != null ? _aimTarget.position : _muzzle.position;
        Vector3 dir = GetShootDirection(origin);

        Vector3 hitPoint = origin + dir * 100f;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, 500f, _hitLayers))
        {
            hitPoint = hit.point;

            if (hit.collider.TryGetComponent<EnemyBase>(out var enemy))
                enemy.TakeDamage(_stats.Damage);

            if (_hitVFX != null)
            {
                GameObject fx = Instantiate(
                    _hitVFX,
                    hit.point + hit.normal * 0.02f,
                    Quaternion.LookRotation(hit.normal)
                );
                Destroy(fx, 1.5f);
            }
        }

        StartCoroutine(SpawnTracer(_muzzle.position, hitPoint));

        _sfxSource.PlayOneShot(_shootSound);
        ApplyRecoil();
        _shotCount++;
    }

    private Vector3 GetShootDirection(Vector3 origin)
    {
        Vector3 forward = _aimTarget != null
            ? _aimTarget.forward
            : _muzzle.forward;

        float spreadMult = _isAiming ? _aimSpreadMult : 1f;
        float baseSpread = 0.02f;
        float spraySpread = Mathf.Min(_shotCount * 0.015f, 0.12f);
        float totalSpread = (baseSpread + spraySpread) * spreadMult;

        Vector3 spread = new Vector3(
            Random.Range(-totalSpread, totalSpread),
            Random.Range(-totalSpread, totalSpread),
            0f
        );

        return (forward + spread).normalized;
    }

    private void ApplyRecoil()
    {
        float mult = _isAiming ? 0.5f : 1f;
        float pitchKick = _recoilUp * mult * (1f + _shotCount * 0.04f);
        float yawKick = _recoilSide * mult * Random.Range(-1f, 1f);

        pitchKick = Mathf.Min(pitchKick, _maxRecoilPitch);

        _controller?.AddRecoil(pitchKick, yawKick);

        _recoilPitchTarget += pitchKick;
        _recoilYawTarget += yawKick;
        _recoilPitchTarget = Mathf.Clamp(_recoilPitchTarget, 0f, _maxRecoilPitch);
    }

    private IEnumerator SpawnTracer(Vector3 from, Vector3 to)
    {
        if (_tracerPrefab == null) yield break;

        LineRenderer lr = Instantiate(_tracerPrefab, Vector3.zero, Quaternion.identity);
        lr.SetPosition(0, from);
        lr.SetPosition(1, to);
        lr.enabled = true;

        yield return new WaitForSeconds(_tracerDuration);
        Destroy(lr.gameObject);
    }

    private IEnumerator ReloadRoutine()
    {
        _isReloading = true;
        _isShooting = false;
        _shotCount = 0;

        EventManager.RaiseReloadStarted(_stats.ReloadTime);

        if (_sfxSource != null && _reloadSound != null)
        {
            float targetPitch = _originalReloadSoundDuration / _stats.ReloadTime;
            _sfxSource.pitch = targetPitch;
            _sfxSource.PlayOneShot(_reloadSound);
        }

        yield return new WaitForSeconds(_stats.ReloadTime);
      
        if (_sfxSource != null) _sfxSource.pitch = 1f;

        _stats.CurrentAmmo = _stats.MagazineSize;
        _isReloading = false;

        EventManager.RaiseReloadFinished();
        EventManager.RaiseAmmoChanged(_stats.CurrentAmmo, _stats.MagazineSize);
    }

    private void HandleMenuOpen()
    {
        _isShooting = false;
        _isReloading = false;
        _countdownLock = true;
        _shotCount = 0;
        _nextFireTime = float.MaxValue;
        StopAllCoroutines();

        if (_sfxSource != null)
        {
            _sfxSource.pitch = 1f;
            _sfxSource.Stop();
        }
    }

    private void HandleMenuClose()
    {
        _isShooting = false;
        _nextFireTime = Time.time + 0.5f;
        EventManager.RaiseAmmoChanged(_stats.CurrentAmmo, _stats.MagazineSize);
    }

    private void HandleDied()
    {
        _isShooting = false;
        _isReloading = false;
        _shotCount = 0;
        StopAllCoroutines();
    }

    private void OnCountdownTick(int sec)
    {
        _isShooting = false;
        _countdownLock = true;
    }

    private void OnCountdownDone() => _countdownLock = false;
}
