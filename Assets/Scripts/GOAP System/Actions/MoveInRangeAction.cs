using CardSystem;
using UnityEngine;
using static GOAPDeterminationMethods;
using static IsoMetricConversions;
using static AStarPathfinding.FindPathAStar;

public class MoveInRangeAction : GoapAction
{
    private UnitMovementController _unitMover;

    public MoveInRangeAction(string overrideName = "") : base(overrideName)
    {

    }
    public MoveInRangeAction(GoapAction refAction) : base(refAction)
    {

    }
    public override bool PrePerform(ref WorldStates beliefs)
    {
        if (beliefs.GetStates.ContainsKey(GoapStates.InRange.ToString())) return false;
        
        _unitMover = _agent.GetComponent<UnitMovementController>();

        bool isAttacking = _agent.GetCurrentGoal.key == GoapGoals.KillPlayer.ToString();
        var ability = isAttacking ? _agent.GetHarmfulAbility : _agent.GetHelpfulAbility;
        if (ability == null) return false;

        Unit curTar = isAttacking ? _agent.GetEnemyTarget : _agent.GetAllyTarget;
        if (curTar == _agent.GetUnit) return false;

        var tarPos = ByteMapController.Instance.GetPositionOfUnit(curTar);
        var agentPos = ByteMapController.Instance.GetPositionOfUnit(_agent.GetUnit);

        var closestTile = GetClosestInRangeTile(curTar, tarPos, agentPos, ability.GetRange);

        _unitMover.CalculatePath(closestTile);
        return true;
    }

    private Vector2Int GetClosestInRangeTile(Unit target, Vector2Int tarPos, Vector2Int agentPos, int range)
    {
        var validInRangeTiles = TargetingStrategy.ComputeCellsInAbilityRange(tarPos, range, true);

        Vector2Int closestTile = tarPos;
        int closestDist = int.MaxValue;
        foreach (var tile in validInRangeTiles)
        {
            if (tile == tarPos) continue;
            var distToTile = CalculatePath(agentPos, tile).Count;
            if (distToTile >= closestDist) continue;
            closestDist = distToTile;
            closestTile = tile;
        }
        return closestTile;
    }
    public override void Perform()
    {
        _unitMover.OnStartUnitMove(() =>
        {
            _agent.CompleteAction();
        });
    }

    public override void PostPerform(ref WorldStates beliefs)
    {
        beliefs.ModifyState(GoapStates.InRange.ToString(), 1);
        beliefs.RemoveState(GoapStates.OutOfRange.ToString());

        CheckIfInLOS(_agent, ref beliefs);
    }

    public override float EvaluateCost(string tempGoal, Unit tempTarget)
    {
        if (_agent == null || tempTarget == null) return _cost;


        var tarPos = ByteMapController.Instance.GetPositionOfUnit(tempTarget);
        var agentPos = ByteMapController.Instance.GetPositionOfUnit(_agent.GetUnit);
        var ability = tempGoal == GoapGoals.KillPlayer.ToString() ? _agent.GetHarmfulAbility : _agent.GetHelpfulAbility;
        if (ability == null) return float.MaxValue;

        var closestTile = GetClosestInRangeTile(tempTarget, tarPos, agentPos, ability.GetRange);

        var distRatio = GetAdjustedMovementDistRatio(agentPos, closestTile, _agent.GetUnit);

        //bandaid fix, if target is this unit make moving into range cost a lot so it isn't chosen in plan
        bool isStayingAlive = tempGoal == GoapGoals.StayAliveSelfFocus.ToString();
        if (isStayingAlive) 
            distRatio = float.MaxValue;
        return _cost * distRatio * _costMultiplier;
    }
}
