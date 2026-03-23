using UnityEngine;
using static GOAPDeterminationMethods;

public class HealAction : GoapAction
{
    public override bool PrePerform(ref WorldStates beliefs)
    {
        if (_agent.healCharges <= 0 || !CheckCanDoAction(_agent.unit, _agent.GetHealAbility.GetApCost))
        {
            //beliefs.ModifyState(GoapStates.CantHeal.ToString(), 1);
            beliefs.RemoveState(GoapStates.CanHeal.ToString());
            return false;
        }

        return true;
    }
    public override void Perform()
    {
        _agent.GetHealAbility.UseAility(_agent.unit);
        _agent.healCharges--;

        _agent.CompleteAction();
    }
    public override void PostPerform(ref WorldStates beliefs)
    {
        CheckIfHealthy(_agent.unit, ref beliefs);

        string highestPrioGoalName = _agent.GetHighestGoalDesire().key;
        if (highestPrioGoalName != GoapGoals.StayAlive.ToString())
            beliefs.ModifyState(GoapGoals.StayAlive.ToString(), 1);
    }
}
