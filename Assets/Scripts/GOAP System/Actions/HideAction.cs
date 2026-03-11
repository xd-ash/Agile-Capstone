using AStarPathfinding;
using UnityEngine;
using static IsoMetricConversions;

public class HideAction : GoapAction
{
    private UnitMovementController _unitMover;
    private Vector2Int _hidePos;

    public override bool PrePerform(ref WorldStates beliefs)
    {
        var reachableTiles = MovementRangeCalculator.ComputeReachableCells(_agent.unit);
        _unitMover = _agent.GetComponent<UnitMovementController>();
        var target = _agent.GetCurrentTarget;
        if (target == null) return false;
        var targetTile = ConvertToGridFromIsometric(target.transform.localPosition);

        _hidePos = -Vector2Int.one; // maybe change this?
        int bestDistCount = -1;
        foreach (var tile in reachableTiles)
        {
            var pathToTarget = FindPathAStar.CalculatePath(tile, targetTile);
            if (pathToTarget == null || pathToTarget.Count == 0) continue;
            if (!CombatMath.HasLineOfSight(tile, targetTile)) continue;

            if (pathToTarget.Count <= bestDistCount) continue;
            _hidePos = tile;
            bestDistCount = pathToTarget.Count;
        }

        if (_hidePos == -Vector2Int.one)
            return false;
        return true;
    }
    public override void Perform()
    {
        if (_hidePos == -Vector2Int.one || _agent == null) return;
        _unitMover.CalculatePath(_hidePos);

        _unitMover.OnStartUnitMove(() =>
        {
            _agent.CompleteAction();
        });
    }
    public override void PostPerform(ref WorldStates beliefs)
    {
        beliefs.ModifyState(GoapStates.NoLOS.ToString(), 1);
        beliefs.RemoveState(GoapStates.HasLOS.ToString());
    }
}
