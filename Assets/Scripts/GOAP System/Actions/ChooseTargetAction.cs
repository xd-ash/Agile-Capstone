using System.Collections.Generic;
using System.Linq;
using static IsoMetricConversions;
using static GOAPDeterminationMethods;

public class ChooseTargetAction : GoapAction
{
    private Dictionary<int, Unit> _distancesToEnemies;
    private Dictionary<int, Unit> _distancesToAllies;

    public override bool PrePerform(ref WorldStates beliefs)
    {
        _distancesToEnemies = new();
        _distancesToAllies = new();
        var unitMover = _agent.GetComponent<UnitMovementController>();

        foreach (var u in TurnManager.GetUnitTurnOrder)
        {
            if (u == null) continue;
            //Debug.Log($"Unit: {u.name} - Pos {u.transform.localPosition}");

            var tarPos = ConvertToGridFromIsometric(u.transform.localPosition); 
            var tempPath = unitMover.CalculatePath(tarPos);

            if (u == _agent.unit) continue;

            if (u.GetTeam == _agent.unit.GetTeam)
                _distancesToAllies.Add(tempPath.Count, u);
            else
                _distancesToEnemies.Add(tempPath.Count, u);
        }

        return _distancesToEnemies.Count > 0 ? true : false;
    }
    public override void Perform()
    {
        int minEnemyIndex = _distancesToEnemies.Min(x => x.Key);
        var minEnemy = _distancesToEnemies[minEnemyIndex];

        int minAllyIndex;
        Unit minAlly = null;
        if (_distancesToAllies.Count > 0)
        {
            minAllyIndex = _distancesToAllies.Min(x => x.Key);
            minAlly = _distancesToAllies[minAllyIndex];
        }

        var curGoal = _agent.GetHighestGoalDesire().key;
        if (curGoal == GoapGoals.StayAlive.ToString())
            minAlly = _agent.unit;

        _agent.SetCurrentTargets(minEnemy, minAlly);

        //Debug.Log($"target: {(agent.curtarget != null ? agent.curtarget.name : "null")}");

        _agent.CompleteAction();
    }
    public override void PostPerform(ref WorldStates beliefs)
    {
        beliefs.ModifyState(GoapStates.HasTarget.ToString(), 1);
        beliefs.RemoveState(GoapStates.NoTarget.ToString());

        CheckRange(_agent, _agent.GetCurrentAbility.GetRange, ref beliefs);
        CheckIfInLOS(_agent, ref beliefs);
    }
}
