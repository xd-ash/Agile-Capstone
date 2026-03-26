using AStarPathfinding;
using Unity.VisualScripting;
using UnityEngine;
using static IsoMetricConversions;

public class OtherMoveAction : GoapAction
{
    private FindPathAStar aStar;

    public override bool PrePerform(ref WorldStates beliefs)
    {
        aStar = agent.GetComponent<FindPathAStar>();

        if (agent.damageAbility.GetRange > 1)
        {
            beliefs.ModifyState(GoapStates.OutOfAP.ToString(), 1);
            return false;
        }

        var tarPos = ConvertToGridFromIsometric(agent.curtarget.transform.localPosition);
        var tempPath = aStar.CalculatePath(tarPos);
        aStar.CalculatePath(tempPath[^1].location.ToVector());// this is sloppy

        return true;
    }
    public override void Perform()
    {
        aStar.OnStartUnitMove(() =>
        {
            //Debug.Log("test");
            agent.CompleteAction();
        });
    }

    public override void PostPerform(ref WorldStates beliefs)
    {
        beliefs.ModifyState(GoapStates.OutOfAP.ToString(), 1);
    }
}
