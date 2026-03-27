using UnityEngine;
using System.Collections;

public class BossEnemy : EnemyBase
{
    [Header("Boss Settings")]
    [SerializeField] private float _slamRadius = 5f;
    [SerializeField] private float _slamDamage = 40f;
    [SerializeField] private float _slamCooldown = 8f;
    [SerializeField] private int _rangedBurstCount = 5;
    [SerializeField] private Transform[] _muzzlePoints;

    [Header("Audio")]
    [SerializeField] protected AudioSource _audioSource;
    [SerializeField] private AudioClip _fireSound;

    private bool _phase2Active;
    private float _slamTimer;
    private float _maxHp;
    private float _accumulatedDamage;
    private Animator _animator;

    private static readonly int HashSpeed = Animator.StringToHash("Speed");
    private static readonly int HashIsIdle = Animator.StringToHash("IsIdle");

    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponent<Animator>();
    }

    protected override void Update()
    {
        base.Update();

        if (_animator != null)
        {
            float speed = Agent.isOnNavMesh && Agent.enabled ? Agent.velocity.magnitude : 0f;
            _animator.SetFloat(HashSpeed, speed);
            _animator.SetBool(HashIsIdle, speed < 0.1f);
        }

        if (_slamTimer < _slamCooldown)
            _slamTimer += Time.deltaTime;
    }

    public override void PerformAttack()
    {
        if (!HasLineOfSight()) return;

        if (!_phase2Active)
        {
            base.PerformAttack();
        }
        else
        {
            StartCoroutine(BurstFire());
        }

        if (_slamTimer >= _slamCooldown)
        {
            _slamTimer = 0f;
            GroundSlam();
        }
    }

    private bool HasLineOfSight()
    {
        if (PlayerTransform == null) return false;

        Vector3 eyePos = transform.position + Vector3.up * 1.5f;
        Vector3 targetPoint = PlayerTransform.position + Vector3.up * 0.5f;
        Vector3 dir = (targetPoint - eyePos);

        if (Physics.Raycast(eyePos, dir.normalized, out RaycastHit hit, Stats.DetectRange))
        {
            if (hit.collider.CompareTag("Player")) return true;
        }
        return false;
    }

    protected override void OnHit(float amount)
    {
        _accumulatedDamage += amount;

        float currentHP = Mathf.Max(0f, _maxHp - _accumulatedDamage);

        EventManager.RaiseBossHealthChanged(currentHP, _maxHp);

        if (_phase2Active) return;

        if (_accumulatedDamage >= _maxHp * 0.5f)
        {
            _phase2Active = true;
            Agent.speed = Stats.MoveSpeed * 1.5f;
        }
    }

    public override void Initialize(EnemyStats stats, Transform player)
    {
        base.Initialize(stats, player);
        _maxHp = stats.MaxHealth;
        _accumulatedDamage = 0f;        

        EventManager.RaiseBossHealthChanged(_maxHp, _maxHp);
    }

    private void GroundSlam()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, _slamRadius);
        foreach (var c in hits)
        {
            if (c.TryGetComponent<PlayerHealth>(out var ph))
                ph.TakeDamage(_slamDamage);
        }
    }

    private IEnumerator BurstFire()
    {
        for (int i = 0; i < _rangedBurstCount; i++)
        {
            if (PlayerTransform == null) yield break;

            if (_audioSource != null && _fireSound != null)
            {
                _audioSource.PlayOneShot(_fireSound);
            }

            foreach (var muzzle in _muzzlePoints)
            {
                if (muzzle == null) continue;

                Vector3 aimPoint = PlayerTransform.position + Vector3.up * 0.8f;
                Vector3 dir = (aimPoint - muzzle.position).normalized;

                Bullet b = PoolManager.instance.GetBullet(muzzle.position, Quaternion.LookRotation(dir));
                if (b != null)
                {
                    b.Initialize(Stats.Damage * 0.4f, 25f, Stats.DetectRange, true);
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    public override void OnDeath()
    {
        base.OnDeath();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _slamRadius);
    }
}