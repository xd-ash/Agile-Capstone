using UnityEngine;
using static GOAPDeterminationMethods;

public class AttackAction : GoapAction
{
    public override bool PrePerform()
    {
        return CheckCanDoAction(agent.unit, agent.damageAbility.RootNode.GetApCost);
    }
    public override void Perform()
    {
        agent.damageAbility.UseAility(agent.unit);

        agent.CompleteAction();
    }

    public override void PostPerform(ref WorldStates beliefs)
    {
        beliefs.ModifyState(GoapStates.HasAttacked.ToString(), 1);
        CheckForAP(agent.unit, ref beliefs, agent.damageAbility.RootNode.GetApCost);
    }
}
