using System;
using UnityEngine;

public static class EventManager
{
    // Wave Events
    public static event Action<int> OnWaveStarted;
    public static event Action<int> OnWaveCompleted;
    public static event Action<int> OnBossWaveStarted;
    public static event Action OnBossDefeated;

    // Enemy Events
    public static event Action<int> OnEnemyKilled;
    public static event Action<int> OnEnemyCountChanged;

    // Player Events
    public static event Action<float, float> OnPlayerHealthChanged;
    public static event Action OnPlayerDied;

    // Upgrade Events
    public static event Action OnUpgradeMenuOpened;
    public static event Action OnUpgradeMenuClosed;

    // Score / UI
    public static event Action<int> OnScoreChanged;

    public static void RaiseWaveStarted(int wave) => OnWaveStarted?.Invoke(wave);
    public static void RaiseWaveCompleted(int wave) => OnWaveCompleted?.Invoke(wave);
    public static void RaiseBossWaveStarted(int wave) => OnBossWaveStarted?.Invoke(wave);
    public static void RaiseBossDefeated() => OnBossDefeated?.Invoke();
    public static void RaiseEnemyKilled(int score) => OnEnemyKilled?.Invoke(score);
    public static void RaiseEnemyCountChanged(int count) => OnEnemyCountChanged?.Invoke(count);
    public static void RaisePlayerHealthChanged(float cur, float max) => OnPlayerHealthChanged?.Invoke(cur, max);
    public static void RaisePlayerDied() => OnPlayerDied?.Invoke();
    public static void RaiseUpgradeMenuOpened() => OnUpgradeMenuOpened?.Invoke();
    public static void RaiseUpgradeMenuClosed() => OnUpgradeMenuClosed?.Invoke();
    public static void RaiseScoreChanged(int score) => OnScoreChanged?.Invoke(score);

    public static void ClearAllEvents()
    {
        OnWaveStarted = null;
        OnWaveCompleted = null;
        OnBossWaveStarted = null;
        OnBossDefeated = null;
        OnEnemyKilled = null;
        OnEnemyCountChanged = null;
        OnPlayerHealthChanged = null;
        OnPlayerDied = null;
        OnUpgradeMenuOpened = null;
        OnUpgradeMenuClosed = null;
        OnScoreChanged = null;
    }
}
