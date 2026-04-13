using UnityEngine;
using static GOAPDeterminationMethods;

public class AttackAction : GoapAction
{
    public override bool PrePerform(ref WorldStates beliefs)
    {
        bool canDoAction = CheckCanDoAction(_agent.unit, _agent.GetDamageAbility.GetApCost);
        if (!canDoAction)
        {
            beliefs.RemoveState(GoapStates.CanAttack.ToString());
            //beliefs.ModifyState(GoapStates.OutOfAP.ToString(), 1);
        }

        return canDoAction;
    }
    public AttackAction(string overrideName = "") : base(overrideName) 
    {

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

        beliefs.ModifyState(GoapStates.HasAttacked.ToString(), 1);
        _agent.attacksPerformedThisTurn++;
    }

    public override float EvaluateCost(string tempGoal, Unit tempTarget)
    { 
        if (_agent == null || tempTarget == null) return _cost;

        var attackAdjust = tempGoal == GoapGoals.KillPlayer.ToString() ? _agent.attacksPerformedThisTurn : 0; // count attacks performed only on kill player goal to allow for lower cost of attacks in stayalive goal
        var targetHealthRatio = tempTarget.GetHealth / (float)tempTarget.GetMaxHealth;
        var dmgAbility = _agent.GetAgentSO.GetDamageAbility;
        var attackCostRatio = dmgAbility.GetApCost / (float)_agent.unit.GetMaxAP;
        return _cost * (targetHealthRatio + attackCostRatio + attackAdjust) * _costMultiplier;
    }
}
