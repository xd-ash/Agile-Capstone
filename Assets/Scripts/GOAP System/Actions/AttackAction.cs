using UnityEngine;
using static GOAPDeterminationMethods;

public class AttackAction : GoapAction
{
    public AttackAction(string overrideName = "") : base(overrideName)
    {

    }
    public AttackAction(GoapAction refAction) : base(refAction)
    {

    }

    public override bool PrePerform(ref WorldStates beliefs)
    {
        return beliefs.HasState(GoapStates.CanAttack.ToString());
    }
    public override void Perform()
    {
        _agent.GetHarmfulAbility.UseAbility(_agent.unit);
        _agent.OnUseAbility(_agent.GetHarmfulAbility);
        _agent.CompleteAction();
    }

    public override void PostPerform(ref WorldStates beliefs)
    {
        //if agent can no longer attack, then modify state
        if (!CheckCanDoAction(_agent.unit, _agent.GetHarmfulAbility.GetApCost))
            beliefs.RemoveState(GoapStates.CanAttack.ToString());

        beliefs.ModifyState(GoapStates.HasAttacked.ToString(), 1);
        _agent.attacksPerformedThisTurn++;
    }

    public override float EvaluateCost(string tempGoal, Unit tempTarget)
    {
        if (_agent == null || tempTarget == null) return _cost;

        var attackAdjust = tempGoal == GoapGoals.KillPlayer.ToString() ? _agent.attacksPerformedThisTurn : 0; // count attacks performed only on kill player goal to allow for lower cost of attacks in stayalive goal
        var targetHealthRatio = tempTarget.GetHealth / (float)tempTarget.GetMaxHealth;
        var attackCostRatio = _agent.GetHarmfulAbility == null ? int.MaxValue : _agent.GetHarmfulAbility.GetApCost / (float)_agent.unit.GetMaxAP;
        return _cost * (targetHealthRatio + attackCostRatio + attackAdjust) * _costMultiplier;
    }
}
