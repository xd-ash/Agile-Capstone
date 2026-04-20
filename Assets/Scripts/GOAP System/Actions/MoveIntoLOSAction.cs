using AStarPathfinding;
using static IsoMetricConversions;
using static CombatMath;
using static GOAPDeterminationMethods;

public class MoveIntoLOSAction : GoapAction
{
    private UnitMovementController _unitMover;

    public MoveIntoLOSAction(string overrideName = "") : base(overrideName)
    {

    }
    public MoveIntoLOSAction(GoapAction refAction) : base(refAction)
    {

    }
    public override bool PrePerform(ref WorldStates beliefs)
    {
        if (beliefs.GetStates.ContainsKey(GoapStates.HasLOS.ToString()) || _agent.GetCurrentTarget == null) return false;

        _unitMover = _agent.GetComponent<UnitMovementController>();
        Unit unit = _agent.unit;
        int dmgAbilRange = _agent.GetHarmfulAbility.GetRange;

        var tarPos = ConvertToGridFromIsometric(_agent.GetCurrentTarget.transform.localPosition);
        var tempPath = _unitMover.CalculatePath(tarPos);
        int distanceToTar = tempPath.Count;

        for (int i = tempPath.Count - 1; i >= 0; i--)
        {
            var tempPos = tempPath[i].location.ToVector();
            if (HasLineOfSight(tempPos, tarPos))
            {
                _unitMover.CalculatePath(tempPos);
                return true;
            }
        }

        _unitMover.CalculatePath(tarPos); //default to walking to target if los cannot be reached?
        return false;
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
        //check range
        beliefs.ModifyState(GoapStates.HasLOS.ToString(), 1);
        beliefs.RemoveState(GoapStates.NoLOS.ToString());

        CheckRange(_agent, _agent.GetCurrentAbility.GetRange, ref beliefs);
    }

    public override float EvaluateCost(string tempGoal, Unit tempTarget)
    {
        if (_agent == null || tempTarget == null) return _cost;

        var distRatio = GetAdjustedMovementDistRatio(_agent.transform, tempTarget.transform);
        return _cost * distRatio * _costMultiplier;
    }
}
