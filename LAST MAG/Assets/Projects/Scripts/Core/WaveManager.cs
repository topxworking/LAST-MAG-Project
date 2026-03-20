using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WaveManager : MonoBehaviour
{
    public static WaveManager instance { get; private set; }

    [Header("Spawn Points")]
    [SerializeField] private Transform[] _spawnPoints;

    [Header("Wave Settings")]
    [SerializeField] private int _baseEnemyCount = 5;
    [SerializeField] private int _enemiesAddedPerWave = 2;
    [SerializeField] private float _spawnInterval = 0.5f;
    [SerializeField] private float _waveCompleteDelay = 2f;

    [Header("References")]
    [SerializeField] private Transform _playerTransform;

    private int _currentWave;
    private int _enemiesAlive;
    private int _enemiesToSpawn;
    private bool _waveActive;


    private readonly List<EnemyBase> _activeEnemies = new();

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
    }

    public void StartWave(int waveNumber)
    {
        _currentWave = waveNumber;
        _waveActive = true;

        _activeEnemies.Clear();

        bool isBoss = waveNumber % 10 == 0 && waveNumber > 0;

        if (isBoss)
            StartCoroutine(SpawnBossWave());
        else
            StartCoroutine(SpawnNormalWave());
    }

    private IEnumerator SpawnNormalWave()
    {
        _enemiesToSpawn = _baseEnemyCount + (_currentWave - 1) * _enemiesAddedPerWave;
        _enemiesAlive = _enemiesToSpawn;
        EventManager.RaiseEnemyCountChanged(_enemiesAlive);

        for (int i = 0; i < _enemiesToSpawn; i++)
        {
            EnemyType type = (_currentWave >= 5 && i % 3 == 2)
                ? EnemyType.Ranged
                : EnemyType.Melee;

            SpawnEnemy(type, GetRandomSpawnPoint());
            yield return new WaitForSeconds(_spawnInterval);
        }
    }

    private IEnumerator SpawnBossWave()
    {
        int addCount = 4 + (_currentWave / 10);
        _enemiesToSpawn = addCount + 1;
        _enemiesAlive = _enemiesToSpawn;
        EventManager.RaiseEnemyCountChanged(_enemiesAlive);

        for (int i = 0; i < addCount; i++)
        {
            SpawnEnemy(EnemyType.Melee, GetRandomSpawnPoint());
            yield return new WaitForSeconds(_spawnInterval);
        }

        yield return new WaitForSeconds(1.5f);

        Vector3 bossPos = _playerTransform.position + _playerTransform.forward * 15f;
        if (NavMesh.SamplePosition(bossPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            bossPos = hit.position;

        SpawnEnemy(EnemyType.Boss, bossPos);
    }

    private void SpawnEnemy(EnemyType type, Vector3 position)
    {
        EnemyBase enemy = EnemyFactory.Instance.Create(type, position, _currentWave, _playerTransform);
        _activeEnemies.Add(enemy);
    }

    private Vector3 GetRandomSpawnPoint()
    {
        if (_spawnPoints == null || _spawnPoints.Length == 0)
            return _playerTransform.position + Random.insideUnitSphere * 20f;

        return _spawnPoints[Random.Range(0, _spawnPoints.Length)].position;
    }

    public void RegisterEnemyKilled()
    {
        _enemiesAlive = Mathf.Max(0, _enemiesAlive - 1);
        EventManager.RaiseEnemyCountChanged(_enemiesAlive);

        if (_enemiesAlive <= 0 && _waveActive)
            StartCoroutine(CompleteWave());
    }

    private IEnumerator CompleteWave()
    {
        _waveActive = false;
        yield return new WaitForSeconds(_waveCompleteDelay);
        EventManager.RaiseWaveCompleted(_currentWave);
    }
}
