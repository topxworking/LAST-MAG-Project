using System;
using UnityEngine;

public static class EventManager
{
    // Wave Events
    public static event Action<int> OnWaveStarted;
    public static event Action<int> OnWaveCompleted;
    public static event Action<int> OnBossWaveStarted;
    public static event Action OnBossDefeated;

    public static event Action<int> OnEnemyKilled;
    public static event Action<int> OnEnemyCountChanged;

    public static event Action<float, float> OnPlayerHealthChanged;
    public static event Action OnPlayerDied;

    public static event Action<int, int> OnAmmoChanged;
    public static event Action<float> OnReloadStarted;
    public static event Action OnReloadFinished;

    public static event Action OnUpgradeMenuOpened;
    public static event Action OnUpgradeMenuClosed;

    public static event Action<int> OnScoreChanged;

    public static event Action<int> OnCountdownTick;
    public static event Action OnCountdownFinished;

    public static event Action<float, float> OnBossHealthChanged;
    public static void RaiseBossHealthChanged(float cur, float max)
        => OnBossHealthChanged?.Invoke(cur, max);

    public static void RaiseWaveStarted(int wave) => OnWaveStarted?.Invoke(wave);
    public static void RaiseWaveCompleted(int wave) => OnWaveCompleted?.Invoke(wave);
    public static void RaiseBossWaveStarted(int wave) => OnBossWaveStarted?.Invoke(wave);
    public static void RaiseBossDefeated() => OnBossDefeated?.Invoke();
    public static void RaiseEnemyKilled(int score) => OnEnemyKilled?.Invoke(score);
    public static void RaiseEnemyCountChanged(int count) => OnEnemyCountChanged?.Invoke(count);
    public static void RaisePlayerHealthChanged(float cur, float max) => OnPlayerHealthChanged?.Invoke(cur, max);
    public static void RaisePlayerDied() => OnPlayerDied?.Invoke();
    public static void RaiseAmmoChanged(int cur, int max) => OnAmmoChanged?.Invoke(cur, max);
    public static void RaiseReloadStarted(float duration) => OnReloadStarted?.Invoke(duration);
    public static void RaiseReloadFinished() => OnReloadFinished?.Invoke();
    public static void RaiseUpgradeMenuOpened() => OnUpgradeMenuOpened?.Invoke();
    public static void RaiseUpgradeMenuClosed() => OnUpgradeMenuClosed?.Invoke();
    public static void RaiseScoreChanged(int score) => OnScoreChanged?.Invoke(score);
    public static void RaiseCountdownTick(int sec) => OnCountdownTick?.Invoke(sec);
    public static void RaiseCountdownFinished() => OnCountdownFinished?.Invoke();

    public static void ClearAllEvents()
    {
        OnWaveStarted = null; OnWaveCompleted = null;
        OnBossWaveStarted = null; OnBossDefeated = null;
        OnEnemyKilled = null; OnEnemyCountChanged = null;
        OnPlayerHealthChanged = null; OnPlayerDied = null;
        OnAmmoChanged = null; OnReloadStarted = null;
        OnReloadFinished = null;
        OnUpgradeMenuOpened = null; OnUpgradeMenuClosed = null;
        OnScoreChanged = null;
    }
}
