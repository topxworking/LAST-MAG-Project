using UnityEngine;
using UnityEngine.AI;

public interface IEnemyState
{
    void Enter(EnemyBase enemy);
    void Tick(EnemyBase enemy);
    void Exit(EnemyBase enemy);
}

public class IdleState : IEnemyState
{
    private float _wanderTimer;
    private float _wanderInterval = 3f;

    public void Enter(EnemyBase e)
    {
        if (e.Agent.isActiveAndEnabled && e.Agent.isOnNavMesh)
            e.Agent.isStopped = false;

        if (e.Agent.isOnNavMesh)
        {
            e.Agent.isStopped = false;
            e.Agent.speed = e.Stats.MoveSpeed * 0.4f;
        }
        _wanderTimer = 0f;
        SetRandomDestination(e);
    }

    public void Tick(EnemyBase e)
    {
        if (e.DistanceToPlayer <= e.Stats.DetectRange)
        {
            e.TransitionTo(e.ChaseState);
            return;
        }

        _wanderTimer += Time.deltaTime;
        if (_wanderTimer >= _wanderInterval ||
            !e.Agent.pathPending && e.Agent.remainingDistance < 0.5f)
        {
            _wanderTimer = 0f;
            SetRandomDestination(e);
        }
    }

    public void Exit(EnemyBase e)
    {
        if (e.Agent.isActiveAndEnabled && e.Agent.isOnNavMesh)
            e.Agent.isStopped = false;

        if (e.Agent.isOnNavMesh)
            e.Agent.speed = e.Stats.MoveSpeed;
    }

    private void SetRandomDestination(EnemyBase e)
    {
        Vector3 randomDir = Random.insideUnitSphere * 10f;
        randomDir += e.transform.position;
        randomDir.y = e.transform.position.y;

        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            if (e.Agent.isOnNavMesh)
                e.Agent.SetDestination(hit.position);
        }
    }
}

public class ChaseState : IEnemyState
{
    public void Enter(EnemyBase e)
    {
        if (e.Agent.isActiveAndEnabled && e.Agent.isOnNavMesh)
            e.Agent.isStopped = false;
    }

    public void Tick(EnemyBase e)
    {
        if (e.DistanceToPlayer <= e.Stats.AttackRange)
        {
            e.TransitionTo(e.AttackState);
            return;
        }
        if (e.DistanceToPlayer > e.Stats.DetectRange + 5f)
        {
            e.TransitionTo(e.IdleState);
            return;
        }
        e.Agent.SetDestination(e.PlayerTransform.position);
    }
    public void Exit(EnemyBase e)
    {
        if (e.Agent.isActiveAndEnabled && e.Agent.isOnNavMesh)
            e.Agent.isStopped = false;
    }
}

public class AttackState : IEnemyState
{
    private float _timer;
    public void Enter(EnemyBase e)
    {
        if (e.Agent.isActiveAndEnabled && e.Agent.isOnNavMesh)
            e.Agent.isStopped = true;
        _timer = 0f;
    }
    public void Tick(EnemyBase e)
    {
        if (e.DistanceToPlayer > e.Stats.AttackRange + 0.5f)
        {
            e.TransitionTo(e.ChaseState);
            return;
        }
        Vector3 dir = (e.PlayerTransform.position - e.transform.position).normalized;
        dir.y = 0f;
        e.transform.rotation = Quaternion.Slerp(
            e.transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 8f);

        _timer += Time.deltaTime;
        if (_timer >= e.Stats.AttackRate)
        {
            _timer = 0f;
            e.PerformAttack();
        }
    }
    public void Exit(EnemyBase e)
    {
        if (e.Agent.isActiveAndEnabled && e.Agent.isOnNavMesh)
            e.Agent.isStopped = false;
    }
}

public class DeadState : IEnemyState
{
    public void Enter(EnemyBase e)
    {
        if (e.Agent.isActiveAndEnabled && e.Agent.isOnNavMesh)
            e.Agent.isStopped = true;
        e.Agent.enabled = false;
        e.OnDeath();
    }
    public void Tick(EnemyBase e)  { }
    public void Exit(EnemyBase e)  { }
}

[RequireComponent(typeof(NavMeshAgent))]
public abstract class EnemyBase : MonoBehaviour
{
    public  NavMeshAgent Agent { get; protected set; }
    public EnemyStats Stats { get; protected set; }
    public Transform PlayerTransform { get; protected set; }
    public float DistanceToPlayer { get; protected set; }

    public IEnemyState IdleState   { get; } = new IdleState();
    public IEnemyState ChaseState  { get; } = new ChaseState();
    public IEnemyState AttackState { get; } = new AttackState();
    public IEnemyState DeadState   { get; } = new DeadState();

    private IEnemyState _currentState;
    private float       _currentHealth;
    private bool        _isDead;

    protected virtual void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
    }

    public virtual void Initialize(EnemyStats stats, Transform player)
    {
        Stats           = stats;
        PlayerTransform = player;
        _currentHealth  = stats.MaxHealth;
        _isDead         = false;

        Agent.speed         = stats.MoveSpeed;
        Agent.stoppingDistance = stats.AttackRange * 0.9f;

        TransitionTo(IdleState);
    }

    protected virtual void Update()
    {
        if (_isDead || PlayerTransform == null || Stats == null) return;

        DistanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);
        _currentState?.Tick(this);
    }

    public void TransitionTo(IEnemyState next)
    {
        _currentState?.Exit(this);
        _currentState = next;
        _currentState.Enter(this);
    }

    public virtual void TakeDamage(float amount)
    {
        if (_isDead) return;
        _currentHealth -= amount;
        OnHit(amount);

        if (!_isDead)
        {
            TransitionTo(ChaseState);
        }

        if (_currentHealth <= 0f)
        {
            _isDead = true;
            TransitionTo(DeadState);
        }
    }

    public virtual void PerformAttack()
    {
        if (PlayerTransform.TryGetComponent<PlayerHealth>(out var ph))
            ph.TakeDamage(Stats.Damage);
    }

    protected virtual void OnHit(float amount) { }

    [Header("Death Effect")]
    [SerializeField] private GameObject _deathVFX;

    public virtual void OnDeath()
    {
        if (_deathVFX != null)
        {
            GameObject fx = Instantiate(_deathVFX, transform.position, Quaternion.identity);
            Destroy(fx, 3f);
        }

        if (GameManager.instance != null && Stats != null)
        {
            GameManager.instance.RegisterKill(Stats.EnemyType);
        }

        EventManager.RaiseEnemyKilled(Stats.ScoreValue);
        WaveManager.instance?.RegisterEnemyKilled();
        Destroy(gameObject, 1.5f);
    }
}
