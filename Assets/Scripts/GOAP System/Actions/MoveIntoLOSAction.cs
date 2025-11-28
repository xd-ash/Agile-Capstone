using AStarPathfinding;
using Unity.VisualScripting;
using UnityEngine;
using static IsoMetricConversions;
using static CombatMath;
using static GOAPDeterminationMethods;

public class MoveIntoLOSAction : GoapAction
{
    private FindPathAStar aStar;

    public override bool PrePerform(ref WorldStates beliefs)
    {
        aStar = agent.GetComponent<FindPathAStar>();
        Unit unit = agent.unit;
        int dmgAbilRange = agent.damageAbility.RootNode.GetRange;

        var tarPos = ConvertToGridFromIsometric(agent.curtarget.transform.localPosition);
        var tempPath = aStar.CalculatePath(tarPos);
        int distanceToTar = tempPath.Count;

        for (int i = tempPath.Count - 1; i >= 0; i--)
        {
            var tempPos = tempPath[i].location.ToVector();
            if (HasLineOfSight(tempPos, tarPos))
            {
                aStar.CalculatePath(tempPos);
                return true;
            }
        }

        aStar.CalculatePath(tarPos); //default to walking to target if los cannot be reached?
        return false;
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
        //check range
        beliefs.ModifyState(GoapStates.HasLOS.ToString(), 1);
        beliefs.RemoveState(GoapStates.NoLOS.ToString());

        CheckIfInRange(agent, agent.damageAbility.RootNode.GetRange, ref beliefs);
    }
}
