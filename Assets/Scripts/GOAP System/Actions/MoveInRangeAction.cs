using AStarPathfinding;
using CardSystem;
using UnityEngine;
using static IsoMetricConversions;
using static GOAPDeterminationMethods;

public class MoveInRangeAction : GoapAction
{
    private FindPathAStar aStar;

    public override bool PrePerform(ref WorldStates beliefs)
    {
        if (beliefs.states.ContainsKey(GoapStates.InRange.ToString())) return false;

        aStar = agent.GetComponent<FindPathAStar>();
        Unit unit = agent.unit;
        int dmgAbilRange = agent.damageAbility.RootNode.GetRange;

        var tarPos = ConvertToGridFromIsometric(agent.curtarget.transform.localPosition);
        var tempPath = aStar.CalculatePath(tarPos);
        int distanceToTar = tempPath.Count;
        //Debug.Log($"tarPos: {tarPos} | distancetoTar: {distanceToTar}");

        if (agent.damageAbility == null)
            return false;

        //return true if unit cannot get into ability range and calc path to closest tile
        if ((distanceToTar - dmgAbilRange) > unit.ap)
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
            agent.CompleteAction();
        });
    }

    public override void PostPerform(ref WorldStates beliefs)
    {
        beliefs.ModifyState(GoapStates.InRange.ToString(), 1);
        beliefs.RemoveState(GoapStates.OutOfRange.ToString());

        CheckIfInLOS(agent, ref beliefs);
    }
}
