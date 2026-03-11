using AStarPathfinding;
using Unity.VisualScripting;
using UnityEngine;
using static IsoMetricConversions;
using static CombatMath;
using static GOAPDeterminationMethods;

public class MoveIntoLOSAction : GoapAction
{
    private UnitMovementController _unitMover;

    public override bool PrePerform(ref WorldStates beliefs)
    {
        if (beliefs.states.ContainsKey(GoapStates.HasLOS.ToString())) return false;

        _unitMover = agent.GetComponent<UnitMovementController>();
        Unit unit = agent.unit;
        int dmgAbilRange = agent.damageAbility.GetRange;

        var tarPos = ConvertToGridFromIsometric(agent.curtarget.transform.localPosition);
        var tempPath = _unitMover.CalculatePath(tarPos);
        int distanceToTar = tempPath.Count;

        for (int i = tempPath.Count - 1; i >= 0; i--)
        {
            var tempPos = tempPath[i].location.ToVector();
            if (HasLineOfSight(tempPos, tarPos))
            {
                _unitMover.CalculatePath(tempPos);
                return true;
            }
        }

        _unitMover.CalculatePath(tarPos); //default to walking to target if los cannot be reached?
        return false;
    }
    public override void Perform()
    {
        _unitMover.OnStartUnitMove(() =>
        {
            agent.CompleteAction();
        });
    }
    public override void PostPerform(ref WorldStates beliefs)
    {
        //check range
        beliefs.ModifyState(GoapStates.HasLOS.ToString(), 1);
        beliefs.RemoveState(GoapStates.NoLOS.ToString());

        CheckIfInRange(agent, agent.damageAbility.GetRange, ref beliefs);
    }
}
