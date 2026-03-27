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

    public int MagazineSize = 30;
    public float ReloadTime = 2.0f;

    public int UpgradePoints = 0;

    [NonSerialized] public float CurrentHealth;
    [NonSerialized] public int CurrentAmmo;

    public void Initialize()
    {
        CurrentHealth = MaxHealth;
        CurrentAmmo = MagazineSize;
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

    public bool UpgradeReloadSpeed()
    {
        if (UpgradePoints < 1) return false;
        ReloadTime = Mathf.Max(0.3f, ReloadTime - 0.2f);
        UpgradePoints--;
        return true;
    }
}
