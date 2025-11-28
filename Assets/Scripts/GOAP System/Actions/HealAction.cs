using UnityEngine;
using static GOAPDeterminationMethods;

public class HealAction : GoapAction
{
    public override bool PrePerform(ref WorldStates beliefs)
    {
        return CheckCanDoAction(agent.unit, agent.healAbility.RootNode.GetApCost);
    }
    public override void Perform()
    {
        agent.healAbility.UseAility(agent.unit);

        agent.CompleteAction();
    }
    public override void PostPerform(ref WorldStates beliefs)
    {
        beliefs.ModifyState(GoapStates.HasHealed.ToString(), 1);

        CheckIfHealthy(agent.unit, ref beliefs);
    }
}
