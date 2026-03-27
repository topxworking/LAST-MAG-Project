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
    [SerializeField] private float _waveCompleteDelay = 2f;
    [SerializeField] private float _spawnDelay = 0.5f;

    [Header("References")]
    [SerializeField] private Transform _playerTransform;

    private int _currentWave;
    private int _enemiesAlive;
    private bool _waveActive;

    private Coroutine _currentWaveRoutine;

    private readonly List<EnemyBase> _activeEnemies = new List<EnemyBase>();

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
    }

    public void StartWave(int waveNumber)
    {
        if (_waveActive) return;

        if (_currentWaveRoutine != null) StopCoroutine(_currentWaveRoutine);

        _currentWave = waveNumber;
        _waveActive = true;

        _activeEnemies.Clear();

        EventManager.RaiseWaveStarted(_currentWave);
        _currentWaveRoutine = StartCoroutine(CountdownThenSpawn());
    }

    private IEnumerator CountdownThenSpawn()
    {
        for (int i = 5; i >= 1; i--)
        {
            EventManager.RaiseCountdownTick(i);
            yield return new WaitForSeconds(1f);
        }
        EventManager.RaiseCountdownFinished();

        bool isBoss = _currentWave % 10 == 0 && _currentWave > 0;

        if (isBoss)
            yield return StartCoroutine(SpawnBossWaveRoutine());
        else
            yield return StartCoroutine(SpawnNormalWaveRoutine());
    }

    private IEnumerator SpawnNormalWaveRoutine()
    {
        int totalToSpawn = _baseEnemyCount + (_currentWave - 1) * _enemiesAddedPerWave;

        _enemiesAlive = totalToSpawn;
        EventManager.RaiseEnemyCountChanged(_enemiesAlive);

        for (int i = 0; i < totalToSpawn; i++)
        {
            EnemyType type = EnemyType.Melee;
            if (_currentWave >= 3 && i % 4 == 3) type = EnemyType.Flying;
            else if (_currentWave >= 5 && i % 3 == 2) type = EnemyType.FlyingElite;

            SpawnEnemy(type, GetRandomSpawnPoint());

            yield return new WaitForSeconds(_spawnDelay);
        }
    }

    private IEnumerator SpawnBossWaveRoutine()
    {
        int meleeCount = 4 + (_currentWave / 10);
        int eliteCount = 2;
        int bossCount = 1;

        _enemiesAlive = meleeCount + eliteCount + bossCount;
        EventManager.RaiseEnemyCountChanged(_enemiesAlive);

        for (int i = 0; i < meleeCount; i++)
        {
            SpawnEnemy(EnemyType.Melee, GetRandomSpawnPoint());
            yield return new WaitForSeconds(_spawnDelay);
        }

        for (int i = 0; i < eliteCount; i++)
        {
            SpawnEnemy(EnemyType.FlyingElite, GetRandomSpawnPoint());
            yield return new WaitForSeconds(_spawnDelay);
        }

        Vector3 bossPos = _playerTransform.position + _playerTransform.forward * 15f;
        if (NavMesh.SamplePosition(bossPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            bossPos = hit.position;

        SpawnEnemy(EnemyType.Boss, bossPos);
    }

    private void SpawnNormalWaveInstant()
    {
        int totalToSpawn = _baseEnemyCount + (_currentWave - 1) * _enemiesAddedPerWave;

        _enemiesAlive = totalToSpawn;
        EventManager.RaiseEnemyCountChanged(_enemiesAlive);

        for (int i = 0; i < totalToSpawn; i++)
        {
            EnemyType type = EnemyType.Melee;
            if (_currentWave >= 3 && i % 4 == 3) type = EnemyType.Flying;
            else if (_currentWave >= 5 && i % 3 == 2) type = EnemyType.FlyingElite;

            SpawnEnemy(type, GetRandomSpawnPoint());
        }
    }

    private void SpawnBossWaveInstant()
    {
        int meleeCount = 4 + (_currentWave / 10);
        int eliteCount = 2;
        int bossCount = 1;

        _enemiesAlive = meleeCount + eliteCount + bossCount;
        EventManager.RaiseEnemyCountChanged(_enemiesAlive);

        for (int i = 0; i < meleeCount; i++) SpawnEnemy(EnemyType.Melee, GetRandomSpawnPoint());
        for (int i = 0; i < eliteCount; i++) SpawnEnemy(EnemyType.FlyingElite, GetRandomSpawnPoint());

        Vector3 bossPos = _playerTransform.position + _playerTransform.forward * 15f;
        if (NavMesh.SamplePosition(bossPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            bossPos = hit.position;

        SpawnEnemy(EnemyType.Boss, bossPos);
    }

    private void SpawnEnemy(EnemyType type, Vector3 position)
    {
        EnemyBase enemy = EnemyFactory.instance.Create(type, position, _currentWave, _playerTransform);

        _activeEnemies.Add(enemy);
    }

    private Vector3 GetRandomSpawnPoint()
    {
        if (_spawnPoints == null || _spawnPoints.Length == 0)
        {
            Vector2 rand = Random.insideUnitCircle.normalized * Random.Range(15f, 25f);
            return _playerTransform.position + new Vector3(rand.x, 0, rand.y);
        }
        return _spawnPoints[Random.Range(0, _spawnPoints.Length)].position;
    }

    public void RegisterEnemyKilled()
    {
        if (!_waveActive) return;

        _enemiesAlive = Mathf.Max(0, _enemiesAlive - 1);
        EventManager.RaiseEnemyCountChanged(_enemiesAlive);

        if (_enemiesAlive <= 0)
            StartCoroutine(CompleteWave());
    }

    private IEnumerator CompleteWave()
    {
        _waveActive = false;

        foreach (var enemy in _activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        _activeEnemies.Clear();

        yield return new WaitForSeconds(_waveCompleteDelay);
        EventManager.RaiseWaveCompleted(_currentWave);
    }
}
