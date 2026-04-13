using AStarPathfinding;
using UnityEngine;
using static IsoMetricConversions;
using static GOAPDeterminationMethods;

public class MoveOutOfLOSAction : GoapAction
{
    private UnitMovementController _unitMover;
    private Vector2Int _hidePos;

    public MoveOutOfLOSAction(string overrideName = "") : base(overrideName)
    {

    }

    public override bool PrePerform(ref WorldStates beliefs)
    {
        if (!CheckForAP(_agent.unit, ref beliefs)) return false;

        var reachableTiles = MovementRangeCalculator.ComputeReachableCells(_agent.unit);
        _unitMover = _agent.GetComponent<UnitMovementController>();
        var target = _agent.GetCurrentTarget;
        if (target == null) return false;
        _hidePos = DetermineHidePos(target);

        if (_hidePos == -Vector2Int.one)
        {
            beliefs.ModifyState(GoapStates.OutOfAP.ToString(), 1);
            beliefs.RemoveState(GoapStates.HasAP.ToString());
            return false;
        }
        return true;
    }
    private Vector2Int DetermineHidePos(Unit target)
    {
        var reachableTiles = MovementRangeCalculator.ComputeReachableCells(_agent.unit);
        if (target == null) return ConvertToGridFromIsometric(_agent.transform.localPosition);
        var targetTile = ByteMapController.Instance.GetPositionOfUnit(target);
        //var targetTile = ConvertToGridFromIsometric(target.transform.localPosition);

        //Debug.Log($"Target: {target.name}, reachableTiles: {reachableTiles.Count}, targetTile: {targetTile}");

        var hidePos = -Vector2Int.one;
        int bestDistCount = int.MaxValue;
        foreach (var tile in reachableTiles)
        {
            var pathToTarget = FindPathAStar.CalculatePath(tile, targetTile, true);

            if (pathToTarget == null || pathToTarget.Count == 0) continue;
            if (CombatMath.HasLineOfSight(tile, targetTile)) continue;
            if (pathToTarget.Count >= bestDistCount) continue;
            hidePos = tile;
            bestDistCount = pathToTarget.Count;
        }

        //Debug.Log($"hidePos:{hidePos}");
        return hidePos;
    }
    public override void Perform()
    {
        if (_hidePos == -Vector2Int.one || _agent == null) return;
        _unitMover.CalculatePath(_hidePos);
        //Debug.Log($"hidePos: {_hidePos}");

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

    private Unit _tempTarget;
    private Vector2Int _tempHidePos;

    public override float EvaluateCost(string tempGoal, Unit tempTarget)
    {
        if (_agent == null || tempTarget == null) return _cost;

        if (_tempTarget != tempTarget)
        {
            _tempHidePos = DetermineHidePos(tempTarget);
            _tempTarget = tempTarget;
        }

        var agentPos = ConvertToGridFromIsometric(_agent.transform.localPosition);
        var distRatio = GetAdjustedMovementDistRatio(agentPos, _tempHidePos, _agent.unit);
        //var distRatio = GetAdjustedMovementDistRatio(_agent.transform, tempTarget.transform);
        return _cost * distRatio * _costMultiplier;
    }
}
