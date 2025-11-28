using UnityEngine;
using static GOAPDeterminationMethods;

public class AttackAction : GoapAction
{
    public override bool PrePerform(ref WorldStates beliefs)
    {
        bool canDoAction = CheckCanDoAction(agent.unit, agent.damageAbility.RootNode.GetApCost);
        if (!canDoAction)
            beliefs.ModifyState(GoapStates.HasAttacked.ToString(), 1);
    
        return canDoAction;
    }
    public override void Perform()
    {
        agent.damageAbility.UseAility(agent.unit);

        agent.CompleteAction();
    }

    public override void PostPerform(ref WorldStates beliefs)
    {
        if (!CheckCanDoAction(agent.unit, agent.damageAbility.RootNode.GetApCost))
            beliefs.ModifyState(GoapStates.HasAttacked.ToString(), 1);
    }
}
