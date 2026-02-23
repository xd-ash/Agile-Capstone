using UnityEngine;

public class EndTurnAction : GoapAction
{
    public override bool PrePerform(ref WorldStates beliefs)
    {
        return true;
    }
    public override void Perform()
    {
        _agent.ClearPlanner();
        TurnManager.Instance.EndEnemyTurn();
        //Debug.Log($"End turn Perform");
        _agent.CompleteAction();
    }
    public override void PostPerform(ref WorldStates beliefs)
    {
        // do nothing?
    }
}
