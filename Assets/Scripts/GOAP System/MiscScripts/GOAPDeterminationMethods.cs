using static IsoMetricConversions;
using static CombatMath;
using UnityEngine;
using static AStarPathfinding.FindPathAStar;

public static class GOAPDeterminationMethods
{
    private static int _atRangeThreshold = 4;
    public static int GetAtRangeThreshold => _atRangeThreshold;

    public static int FindAPAfterAction(Unit unit, int actionAPCost)
    {
        int result = unit.GetAP - actionAPCost;
        return result <= 0 ? 0 : result;
    }
    public static bool CheckCanDoAction(Unit unit, int actionAPCost)
    {
        return unit.GetAP >= actionAPCost;
    }
    public static bool CheckForAP(Unit unit, ref WorldStates beliefs)
    {
        if (unit.GetAP == 0)
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
    public static bool CheckRange(GoapAgent agent, int abilityRange, ref WorldStates beliefs)
    {
        var unitMover = agent.GetComponent<UnitMovementController>();
        //int dmgAbilRange = agent.damageAbility.GetRange;
         
        var tarPos = ConvertToGridFromIsometric(agent.GetCurrentTarget.transform.localPosition);
        var tempPath = unitMover.CalculatePath(tarPos);
       // Debug.Log($"pathCount range: {tempPath.Count}, abilRange: {abilityRange}");
        int distanceToTar = tempPath.Count;

        if (distanceToTar >= _atRangeThreshold)
        {
            beliefs.ModifyState(GoapStates.AtRange.ToString(), 1);
            beliefs.RemoveState(GoapStates.AtMelee.ToString());
        }
        else
        {
            beliefs.ModifyState(GoapStates.AtMelee.ToString(), 1);
            beliefs.RemoveState(GoapStates.AtRange.ToString());
        }

        if (distanceToTar > abilityRange)
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
        if (unit == null) return false;

        float healthPercent = (float)unit.GetHealth / (float)unit.GetMaxHealth;
        
        if (healthPercent > 0.65f)
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
        var tarPos = ConvertToGridFromIsometric(agent.GetCurrentTarget.transform.localPosition);

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
    public static float GetAdjustedMovementDistRatio(Transform agent, Transform target)
    {
        if (!agent.TryGetComponent(out Unit unit)) return 0f;

        var distToTar = CalculatePath(agent, target).Count;
        int maxAP = unit.GetAP;
        float distRatio = (maxAP - distToTar) / (float)maxAP;
        return Mathf.Clamp(distRatio, 0, 1);
    }
    public static float GetAdjustedMovementDistRatio(Vector2Int agentPos, Vector2Int targetPos, Unit unit)
    {
        if (unit == null) return 0f;

        var distToTar = CalculatePath(agentPos, targetPos).Count;
        int maxAP = unit.GetAP;
        float distRatio = distToTar / (float)maxAP;
        return Mathf.Clamp(distRatio, 0, 1);
    }
}
