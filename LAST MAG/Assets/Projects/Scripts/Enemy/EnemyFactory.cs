using UnityEngine;

public enum EnemyType { Melee, Ranged, Boss, Flying, FlyingElite }

[System.Serializable]
public struct EnemyPrefabEntry
{
    public EnemyType   Type;
    public EnemyBase   Prefab;
    public EnemyStats  BaseStats;
}

public class EnemyFactory : MonoBehaviour
{
    public static EnemyFactory instance { get; private set; }

    [SerializeField] private EnemyPrefabEntry[] _entries;

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
    }

    public EnemyBase Create(EnemyType type, Vector3 position, int currentWave, Transform player)
    {
        EnemyPrefabEntry entry = GetEntry(type);

        EnemyStats scaledStats = entry.BaseStats.Clone();
        scaledStats.ScaleForWave(currentWave);

        EnemyBase enemy = Instantiate(entry.Prefab, position, Quaternion.identity);
        enemy.Initialize(scaledStats, player);
        enemy.gameObject.tag = "Enemy";

        return enemy;
    }

    private EnemyPrefabEntry GetEntry(EnemyType type)
    {
        foreach (var e in _entries)
            if (e.Type == type) return e;

        return _entries[0];
    }
}
