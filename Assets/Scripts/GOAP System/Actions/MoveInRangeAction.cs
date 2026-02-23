using AStarPathfinding;
using static IsoMetricConversions;
using static GOAPDeterminationMethods;

public class MoveInRangeAction : GoapAction
{
    private FindPathAStar aStar;

    public MoveInRangeAction(GoapAgent agent) : base(agent) { }

    public override bool PrePerform(ref WorldStates beliefs)
    {
        if (beliefs.GetStates.ContainsKey(GoapStates.InRange.ToString())) return false;

        aStar = _agent.GetComponent<FindPathAStar>();
        Unit unit = _agent.unit;
        int dmgAbilRange = _agent.damageAbility.GetRange;

        var tarPos = ConvertToGridFromIsometric(_agent.GetCurrentTarget.transform.localPosition);
        var tempPath = aStar.CalculatePath(tarPos);
        int distanceToTar = tempPath.Count;
        //Debug.Log($"tarPos: {tarPos} | distancetoTar: {distanceToTar}");

        if (_agent.damageAbility == null)
            return false;

        //return true if unit cannot get into ability range and calc path to closest tile
        if ((distanceToTar - dmgAbilRange) > unit.GetAP)
            return true;

        int inRangeTileIndex = dmgAbilRange;

        // calc new path to tile just within ability range
        aStar.CalculatePath(tempPath[inRangeTileIndex].location.ToVector());

        return true;
    }
    public override void Perform()
    {
        aStar.OnStartUnitMove(() =>
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
}
