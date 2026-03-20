using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private Bullet _bulletPrefab;
    [SerializeField] private int _bulletPoolSize = 30;

    private ObjectPool<Bullet> _bulletPool;
    private Transform _bulletParent;

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;

        _bulletParent = new GameObject("BulletPool").transform;
        _bulletParent.SetParent(transform);
        _bulletPool = new ObjectPool<Bullet>(_bulletPrefab, _bulletPoolSize, _bulletParent);
    }

    public Bullet GetBullet(Vector3 pos, Quaternion rot)
        => _bulletPool.Get(pos, rot);

    public void ReturnBullet(Bullet bullet)
        => _bulletPool.Return(bullet);
}
