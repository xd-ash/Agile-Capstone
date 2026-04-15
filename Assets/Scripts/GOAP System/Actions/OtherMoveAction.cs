using AStarPathfinding;
using static IsoMetricConversions;

public class OtherMoveAction : GoapAction
{
    private UnitMovementController _unitMover;

    public OtherMoveAction(string overrideName = "") : base(overrideName)
    {

    }
    public OtherMoveAction(GoapAction refAction) : base(refAction)
    {

    }
    public override bool PrePerform(ref WorldStates beliefs)
    {
        _unitMover = _agent.GetComponent<UnitMovementController>();

        if (_agent.GetDamageAbility.GetRange > 1)
        {
            beliefs.ModifyState(GoapStates.OutOfAP.ToString(), 1);
            return false;
        }

        var tarPos = ConvertToGridFromIsometric(_agent.GetCurrentTarget.transform.localPosition);
        var tempPath = _unitMover.CalculatePath(tarPos);
        _unitMover.CalculatePath(tempPath[^1].location.ToVector());// this is sloppy

        return true;
    }
    public override void Perform()
    {
        _unitMover.OnStartUnitMove(() =>
        {
            //Debug.Log("test");
            _agent.CompleteAction();
        });
    }

    public override void PostPerform(ref WorldStates beliefs)
    {
        beliefs.ModifyState(GoapStates.OutOfAP.ToString(), 1);
    }

    public override float EvaluateCost(string tempGoal, Unit tempTarget)
    {
        if (_agent == null || tempTarget == null) return _cost;

        return _cost;
    }
}
