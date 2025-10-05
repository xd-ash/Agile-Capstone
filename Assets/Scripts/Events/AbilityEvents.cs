using System;
using UnityEngine;

public static class AbilityEvents
{
    public static event Action OnAbilityTargetingStarted;
    public static event Action OnAbilityUsed;

    public static void TargetingStarted() => OnAbilityTargetingStarted?.Invoke();
    public static void AbilityUsed() => OnAbilityUsed?.Invoke();
}
