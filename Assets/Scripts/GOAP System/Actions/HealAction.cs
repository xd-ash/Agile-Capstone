using UnityEngine;
using static GOAPDeterminationMethods;

public class HealAction : GoapAction
{
    public HealAction(GoapAgent agent) : base(agent) { }

    public override bool PrePerform(ref WorldStates beliefs)
    {
        if (_agent.healCharges <= 0)
        {
            beliefs.RemoveState(GoapStates.CanHeal.ToString());
            return false;
        }

        return CheckCanDoAction(_agent.unit, _agent.healAbility.GetApCost);
    }
    public override void Perform()
    {
        _agent.healAbility.UseAility(_agent.unit);
        _agent.healCharges--;

        _agent.CompleteAction();
    }
    public override void PostPerform(ref WorldStates beliefs)
    {
        if(CheckIfHealthy(_agent.unit, ref beliefs))
            beliefs.ModifyState(GoapStates.HasHealed.ToString(), 1);
    }
}
