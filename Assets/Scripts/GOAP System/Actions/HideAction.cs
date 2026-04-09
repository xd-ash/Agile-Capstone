using static GOAPDeterminationMethods;

public class HideAction : GoapAction
{
    public HideAction(string overrideName = "") : base(overrideName)
    {

    }
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
    public override float EvaluateCost(string tempGoal, Unit tempTarget)
    {
        if (_agent == null || tempTarget == null) return _cost;

        var agentHealthRatio = _agent.unit.GetHealth / (float)_agent.unit.GetMaxHealth;
        return _cost;
    }
}
