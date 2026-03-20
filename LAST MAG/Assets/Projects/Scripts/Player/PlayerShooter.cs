using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _muzzle;
    [SerializeField] private Transform _aimTarget;

    private PlayerHealth _health;
    private PlayerStats _stats;

    private bool _isShooting;
    private bool _isAiming;
    private float _nextFireTime;

    private void Awake()
    {
        _health = GetComponent<PlayerHealth>();
    }

    private void Start()
    {
        _stats = _health.Stats;
    }

    private void Update()
    {
        if (_isShooting && Time.time > _nextFireTime)
            Fire();
    }

    public void StartShooting() => _isShooting = true;
    public void StopShooting() => _isShooting = false;
    public void SetAiming(bool v) => _isAiming = v;

    private void Fire()
    {
        _nextFireTime = Time.time + _stats.FireRate;

        Vector3 direction = _muzzle.forward;
        if (_aimTarget != null )
        {
            Ray ray = new Ray(_aimTarget.position, _aimTarget.forward);
            Vector3 targetPoint = ray.GetPoint(_stats.BulletRange);

            if (Physics.Raycast(ray, out RaycastHit hit, _stats.BulletRange))
                targetPoint = hit.point;

            direction = (targetPoint - _muzzle.position).normalized;
        }

        Quaternion rot = Quaternion.LookRotation(direction);
        Bullet bullet = PoolManager.instance.GetBullet(_muzzle.position, rot);
        bullet.Initialize(_stats.Damage, _stats.BulletSpeed, _stats.BulletRange, false);
    }
}
