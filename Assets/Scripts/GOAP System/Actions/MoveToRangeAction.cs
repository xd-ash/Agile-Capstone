using AStarPathfinding;
using UnityEngine;
using static GOAPDeterminationMethods;
using static IsoMetricConversions;
using static CombatMath;
using System;

public class MoveToRangeAction : GoapAction
{
    private Vector2Int _movePos;

    public MoveToRangeAction(string overrideName = "") : base(overrideName)
    {

    }
    public MoveToRangeAction(GoapAction refAction) : base(refAction)
    {

    }
    public override bool PrePerform(ref WorldStates beliefs)
    {
        Debug.Log($"test");
        if(!CheckForAP(_agent.GetUnit, ref beliefs)) return false;
        var target = _agent.GetCurrentTarget;
        if (target == null)
        {
            Debug.Log($"test2");
            return false;
        }
        Debug.Log($"test3");

        var tmp = DetermineMovePos(target);
        _movePos = tmp.Item1;
        int furthestDist = tmp.Item2;
        return furthestDist != 0;
    }
    private Tuple<Vector2Int, int> DetermineMovePos(Unit target)
    {
        var agentTile = ConvertToGridFromIsometric(_agent.transform.localPosition);
        if (target == null) return new(agentTile, 0);
        //var targetTile = ConvertToGridFromIsometric(target.transform.localPosition);
        var targetTile = ByteMapController.Instance.GetPositionOfUnit(target);
        var reachableTiles = MovementRangeCalculator.ComputeReachableCells(_agent.GetUnit);

        int furthestDist = 0;
        Vector2Int movePos = agentTile;
        foreach (var reachableTile in reachableTiles)
        {
            var dist = FindPathAStar.CalculatePath(reachableTile, targetTile, true).Count;
            if (dist <= furthestDist || dist > GetAtRangeThreshold) continue;
            if (!HasLineOfSight(reachableTile, targetTile) || FindPathAStar.CalculatePath(agentTile, targetTile).Count > dist) continue;
            furthestDist = dist;
            movePos = reachableTile;
        }
        return new(movePos, furthestDist);
    }
    public override void Perform()
    {
        var unitMover = _agent.GetComponent<UnitMovementController>();

        var count = unitMover?.CalculatePath(_movePos).Count;
        
        unitMover.OnStartUnitMove(() =>
        {
            _agent.CompleteAction();
        });
    }
    public override void PostPerform(ref WorldStates beliefs)
    {
        CheckRange(_agent, _agent.GetCurrentAbility.GetRange, ref beliefs);
    }

    public override float EvaluateCost(string tempGoal, Unit tempTarget)
    {
        if (_agent == null || tempTarget == null) return _cost;
        var moveTuple = DetermineMovePos(tempTarget);
        var agentTile = ConvertToGridFromIsometric(_agent.transform.localPosition);
        var distRatio = GetAdjustedMovementDistRatio(agentTile, moveTuple.Item1, _agent.GetUnit);
        return _cost * distRatio * _costMultiplier;
    }
}
