using UnityEngine;

public class EnemyFlyingElite : EnemyBase
{
    [Header("Elite Flying Settings")]
    [SerializeField] private float _hoverHeight = 6f;
    [SerializeField] private float _hoverSpeed = 1.5f;
    [SerializeField] private float _hoverAmplitude = 0.8f;
    [SerializeField] private float _stopRange = 12f;
    [SerializeField] private Transform _muzzle;

    private float _hoverTimer;
    private float _baseY;
    private float _attackTimer;
    private float _distToPlayer;

    protected override void Awake()
    {
        base.Awake();
        Agent.enabled = false;
    }

    public override void Initialize(EnemyStats stats, Transform player)
    {
        Stats = stats;
        PlayerTransform = player;
        _baseY = transform.position.y + _hoverHeight;
        _attackTimer = 0f;
    }

    protected override void Update()
    {
        if (PlayerTransform == null || Stats == null) return;

        _hoverTimer += Time.deltaTime * _hoverSpeed;
        _distToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

        float targetY = _baseY + Mathf.Sin(_hoverTimer) * _hoverAmplitude;

        if (_distToPlayer > _stopRange)
        {
            Vector3 targetPos = new Vector3(
                PlayerTransform.position.x,
                targetY,
                PlayerTransform.position.z
            );
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                Stats.MoveSpeed * Time.deltaTime
            );
        }
        else
        {
            transform.position = new Vector3(
                transform.position.x,
                targetY,
                transform.position.z
            );
        }

        Vector3 dir = (PlayerTransform.position - transform.position);
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                Time.deltaTime * 5f
            );

        _attackTimer += Time.deltaTime;
        if (_attackTimer >= Stats.AttackRate)
        {
            _attackTimer = 0f;
            FireBurst();
        }
    }

    private void FireBurst()
    {
        if (PoolManager.instance == null) return;

        Transform firePoint = _muzzle != null ? _muzzle : transform;
        Vector3 aimPoint = PlayerTransform.position + Vector3.up * 0.5f;

        float[] spreadAngles = { -6f, 0f, 6f };

        foreach (float angle in spreadAngles)
        {
            Vector3 baseDir = (aimPoint - firePoint.position).normalized;
            Vector3 spreadDir = Quaternion.Euler(0f, angle, 0f) * baseDir;
            Bullet b = PoolManager.instance.GetBullet(
                                     firePoint.position,
                                     Quaternion.LookRotation(spreadDir));
            if (b != null)
                b.Initialize(Stats.Damage, 20f, Stats.DetectRange, true);
        }
    }

    public override void OnDeath()
    {
        if (GameManager.instance != null && Stats != null)
        {
            GameManager.instance.RegisterKill(EnemyType.FlyingElite);
        }

        base.OnDeath();

        EventManager.RaiseEnemyKilled(Stats.ScoreValue);
        WaveManager.instance?.RegisterEnemyKilled();
        Destroy(gameObject, 1.5f);
    }
}
