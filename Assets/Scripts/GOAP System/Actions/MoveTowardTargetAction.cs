using AStarPathfinding;
using Unity.VisualScripting;
using UnityEngine;
using static IsoMetricConversions;

public class MoveTowardTargetAction : GoapAction
{
    private FindPathAStar aStar;

    public override bool PrePerform()
    {
        aStar = agent.GetComponent<FindPathAStar>();
        Unit unit = agent.unit;
        int dmgAbilRange = agent.damageAbility.RootNode.GetRange;

        var tarPos = ConvertToGridFromIsometric(agent.curtarget.transform.localPosition);
        var tempPath = aStar.CalculatePath(tarPos);
        int distanceToTar = tempPath.Count;

        if (agent.damageAbility == null)
        {
            Debug.Log($"dmg abil is null for {agent.gameObject.name}'s goap agent"); //remove me later
            return false;
        }

        //return false if unit cannot get into ability range
        if ((distanceToTar - dmgAbilRange) > unit.ap)
            return false;

        int inRangeTileIndex = tempPath.Count - dmgAbilRange;

        // calc new path to tile just within ability range
        aStar.CalculatePath(tempPath[inRangeTileIndex].location.ToVector());
        return true;
    }
    public override void Perform()
    {
        Debug.Log($"mvoe in range Perform");

        aStar.OnStartUnitMove(() =>
        {
            agent.CompleteAction();
        });
    }

    public override void PostPerform(ref WorldStates beliefs)
    {
        beliefs.ModifyState(GoapStates.InRange.ToString(), 1);
        //CheckForAP(agent.unit, ref beliefs);
    }
}
