using System;

public static class ShieldEvents
{
    // current shield amount
    public static event Action<int> OnPlayerShieldChanged;
    public static event Action<int> OnEnemyShieldChanged;

    public static void RaisePlayerShieldChanged(int current) => OnPlayerShieldChanged?.Invoke(current);
    public static void RaiseEnemyShieldChanged(int current)  => OnEnemyShieldChanged?.Invoke(current);
}