using UnityEngine;

public class EndTurnAction : GoapAction
{
    public override bool PrePerform()
    {
        return true;
    }
    public override void Perform()
    {
        TurnManager.instance.EndEnemyTurn();
        Debug.Log($"End turn Perform");
        agent.CompleteAction();
    }
    public override void PostPerform(ref WorldStates beliefs)
    {
        // do nothing?
    }
}
