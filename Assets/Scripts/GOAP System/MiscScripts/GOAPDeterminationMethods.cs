using AStarPathfinding;
using Unity.VisualScripting;
using UnityEngine;
using static IsoMetricConversions;

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
    public static void CheckForAP(Unit unit, ref WorldStates beliefs, int actionCost = 0)
    {
        if (unit.ap == 0 || unit.ap < actionCost)
        {
            beliefs.ModifyState(GoapStates.OutOfAP.ToString(), 1);
            beliefs.RemoveState(GoapStates.HasAP.ToString());
        }
        else
        {
            beliefs.ModifyState(GoapStates.HasAP.ToString(), 1);
            beliefs.RemoveState(GoapStates.OutOfAP.ToString());
        }
    }
    public static bool CheckIfInRange(GoapAgent agent, Unit target, int abilityRange)
    {
        var aStar = agent.GetComponent<FindPathAStar>();
        int dmgAbilRange = agent.damageAbility.RootNode.GetRange;

        var tarPos = ConvertToGridFromIsometric(agent.curtarget.transform.localPosition);
        var tempPath = aStar.CalculatePath(tarPos);
        int distanceToTar = tempPath.Count;

        if (distanceToTar > dmgAbilRange)
            return false;
        return true;
    }
}
