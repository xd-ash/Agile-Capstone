using static IsoMetricConversions;
using static CombatMath;
using UnityEngine;
using static AStarPathfinding.FindPathAStar;
using CardSystem;

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

        var tarPos = ByteMapController.Instance.GetPositionOfUnit(agent.GetCurrentTarget);
        var agentPos = ByteMapController.Instance.GetPositionOfUnit(agent.GetUnit);

        var validTiles = TargetingStrategy.ComputeCellsInAbilityRange(tarPos, abilityRange);
        var pathToTar = CalculatePath(agentPos, tarPos, true);
        int distanceToTar = pathToTar == null ? int.MaxValue : pathToTar.Count;

        // only worry about "at range" or "at melee" if agent can move
        if (beliefs.GetStates.ContainsKey(GoapStates.CanMove.ToString()) &&
            !beliefs.GetStates.ContainsKey(GoapStates.IsCornered.ToString()))
        {
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
        }
        //if (distanceToTar > abilityRange)
        if (!validTiles.Contains(agentPos))
        {
            beliefs.ModifyState(GoapStates.OutOfRange.ToString(), 1);
            beliefs.RemoveState(GoapStates.InRange.ToString());
            return false;
        }

        beliefs.ModifyState(GoapStates.InRange.ToString(), 1);
        beliefs.RemoveState(GoapStates.OutOfRange.ToString());
        return true;
    }
    public static bool CheckRange(GoapAgent agent, Unit target, int abilityRange, ref WorldStates beliefs)
    {
        // if target is agent unit (self)
        if (agent.GetUnit == target)
        {
            beliefs.ModifyState(GoapStates.InRange.ToString(), 1);
            beliefs.RemoveState(GoapStates.OutOfRange.ToString());

            beliefs.ModifyState(GoapStates.AtMelee.ToString(), 1);
            beliefs.RemoveState(GoapStates.AtRange.ToString());
            return true;
        }
        var unitMover = agent.GetComponent<UnitMovementController>();

        var tarPos = ByteMapController.Instance.GetPositionOfUnit(target);
        var agentPos = ByteMapController.Instance.GetPositionOfUnit(agent.GetUnit);

        var validTiles = TargetingStrategy.ComputeCellsInAbilityRange(tarPos, abilityRange);
        var pathToTar = CalculatePath(agentPos, tarPos);
        int distanceToTar = pathToTar == null ? int.MaxValue : pathToTar.Count;

        // only worry about "at range" or "at melee" if agent can move
        if (beliefs.GetStates.ContainsKey(GoapStates.CanMove.ToString()) &&
            !beliefs.GetStates.ContainsKey(GoapStates.IsCornered.ToString()))
        {
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
        }

        if (!validTiles.Contains(agentPos))
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
        //var tarPos = ConvertToGridFromIsometric(agent.GetCurrentTarget.transform.localPosition);
        var tarPos = ByteMapController.Instance.GetPositionOfUnit(agent.GetCurrentTarget);

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
    public static bool CheckIfInLOS(GoapAgent agent, Unit target, ref WorldStates beliefs)
    {
        var agentPos = ConvertToGridFromIsometric(agent.transform.localPosition);
        var tarPos = ByteMapController.Instance.GetPositionOfUnit(target);
        //var tarPos = ConvertToGridFromIsometric(target.transform.localPosition);

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
        if (!agent.TryGetComponent(out Unit unit)) return int.MaxValue;

        var distToTar = CalculatePath(agent, target).Count;
        int maxAP = unit.GetAP;
        float distRatio = distToTar / (float)maxAP;
        return distRatio;//Mathf.Clamp(distRatio, 0, 1);
    }
    public static float GetAdjustedMovementDistRatio(Vector2Int agentPos, Vector2Int targetPos, Unit unit)
    {
        if (unit == null) return int.MaxValue;

        var distToTar = CalculatePath(agentPos, targetPos).Count;
        int maxAP = unit.GetAP;
        float distRatio = distToTar / (float)maxAP;
        return distRatio;//Mathf.Clamp(distRatio, 0, 1);
    }
    public static bool CheckIfRooted(Unit agentUnit, ref WorldStates beliefs)
    {
        if (agentUnit.GetCanMove)
            beliefs.ModifyState(GoapStates.CanMove.ToString(), 1);
        else
            beliefs.RemoveState(GoapStates.CanMove.ToString());

        return agentUnit.GetCanMove;
    }
}
