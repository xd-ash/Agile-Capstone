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
        if (!_agent.CheckCanUseHeal || !CheckCanDoAction(_agent.unit, _agent.GetHelpfulAbility.GetApCost))
        {
            beliefs.RemoveState(GoapStates.CanHeal.ToString());
            return false;
        }

        return true;
    }
    public override void Perform()
    {
        _agent.GetHelpfulAbility?.UseAbility(_agent.unit);
        _agent.OnUseAbility(_agent.GetHelpfulAbility);

        _agent.CompleteAction();
    }
    public override void PostPerform(ref WorldStates beliefs)
    {
        if (!CheckIfHealthy(_agent.GetCurrentTarget, ref beliefs)) return;

        /*if (_agent.GetCurrentGoal.key == GoapGoals.StayAlive.ToString())
            beliefs.ModifyState(GoapGoals.StayAlive.ToString(), 1);
        else if (_agent.GetCurrentGoal.key == GoapGoals.KeepAlliesAlive.ToString())
            beliefs.ModifyState(GoapGoals.KeepAlliesAlive.ToString(), 1);*/
    }

    public override float EvaluateCost(string tempGoal, Unit tempTarget)
    {
        if (_agent == null || tempTarget == null) return _cost;

        if (tempGoal == GoapGoals.StayAlive.ToString())
            tempTarget = _agent.unit;

        float tarHealthRatio = tempTarget.GetHealth / (float)tempTarget.GetMaxHealth;
        float healCostRatio = _agent.GetHelpfulAbility == null ? float.MaxValue : _agent.GetHelpfulAbility.GetApCost / (float)_agent.unit.GetMaxAP;
        return _cost * (tarHealthRatio + healCostRatio) * _costMultiplier;
    }
}
