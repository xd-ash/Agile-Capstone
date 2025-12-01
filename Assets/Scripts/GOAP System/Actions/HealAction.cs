using UnityEngine;
using static GOAPDeterminationMethods;

public class HealAction : GoapAction
{
    public override bool PrePerform(ref WorldStates beliefs)
    {
        if (agent.healCharges <= 0)
        {
            beliefs.RemoveState(GoapStates.CanHeal.ToString());
            return false;
        }

        return CheckCanDoAction(agent.unit, agent.healAbility.RootNode.GetApCost);
    }
    public override void Perform()
    {
        agent.healAbility.UseAility(agent.unit);
        agent.healCharges--;

        agent.CompleteAction();
    }
    public override void PostPerform(ref WorldStates beliefs)
    {
        if(CheckIfHealthy(agent.unit, ref beliefs))
            beliefs.ModifyState(GoapStates.HasHealed.ToString(), 1);
    }
}
