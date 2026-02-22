using UnityEngine;
using static GOAPDeterminationMethods;

public class AttackAction : GoapAction
{
    public AttackAction(GoapAgent agent) : base(agent) { }

    public override bool PrePerform(ref WorldStates beliefs)
    {
        bool canDoAction = CheckCanDoAction(agent.unit, agent.damageAbility.GetApCost);
        if (!canDoAction)
        {
            beliefs.ModifyState(GoapStates.HasAttacked.ToString(), 1);
            beliefs.ModifyState(GoapStates.OutOfAP.ToString(), 1);
        }

        return canDoAction;
    }
    public override void Perform()
    {
        agent.damageAbility.UseAility(agent.unit);

        agent.CompleteAction();
    }

    public override void PostPerform(ref WorldStates beliefs)
    {
        if (!CheckCanDoAction(agent.unit, agent.damageAbility.GetApCost))
        {
            beliefs.ModifyState(GoapStates.HasAttacked.ToString(), 1);
            beliefs.ModifyState(GoapStates.OutOfAP.ToString(), 1);
        }
    }
}
