using System.Collections.Generic;
using System.Linq;
using static IsoMetricConversions;
using static GOAPDeterminationMethods;
using static AStarPathfinding.FindPathAStar;
using UnityEngine;

public class ChooseTargetAction : GoapAction
{
    public override bool PrePerform(ref WorldStates beliefs)
    {
        var distancesToEnemies = GrabUnitDistances(Team.Friendly);
        Debug.Log("preperform: " + (distancesToEnemies.Count > 0 ? true : false));
        return distancesToEnemies.Count > 0 ? true : false; // change this
    }
    private Dictionary<int, Unit> GrabUnitDistances(Team team)
    {
        Dictionary<int, Unit> distToUnits = new();

        foreach (var u in TurnManager.GetUnitTurnOrder)
        {
            if (u == null || u.GetTeam != team) continue;
            if (u == _agent.unit) continue;

            var tempPath = CalculatePath(_agent.transform, u.transform);
            distToUnits.Add(tempPath.Count, u);
        }
        return distToUnits;
    }
    public Unit[] GetCurrentTargets(string curGoal)
    {
        var enemiesDists = GrabUnitDistances(Team.Friendly);
        var allyDists = GrabUnitDistances(Team.Enemy);
        Unit minIndexEnemy = null, minIndexAlly = null;
        int minIndex = 0;

        minIndex = enemiesDists.Min(x => x.Key);
        minIndexEnemy = enemiesDists[minIndex];

        if (curGoal == GoapGoals.KeepAlliesAlive.ToString() && allyDists.Count > 0)
        {
            minIndex = allyDists.Min(x => x.Key);
            minIndexAlly = allyDists[minIndex];
        }
        else if (curGoal == GoapGoals.StayAlive.ToString())
            minIndexAlly = _agent.unit;

        return new Unit[2] { minIndexEnemy, minIndexAlly };
    }
    public override void Perform()
    {
        var curGoal = _agent.GetHighestGoalDesire().key;
        var targets = GetCurrentTargets(curGoal);
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

    public override float EvaluateCost(Unit tempTarget)
    {
        return _cost;
    }
}
