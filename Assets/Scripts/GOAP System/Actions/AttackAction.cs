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
        Debug.Log($"Attack Perform");
        agent.damageAbility.UseAility(agent.unit);

        agent.CompleteAction();
    }

    public override void PostPerform(ref WorldStates beliefs)
    {
        Debug.Log("im swapping state for hasattacked");
        beliefs.ModifyState(GoapStates.HasAttacked.ToString(), 1);
        //CheckForAP(agent.unit, ref beliefs);
    }
}
