using static GOAPDeterminationMethods;

public class AttackAction : GoapAction
{
    public override bool PrePerform(ref WorldStates beliefs)
    {
        bool canDoAction = CheckCanDoAction(_agent.unit, _agent.GetDamageAbility.GetApCost);
        if (!canDoAction)
        {
            beliefs.RemoveState(GoapStates.CanAttack.ToString());
            beliefs.ModifyState(GoapStates.OutOfAP.ToString(), 1);
        }

        return canDoAction;
    }
    public override void Perform()
    {
        _agent.GetDamageAbility.UseAility(_agent.unit);

        _agent.CompleteAction();
    }

    public override void PostPerform(ref WorldStates beliefs)
    {
        //if agent can no longer attack, then modify states
        if (!CheckCanDoAction(_agent.unit, _agent.GetDamageAbility.GetApCost))
        {
            beliefs.ModifyState(GoapGoals.KillPlayer.ToString(), 1);
            beliefs.ModifyState(GoapStates.OutOfAP.ToString(), 1);
        }
    }

    public override float EvaluateCost(Unit tempTarget)
    {
        if (_agent == null || tempTarget == null) return _cost;

        return _cost;
    }
}
