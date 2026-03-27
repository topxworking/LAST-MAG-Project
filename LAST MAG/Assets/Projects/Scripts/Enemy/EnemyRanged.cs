using UnityEngine;

public class EnemyRanged : EnemyBase
{
    [SerializeField] private Transform _muzzle;

    public override void PerformAttack()
    {
        if (_muzzle == null) return;

        Vector3 dir = (PlayerTransform.position + Vector3.up * 0.5f - _muzzle.position).normalized;
        Quaternion rot = Quaternion.LookRotation(dir);

        Bullet bullet = PoolManager.instance.GetBullet(_muzzle.position, rot);
        bullet.Initialize(Stats.Damage, 15f, Stats.DetectRange, true);
    }
}
