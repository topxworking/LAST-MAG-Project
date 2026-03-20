using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float _baseMaxHealth = 100f;
    [SerializeField] private float _baseMoveSpeed = 6f;
    [SerializeField] private float _baseJumpForce = 8f;
    [SerializeField] private float _baseDamage = 20f;
    [SerializeField] private float _baseFireRate = 0.2f;
    [SerializeField] private float _baseBulletSpeed = 30f;

    public PlayerStats Stats { get; private set; }

    private bool _isDead;

    private void Awake()
    {
        Stats = new PlayerStats
        {
            MaxHealth = _baseMaxHealth,
            MoveSpeed = _baseMoveSpeed,
            JumpForce = _baseJumpForce,
            Damage = _baseDamage,
            FireRate = _baseFireRate,
            BulletSpeed = _baseBulletSpeed,
        };
        Stats.Initialize();
    }

    private void Start()
    {
        EventManager.RaisePlayerHealthChanged(Stats.CurrentHealth, Stats.MaxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (_isDead) return;
        Stats.CurrentHealth = Mathf.Max(0f, Stats.CurrentHealth - amount);
        EventManager.RaisePlayerHealthChanged(Stats.CurrentHealth, Stats.MaxHealth);

        if (Stats.CurrentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        Stats.CurrentHealth = Mathf.Min(Stats.MaxHealth, Stats.CurrentHealth + amount);
        EventManager.RaisePlayerHealthChanged(Stats.CurrentHealth, Stats.MaxHealth);
    }

    private void Die()
    {
        _isDead = true;
        EventManager.RaisePlayerDied();
    }
}
