using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour, IPoolable
{
    [SerializeField] private GameObject _hitVFX;

    private ObjectPool<Bullet> _pool;
    private Rigidbody _rb;

    private float _damage;
    private float _speed;
    private float _maxRange;
    private float _travelledDistance;
    private bool _isEnemyBullet;
    private Vector3 _spawnPosition;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void Update()
    {
        _travelledDistance += _speed * Time.deltaTime;
        if (_travelledDistance >= _maxRange)
            ReturnToPool();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isEnemyBullet)
        {
            if (other.TryGetComponent<PlayerHealth>(out var ph))
            {
                ph.TakeDamage(_damage);
                SpawnVFX();
                ReturnToPool();
            }
        }
        else
        {
            if (other.TryGetComponent<EnemyBase>(out var enemy))
            {
                enemy.TakeDamage(_damage);
                SpawnVFX();
                ReturnToPool();
            }
        }

        if (!other.isTrigger && !other.CompareTag("Player") && !other.CompareTag("Enemy"))
        {
            SpawnVFX();
            ReturnToPool();
        }
    }

    public void Initialize(float damage, float speed, float range, bool isEnemy)
    {
        _damage = damage;
        _speed = speed;
        _maxRange = range;
        _isEnemyBullet = isEnemy;
        _spawnPosition = transform.position;
        _travelledDistance = 0f;
        _rb.linearVelocity = transform.forward * speed;
    }

    public void SetPool<T>(ObjectPool<T> pool) where T : MonoBehaviour, IPoolable
        => _pool = pool as ObjectPool<Bullet>;

    public void OnSpawn() { }
    public void OnDespawn() { _rb.linearVelocity = Vector3.zero; }

    private void ReturnToPool() => PoolManager.instance.ReturnBullet(this);

    private void SpawnVFX()
    {
        if (_hitVFX != null)
            Instantiate(_hitVFX, transform.position, Quaternion.identity);
    }
}
