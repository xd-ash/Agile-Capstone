using UnityEngine;
using static GOAPDeterminationMethods;

public class AttackAction : GoapAction
{
    public override bool PrePerform(ref WorldStates beliefs)
    {
        //Debug.Log($"agent: {agent.name}");
        return CheckForAP(agent.unit, ref beliefs, agent.damageAbility.RootNode.GetApCost);
    }
    public override void Perform()
    {
        agent.damageAbility.UseAility(agent.unit);

        agent.CompleteAction();
    }

    public override void PostPerform(ref WorldStates beliefs)
    {
        beliefs.ModifyState(GoapStates.HasAttacked.ToString(), 1);
    }
}
