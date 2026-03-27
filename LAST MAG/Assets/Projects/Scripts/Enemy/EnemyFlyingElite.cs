using UnityEngine;

public class EnemyFlyingElite : EnemyBase
{
    [Header("Elite Flying Settings")]
    [SerializeField] private float _hoverHeight = 6f;
    [SerializeField] private float _hoverSpeed = 1.5f;
    [SerializeField] private float _hoverAmplitude = 0.8f;
    [SerializeField] private float _stopRange = 12f;
    [SerializeField] private Transform _muzzle;

    [Header("Audio")]
    [SerializeField] protected AudioSource _audioSource;
    [SerializeField] private AudioClip _fireSound;

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

            Vector3 nextPos = Vector3.MoveTowards(
                transform.position,
                targetPos,
                Stats.MoveSpeed * Time.deltaTime
            );

            Vector3 moveDir = (nextPos - transform.position).normalized;
            float moveDist = Vector3.Distance(transform.position, nextPos);

            if (!Physics.Raycast(transform.position, moveDir, moveDist + 0.5f, LayerMask.GetMask("Ground")))
            {
                transform.position = nextPos;
            }
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

            if (HasLineOfSight())
            {
                FireBurst();
            }
        }
    }

    private bool HasLineOfSight()
    {
        Vector3 eyePos = transform.position + Vector3.up * 0.5f;
        Vector3 dirToPlayer = (PlayerTransform.position + Vector3.up * 0.5f) - eyePos;
        float dist = dirToPlayer.magnitude;

        if (Physics.Raycast(eyePos, dirToPlayer.normalized, out RaycastHit hit, dist))
        {
            if (hit.collider.CompareTag("Player")) return true;
        }
        return false;
    }

    private void FireBurst()
    {
        if (_audioSource && _fireSound)
            _audioSource.PlayOneShot(_fireSound);

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
        base.OnDeath();
    }
}
