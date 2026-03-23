using AStarPathfinding;
using UnityEngine;
using static GOAPDeterminationMethods;
using static IsoMetricConversions;
using static CombatMath;

public class MoveToRangeAction : GoapAction
{
    private Vector2Int _movePos;

    public override bool PrePerform(ref WorldStates beliefs)
    {
        if(!CheckForAP(_agent.unit, ref beliefs)) return false;
        var reachableTiles = MovementRangeCalculator.ComputeReachableCells(_agent.unit);
        var target = _agent.GetCurrentTarget;
        if (target == null) return false;
        var agentTile = ConvertToGridFromIsometric(_agent.transform.localPosition);
        var targetTile = ConvertToGridFromIsometric(target.transform.localPosition);

        int furthestDist = 0;
        foreach (var reachableTile in reachableTiles)
        {
            var dist = FindPathAStar.CalculatePath(reachableTile, targetTile, true).Count;
            if (dist <= furthestDist || dist > GetAtRangeThreshold) continue;
            if (!HasLineOfSight(reachableTile, targetTile) || FindPathAStar.CalculatePath(agentTile, targetTile).Count > dist) continue;
            furthestDist = dist;
            _movePos = reachableTile;
        }

        return furthestDist != 0;
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
}
