using UnityEngine;
using static GOAPDeterminationMethods;
using static AStarPathfinding.FindPathAStar;

public class HealAction : GoapAction
{
    public HealAction(string overrideName = "") : base(overrideName)
    {

    }
    public HealAction(GoapAction refAction) : base(refAction)
    {

    }
    public override bool PrePerform(ref WorldStates beliefs)
    {
        if (!_agent.CheckCanUseHeal || !CheckCanDoAction(_agent.GetUnit, _agent.GetHelpfulAbility.GetApCost))
        {
            beliefs.RemoveState(GoapStates.CanHeal.ToString());
            return false;
        }

        return true;
    }
    public override void Perform()
    {
        _agent.GetHelpfulAbility?.UseAbility(_agent.GetUnit);
        _agent.OnUseAbility(_agent.GetHelpfulAbility);

        _agent.CompleteAction();
    }
    public override void PostPerform(ref WorldStates beliefs)
    {
        if (!CheckIfHealthy(_agent.GetCurrentTarget, ref beliefs)) return;
    }

    public override float EvaluateCost(string tempGoal, Unit tempTarget)
    {
        if (_agent == null || tempTarget == null) return _cost;

        float tarHealthRatio = tempTarget.GetHealth / (float)tempTarget.GetMaxHealth;
        if (tarHealthRatio == 1)
            tarHealthRatio = float.MaxValue;
        float healCostRatio = _agent.GetHelpfulAbility == null ? float.MaxValue : _agent.GetHelpfulAbility.GetApCost / (float)_agent.GetUnit.GetMaxAP;
        return _cost * (tarHealthRatio + healCostRatio) * _costMultiplier;
    }
}
