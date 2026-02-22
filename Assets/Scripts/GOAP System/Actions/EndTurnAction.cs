using UnityEngine;

public class EndTurnAction : GoapAction
{
    public EndTurnAction(GoapAgent agent) : base(agent) { }

    public override bool PrePerform(ref WorldStates beliefs)
    {
        return true;
    }
    public override void Perform()
    {
        agent.ClearPlanner();
        TurnManager.Instance.EndEnemyTurn();
        //Debug.Log($"End turn Perform");
        agent.CompleteAction();
    }
    public override void PostPerform(ref WorldStates beliefs)
    {
        // do nothing?
    }
}
