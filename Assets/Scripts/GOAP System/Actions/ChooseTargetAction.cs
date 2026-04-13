using System.Collections.Generic;
using System.Linq;
using static IsoMetricConversions;
using static GOAPDeterminationMethods;
using static AStarPathfinding.FindPathAStar;
using UnityEngine;

public class ChooseTargetAction : GoapAction
{
    public ChooseTargetAction(string overrideName = "") : base(overrideName)
    {

    }
    public override bool PrePerform(ref WorldStates beliefs)
    {
        var distancesToEnemies = GrabUnitDistances(Team.Friendly, _agent);
        return distancesToEnemies.Count > 0 ? true : false; // change this
    }
    private static Dictionary<int, Unit> GrabUnitDistances(Team team, GoapAgent agent)
    {
        Dictionary<int, Unit> distToUnits = new();

        foreach (var u in TurnManager.GetUnitTurnOrder)
        {
            if (u == null || u.GetTeam != team) continue;
            if (u == agent.unit) continue;

            var tempPath = CalculatePath(agent.transform, u.transform);
            distToUnits.Add(tempPath.Count, u);
        }
        return distToUnits;
    }
    public static Unit[] GetCurrentTargets(string curGoal, GoapAgent agent)
    {
        var enemiesDists = GrabUnitDistances(Team.Friendly, agent);
        var allyDists = GrabUnitDistances(Team.Enemy, agent);
        Unit minIndexEnemy = null, minIndexAlly = agent.unit;
        int minIndex = 0;
       
        minIndex = enemiesDists.Min(x => x.Key);
        minIndexEnemy = enemiesDists[minIndex];

        if (curGoal == GoapGoals.KeepAlliesAlive.ToString() && allyDists.Count > 0)
        {
            minIndex = allyDists.Min(x => x.Key);
            minIndexAlly = allyDists[minIndex];
        }

        return new Unit[2] { minIndexEnemy, minIndexAlly };
    }
    public override void Perform()
    {
        var curGoal = _agent.GetCurrentGoal.key;
        var targets = GetCurrentTargets(curGoal, _agent);
        _agent.SetCurrentTargets(targets[0], targets[1]);

        _agent.CompleteAction();
    }
    public override void PostPerform(ref WorldStates beliefs)
    {
        beliefs.ModifyState(GoapStates.HasTarget.ToString(), 1);
        beliefs.RemoveState(GoapStates.NoTarget.ToString());

        CheckRange(_agent, _agent.GetCurrentAbility.GetRange, ref beliefs);
        CheckIfInLOS(_agent, ref beliefs);
    }

    public override float EvaluateCost(string tempGoal, Unit tempTarget)
    {
        return _cost;
    }
}
