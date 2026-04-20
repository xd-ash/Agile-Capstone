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
    public ChooseTargetAction(GoapAction refAction) : base(refAction)
    {

    }
    public override bool PrePerform(ref WorldStates beliefs)
    {
        var distancesToEnemies = GrabUnitDistances(Team.Friendly, _agent);
        return distancesToEnemies.Count > 0 ? true : false; // change this
    }
    private static Dictionary<Unit, int> GrabUnitDistances(Team team, GoapAgent agent)
    {
        Dictionary<Unit, int> distToUnits = new();

        foreach (var u in TurnManager.GetUnitTurnOrder)
        {
            if (u == null) continue;
            Debug.Log($"unit name: {u.name}, team: {u.GetTeam}, expectedTeam: {team.ToString()}, agent unit name: {agent.GetUnit?.name}");

            if (u.GetTeam != team) continue;
            Debug.Log("Test");
            if (u == agent.GetUnit) continue;
            Debug.Log("Test2");

            var tempPath = CalculatePath(agent.transform, u.transform);

            if (!distToUnits.ContainsKey(u))
                distToUnits.Add(u, tempPath.Count);
        }
        return distToUnits;
    }
    public static Unit[] GetCurrentTargets(string curGoal, GoapAgent agent)
    {
        var enemiesDists = GrabUnitDistances(Team.Friendly, agent);
        var allyDists = GrabUnitDistances(Team.Enemy, agent);
        Unit minDistEnemy = null, minDistAlly = agent.GetUnit;
        int minDist = int.MaxValue;

        Debug.Log($"enemy count: {enemiesDists.Count}");
        for (int i = 0; i < enemiesDists.Count; i++)
        {
            var kvp = enemiesDists.ElementAt(i);
            if (kvp.Value >= minDist) continue;
            minDist = kvp.Value;
            minDistEnemy = kvp.Key;
        }

        minDist = int.MaxValue;
        if (curGoal == GoapGoals.KeepAlliesAlive.ToString() && allyDists.Count > 0)
        {
            for (int i = 0; i < allyDists.Count; i++)
            {
                var kvp = allyDists.ElementAt(i);
                if (kvp.Value >= minDist) continue;
                minDist = kvp.Value;
                minDistAlly = kvp.Key;
            }
        }

        return new Unit[2] { minDistEnemy, minDistAlly };
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
