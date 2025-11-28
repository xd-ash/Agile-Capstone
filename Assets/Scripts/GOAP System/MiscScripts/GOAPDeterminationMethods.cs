using AStarPathfinding;
using Unity.VisualScripting;
using UnityEngine;
using static IsoMetricConversions;
using static CombatMath;

public static class GOAPDeterminationMethods
{
    public static int FindAPAfterAction(Unit unit, int actionAPCost)
    {
        int result = unit.ap - actionAPCost;
        return result <= 0 ? 0 : result;
    }
    public static bool CheckCanDoAction(Unit unit, int actionAPCost)
    {
        return unit.ap >= actionAPCost;
    }
    public static bool CheckForAP(Unit unit, ref WorldStates beliefs)
    {
        if (unit.ap == 0)
        {
            beliefs.ModifyState(GoapStates.OutOfAP.ToString(), 1);
            beliefs.RemoveState(GoapStates.HasAP.ToString());
            return false;
        }
        else
        {
            beliefs.ModifyState(GoapStates.HasAP.ToString(), 1);
            beliefs.RemoveState(GoapStates.OutOfAP.ToString());
            return true;
        }
    }
    public static bool CheckIfInRange(GoapAgent agent, int abilityRange, ref WorldStates beliefs)
    {
        var aStar = agent.GetComponent<FindPathAStar>();
        int dmgAbilRange = agent.damageAbility.RootNode.GetRange;

        var tarPos = ConvertToGridFromIsometric(agent.curtarget.transform.localPosition);
        var tempPath = aStar.CalculatePath(tarPos);
        int distanceToTar = tempPath.Count;

        if (distanceToTar > dmgAbilRange)
        {
            beliefs.ModifyState(GoapStates.OutOfRange.ToString(), 1);
            beliefs.RemoveState(GoapStates.InRange.ToString());
            return false;
        }

        beliefs.ModifyState(GoapStates.InRange.ToString(), 1);
        beliefs.RemoveState(GoapStates.OutOfRange.ToString());
        return true;
    }
    public static bool CheckIfHealthy(Unit unit, ref WorldStates beliefs)
    {
        float healthPercent = unit.health / unit.maxHealth;

        if (healthPercent > 0.75f)
        {
            beliefs.ModifyState(GoapStates.IsHealthy.ToString(), 1);
            beliefs.RemoveState(GoapStates.IsHurt.ToString());
            return true;
        }
        else
        {
            beliefs.ModifyState(GoapStates.IsHurt.ToString(), 1);
            beliefs.RemoveState(GoapStates.IsHealthy.ToString());
            return false;
        }
    }
    public static bool CheckIfInLOS(GoapAgent agent, ref WorldStates beliefs)
    {
        var agentPos = ConvertToGridFromIsometric(agent.transform.localPosition);
        var tarPos = ConvertToGridFromIsometric(agent.curtarget.transform.localPosition);

        bool hasLOS = HasLineOfSight(agentPos, tarPos);

        if (hasLOS)
        {
            beliefs.ModifyState(GoapStates.HasLOS.ToString(), 1);
            beliefs.RemoveState(GoapStates.NoLOS.ToString());
        }
        else
        {
            beliefs.ModifyState(GoapStates.NoLOS.ToString(), 1);
            beliefs.RemoveState(GoapStates.HasLOS.ToString());
        }
        return hasLOS;
    }
}
