using AStarPathfinding;
using System.Collections.Generic;
using System.Linq;
using static IsoMetricConversions;
using static GOAPDeterminationMethods;

public class ChooseTargetAction : GoapAction
{
    private Dictionary<int, Unit> distancesToUnits;

    public ChooseTargetAction(GoapAgent agent) : base(agent) { }

    public override bool PrePerform(ref WorldStates beliefs)
    {
        distancesToUnits = new();
        FindPathAStar aStar = agent.GetComponent<FindPathAStar>();

        foreach (var u in TurnManager.GetUnitTurnOrder)
        {
            if (u == null || u.GetTeam == agent.unit.GetTeam) continue;
            //Debug.Log($"Unit: {u.name} - Pos {u.transform.localPosition}");

            var tarPos = ConvertToGridFromIsometric(u.transform.localPosition); 
            var tempPath = aStar.CalculatePath(tarPos);

            distancesToUnits.Add(tempPath.Count, u);
        }

        return distancesToUnits.Count > 0 ? true : false;
    }
    public override void Perform()
    {
        int min = distancesToUnits.Min(x => x.Key);
        agent.SetCurrentTarget(distancesToUnits[min]);

        //Debug.Log($"target: {(agent.curtarget != null ? agent.curtarget.name : "null")}");

        agent.CompleteAction();
    }
    public override void PostPerform(ref WorldStates beliefs)
    {
        beliefs.ModifyState(GoapStates.HasTarget.ToString(), 1);
        beliefs.RemoveState(GoapStates.NoTarget.ToString());

        CheckIfInRange(agent, agent.damageAbility.GetRange, ref beliefs);
        CheckIfInLOS(agent, ref beliefs);
    }
}
