using System;
using System.Diagnostics;
using CardSystem;

public static class AbilityEvents
{
    public static event Action OnAbilityTargetingStarted;
    public static event Action OnAbilityTargetingStopped;
    public static event Action<Team> OnAbilityUsed;
    public static event Action<Team, CardCategory> OnAbilityUsedDetailed;

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

    public static void AbilityUsed(Team unitTeam, CardCategory category = CardCategory.Melee)
    {
        IsTargeting = false;
        OnAbilityUsed?.Invoke(unitTeam);
        OnAbilityUsedDetailed?.Invoke(unitTeam, category);
        if (unitTeam == Team.Friendly)
            TargetingStopped();
    }
}