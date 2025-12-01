using CardSystem;
using System;
using UnityEngine;

public static class AbilityEvents
{
    public static event Action OnAbilityTargetingStarted;
    public static event Action OnAbilityTargetingStopped;
    public static event Action<Team> OnAbilityUsed;

    public static bool IsTargeting { get; private set; }

    public static void TargetingStarted()
    {
        IsTargeting = true;
        OnAbilityTargetingStarted?.Invoke();
    }

    public static void TargetingStopped()
    {
        IsTargeting = false;
        OnAbilityTargetingStopped?.Invoke();
    }

    public static void AbilityUsed(Team unitTeam)
    {
        IsTargeting = false;
        OnAbilityUsed?.Invoke(unitTeam);
    }
}
