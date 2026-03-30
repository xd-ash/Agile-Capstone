using UnityEngine;
using static GOAPDeterminationMethods;
using static AStarPathfinding.FindPathAStar;

public class HealAction : GoapAction
{
    public override bool PrePerform(ref WorldStates beliefs)
    {
        if (_agent.healCharges == 0 || !CheckCanDoAction(_agent.unit, _agent.GetHealAbility.GetApCost))
        {
            beliefs.RemoveState(GoapStates.CanHeal.ToString());
            return false;
        }

        return true;
    }
    public override void Perform()
    {
        _agent.GetHealAbility.UseAility(_agent.unit);
        if (_agent.healCharges > 0)
            _agent.healCharges--;

        _agent.CompleteAction();
    }
    public override void PostPerform(ref WorldStates beliefs)
    {
        CheckIfHealthy(_agent.unit, ref beliefs);

        string highestPrioGoalName = _agent.GetHighestGoalDesire().key;
        if (highestPrioGoalName != GoapGoals.StayAlive.ToString())
            beliefs.ModifyState(GoapGoals.StayAlive.ToString(), 1);
        else if (highestPrioGoalName != GoapGoals.KeepAlliesAlive.ToString())
            beliefs.ModifyState(GoapGoals.KeepAlliesAlive.ToString(), 1);
    }

    public override float EvaluateCost(Unit tempTarget)
    {
        if (_agent == null || tempTarget == null) return _cost;

        float tarHealthRatio = tempTarget.GetHealth / (float)tempTarget.GetMaxHealth;
        float healCostRatio = _agent.GetHealAbility.GetApCost / (float)_agent.unit.GetMaxAP;
        //Debug.Log(_cost * (tarHealthRatio + healCostRatio) * _costMultiplier);
        return _cost * (tarHealthRatio + healCostRatio) * _costMultiplier;
    }
}
