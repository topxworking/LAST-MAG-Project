using System;
using UnityEngine;

[Serializable]
public class EnemyStats
{
    public float MaxHealth    = 50f;
    public float MoveSpeed    = 3f;
    public float Damage       = 10f;
    public float AttackRange  = 2f;
    public float AttackRate   = 1.5f;
    public float DetectRange  = 20f;
    public int   ScoreValue   = 10;

    public EnemyType EnemyType;

    public void ScaleForWave(int waveNumber)
    {
        int scaleTier  = (waveNumber / 10);
        float mult     = 1f + scaleTier * 0.25f;

        MaxHealth   *= mult;
        Damage      *= mult;
        MoveSpeed   = Mathf.Min(MoveSpeed * (1f + scaleTier * 0.1f), 12f);
        ScoreValue   = Mathf.RoundToInt(ScoreValue * mult);
    }

    public EnemyStats Clone()
    {
        return new EnemyStats
        {
            MaxHealth   = MaxHealth,
            MoveSpeed   = MoveSpeed,
            Damage      = Damage,
            AttackRange = AttackRange,
            AttackRate  = AttackRate,
            DetectRange = DetectRange,
            ScoreValue  = ScoreValue
        };
    }
}
