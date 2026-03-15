using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    public enum EnemyTier { Tier1, Tier2, Tier3, Tier4 };

    [Header("Enemy Settings")]
    public EnemyTier tier;
    public float attackRange = 2f;
    public float attackCooldown = 1f;

    private float health;
    private float maxHealth;
    private float speed;
    private float damage;

    private NavMeshAgent agent;
    private Transform player;
    private float lastAttackTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null )
        {
            player = playerObj.transform;
        }

        SetupTierStats();
    }

    void SetupTierStats()
    {
        switch (tier)
        {
            case EnemyTier.Tier1:
                health = 50f; speed = 3.5f; damage = 10f;
                break;
            case EnemyTier.Tier2:
                health = 100f; speed = 4.5f; damage = 20f;
                break;
            case EnemyTier.Tier3:
                health = 250f; speed = 5.5f; damage = 35f;
                break;
            case EnemyTier.Tier4:
                health = 500f; speed = 6.5f; damage = 50f;
                break;
        }

        maxHealth = health;
        agent.speed = speed;
        agent.stoppingDistance = attackRange;

        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
    }

    void Update()
    {
        if (player == null) return;

        agent.SetDestination(player.position);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                AttackPlayer();
            }
        }
    }

    void AttackPlayer()
    {
        lastAttackTime = Time.time;
        Debug.Log("ศัตรูระดับ " + tier + "โจมตีผู้เล่น! ดาเมจ:" + damage);
    }

    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        Debug.Log("ศัตรูโดนยิง! เลือดเหลือ: " + health + "/" + maxHealth);

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("ศัตรูตาย!");
        Destroy(gameObject);
    }
}
