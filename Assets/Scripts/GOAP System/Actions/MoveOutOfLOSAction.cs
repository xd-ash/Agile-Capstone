using AStarPathfinding;
using UnityEngine;
using static IsoMetricConversions;
using static GOAPDeterminationMethods;
using System.Collections.Generic;

public class MoveOutOfLOSAction : GoapAction
{
    private UnitMovementController _unitMover;
    private Vector2Int _hidePos;

    public MoveOutOfLOSAction(string overrideName = "") : base(overrideName)
    {

    }
    public MoveOutOfLOSAction(GoapAction refAction) : base(refAction)
    {

    }

    public override bool PrePerform(ref WorldStates beliefs)
    {
        if (!CheckForAP(_agent.GetUnit, ref beliefs)) return false;

        var reachableTiles = MovementRangeCalculator.ComputeReachableCells(_agent.GetUnit);
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
        var reachableCells = MovementRangeCalculator.ComputeReachableCells(_agent.GetUnit);
        if (target == null) return ConvertToGridFromIsometric(_agent.transform.localPosition);
        var targetTile = ByteMapController.Instance.GetPositionOfUnit(target);

        var hidePos = -Vector2Int.one;
        int bestDistCount = int.MaxValue;
        foreach (var tile in reachableCells)
        {
            var pathToTarget = FindPathAStar.CalculatePath(tile, targetTile);

            if (pathToTarget == null || pathToTarget.Count == 0) continue;
            if (pathToTarget.Count >= bestDistCount) continue;
            if (CombatMath.HasLineOfSight(tile, targetTile)) continue;
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

    public override float EvaluateCost(string tempGoal, Unit tempTarget)
    {
        if (_agent == null || tempTarget == null) return _cost;

        bool isOutOfLOS = _agent.CheckForState(GoapStates.NoLOS);

        float distRatio = 1f;
        if (!isOutOfLOS)
        {
            var agentPos = ConvertToGridFromIsometric(_agent.transform.localPosition);
            distRatio = GetAdjustedMovementDistRatio(agentPos, DetermineHidePos(tempTarget), _agent.GetUnit);
        }
        return _cost * distRatio * _costMultiplier;
    }
}
