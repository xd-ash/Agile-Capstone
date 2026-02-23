using static GOAPDeterminationMethods;

public class AttackAction : GoapAction
{
    public override bool PrePerform(ref WorldStates beliefs)
    {
        bool canDoAction = CheckCanDoAction(_agent.unit, _agent.damageAbility.GetApCost);
        if (!canDoAction)
        {
            beliefs.ModifyState(GoapStates.HasAttacked.ToString(), 1);
            beliefs.ModifyState(GoapStates.OutOfAP.ToString(), 1);
        }

        return canDoAction;
    }
    public override void Perform()
    {
        _agent.damageAbility.UseAility(_agent.unit);

        _agent.CompleteAction();
    }

    public override void PostPerform(ref WorldStates beliefs)
    {
        if (!CheckCanDoAction(_agent.unit, _agent.damageAbility.GetApCost))
        {
            beliefs.ModifyState(GoapStates.HasAttacked.ToString(), 1);
            beliefs.ModifyState(GoapStates.OutOfAP.ToString(), 1);
        }
    }
}
