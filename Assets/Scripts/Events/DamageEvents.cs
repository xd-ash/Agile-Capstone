using System;

public static class DamageEvents
{
    // (current, max)
    public static event Action<int, int> OnPlayerDamaged;
    public static event Action<int, int> OnEnemyDamaged;
    
    public static void RaisePlayerDamaged(int current, int max) => OnPlayerDamaged?.Invoke(current, max);
    public static void RaiseEnemyDamaged(int current, int max)  => OnEnemyDamaged?.Invoke(current, max);
}