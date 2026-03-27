using UnityEngine;

public class EnemyFlying : EnemyBase
{
    [Header("Flying Settings")]
    [SerializeField] private float _hoverHeight = 4f;
    [SerializeField] private float _hoverSpeed = 2f;
    [SerializeField] private float _hoverAmplitude = 0.5f;
    [SerializeField] private Transform _muzzle;

    [Header("Audio")]
    [SerializeField] protected AudioSource _audioSource;
    [SerializeField] private AudioClip _fireSound;

    private float _hoverTimer;
    private float _baseY;

    protected override void Awake()
    {
        base.Awake();

        Agent.enabled = false;
    }

    public override void Initialize(EnemyStats stats, Transform player)
    {
        base.Initialize(stats, player);
        _baseY = transform.position.y + _hoverHeight;
    }

    protected override void Update()
    {
        if (PlayerTransform == null || Stats == null) return;

        _hoverTimer += Time.deltaTime * _hoverSpeed;
        float targetY = _baseY + Mathf.Sin(_hoverTimer) * _hoverAmplitude;

        Vector3 toPlayer = PlayerTransform.position - transform.position;
        toPlayer.y = 0f;

        Vector3 targetPos = new Vector3(
            PlayerTransform.position.x,
            targetY,
            PlayerTransform.position.z
        );

        float stopRange = Stats.DetectRange * 0.5f;
        if (DistanceToPlayerPublic > stopRange)
        {
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

        if (toPlayer != Vector3.zero)
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(toPlayer),
                Time.deltaTime * 6f
            );

        DistanceToPlayerPublic = Vector3.Distance(transform.position, PlayerTransform.position);

        _attackTimer += Time.deltaTime;
        if (_attackTimer >= Stats.AttackRate)
        {
            _attackTimer = 0f;

            PerformRangedAttack();
        }
    }

    private float DistanceToPlayerPublic;
    private float _attackTimer;

    private void PerformRangedAttack()
    {
        if (PlayerTransform == null) return;

        Vector3 eyePos = transform.position + Vector3.up * 0.5f;
        Vector3 dirToPlayer = (PlayerTransform.position + Vector3.up * 0.5f) - eyePos;

        if (Physics.Raycast(eyePos, dirToPlayer, out RaycastHit hit, Stats.DetectRange))
        {
            if (!hit.collider.CompareTag("Player"))
            {
                return;
            }
        }

        if (PoolManager.instance == null) return;

        if (_fireSound != null)
        {
            _audioSource.PlayOneShot(_fireSound);
        }

        Transform firePoint = _muzzle != null ? _muzzle : transform;

        Vector3 aimPoint = PlayerTransform.position + Vector3.up * 0.5f;
        Vector3 dir = (aimPoint - firePoint.position).normalized;

        Bullet b = PoolManager.instance.GetBullet(firePoint.position, Quaternion.LookRotation(dir));
        if (b != null)
            b.Initialize(Stats.Damage * 0.6f, 18f, Stats.DetectRange, true);
    }

    public override void OnDeath()
    {
        base.OnDeath();
    }
}