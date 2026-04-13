using UnityEngine;

public class EndTurnAction : GoapAction
{
    public EndTurnAction(string overrideName = "") : base(overrideName)
    {

    }
    public override bool PrePerform(ref WorldStates beliefs)
    {
        return true;
    }
    public override void Perform()
    {
        TurnManager.Instance.EndEnemyTurn();
        _agent.ClearPlanner();
        _agent.CompleteAction();
    }
    public override void PostPerform(ref WorldStates beliefs)
    {
        
    }

    public override float EvaluateCost(string tempGoal, Unit tempTarget)
    {
        return _cost;
    }
}
