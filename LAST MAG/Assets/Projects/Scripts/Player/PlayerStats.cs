using UnityEngine;
using System;

[Serializable]
public class PlayerStats
{
    public float MaxHealth = 100f;
    public float MoveSpeed = 6f;
    public float JumpForce = 8f;
    public float Damage = 20f;
    public float FireRate = 0.2f;
    public float BulletSpeed = 30f;
    public float BulletRange = 50f;
    public int UpgradePoints = 0;

    [NonSerialized] public float CurrentHealth;

    public void Initialize()
    {
        CurrentHealth = MaxHealth;
    }

    public bool UpgradeHealth()
    {
        if (UpgradePoints < 1) return false;
        MaxHealth += 25f;
        CurrentHealth += 25f;
        UpgradePoints--;
        return true;
    }

    public bool UpgradeSpeed()
    {
        if (UpgradePoints < 1) return false;
        MoveSpeed += 0.5f;
        UpgradePoints--;
        return true;
    }

    public bool UpgradeDamage()
    {
        if (UpgradePoints < 1) return false;
        Damage += 10f;
        UpgradePoints--;
        return true;
    }

    public bool UpgradeFireRate()
    {
        if (UpgradePoints < 1) return false;
        FireRate = Mathf.Max(0.05f, FireRate - 0.02f);
        UpgradePoints--;
        return true;
    }

    public bool UpgradeBulletSpeed()
    {
        if (UpgradePoints < 1) return false;
        BulletSpeed += 5f;
        UpgradePoints--;
        return true;
    }

    public bool UpgradeJump()
    {
        if (UpgradePoints < 1) return false;
        JumpForce += 1f;
        UpgradePoints--;
        return true;
    }
}
