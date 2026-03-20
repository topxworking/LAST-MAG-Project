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

    private bool _phase2Active;
    private float _slamTimer;
    private float _maxHealth;

    public override void PerformAttack()
    {
        if (!_phase2Active)
        {
            base.PerformAttack();
        }
        else
        {
            StartCoroutine(BurstFire());
        }

        _slamTimer += Stats.AttackRate;
        if (_slamTimer >= _slamCooldown)
        {
            _slamTimer = 0f;
            GroundSlam();
        }
    }

    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);        
    }

    protected override void OnHit(float amount)
    {
        if (!_phase2Active && _maxHealth > 0f)
        {
            _phase2Active = true;
            Debug.Log("[Boss] Phase 2 activated!");
            Agent.speed = Stats.MoveSpeed * 1.5f;
        }
    }

    public new void Initialize(EnemyStats stats, Transform player)
    {
        _maxHealth = stats.MaxHealth;
        base.Initialize(stats, player);
    }

    private void GroundSlam()
    {
        Debug.Log("[Boss] GROUND SLAM!");
        Collider[] hits = Physics.OverlapSphere(transform.position, _slamRadius);
        foreach (var c in hits)
        {
            if (c.TryGetComponent<PlayerHealth>(out var ph))
                ph.TakeDamage(_slamDamage);
        }
    }

    private System.Collections.IEnumerator BurstFire()
    {
        for (int i = 0; i < _rangedBurstCount; i++)
        {
            foreach (var muzzle in _muzzlePoints)
            {
                Vector3 dir = (PlayerTransform.position + Vector3.up - muzzle.position).normalized;
                Bullet b = PoolManager.instance.GetBullet(muzzle.position, Quaternion.LookRotation(dir));
                b.Initialize(Stats.Damage * 0.5f, 18f, Stats.DetectRange, true);
            }
            yield return new WaitForSeconds(0.15f);
        }
    }

    public override void OnDeath()
    {
        EventManager.RaiseBossDefeated();
        EventManager.RaiseEnemyKilled(Stats.ScoreValue);
        WaveManager.instance?.RegisterEnemyKilled();
        Destroy(gameObject, 2f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _slamRadius);
    }
}
