using AStarPathfinding;
using UnityEngine;
using static IsoMetricConversions;
using static GOAPDeterminationMethods;

public class HideAction : GoapAction
{
    public override bool PrePerform(ref WorldStates beliefs)
    {
        return !CheckIfInLOS(_agent, ref beliefs);
    }
    public override void Perform()
    {
        _agent.ClearPlanner();
        TurnManager.Instance.EndEnemyTurn();
        _agent.CompleteAction();
    }
    public override void PostPerform(ref WorldStates beliefs)
    {
        
    }
}
