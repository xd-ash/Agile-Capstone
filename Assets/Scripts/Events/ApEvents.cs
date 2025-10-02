using System;

public static class ApEvents
{
    public static event Action<int, int> PlayerApChanged;
    public static event Action<int, int> EnemyApChanged;

    public static void RaisePlayerApChanged(int current, int max) => PlayerApChanged?.Invoke(current, max);
    public static void RaiseEnemyApChanged(int current, int max) => EnemyApChanged?.Invoke(current, max);
}