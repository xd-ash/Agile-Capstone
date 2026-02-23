using AStarPathfinding;
using static IsoMetricConversions;
using static CombatMath;
using static GOAPDeterminationMethods;

public class MoveIntoLOSAction : GoapAction
{
    private FindPathAStar aStar;

    public override bool PrePerform(ref WorldStates beliefs)
    {
        if (beliefs.GetStates.ContainsKey(GoapStates.HasLOS.ToString())) return false;

        aStar = _agent.GetComponent<FindPathAStar>();
        Unit unit = _agent.unit;
        int dmgAbilRange = _agent.damageAbility.GetRange;

        var tarPos = ConvertToGridFromIsometric(_agent.GetCurrentTarget.transform.localPosition);
        var tempPath = aStar.CalculatePath(tarPos);
        int distanceToTar = tempPath.Count;

        for (int i = tempPath.Count - 1; i >= 0; i--)
        {
            var tempPos = tempPath[i].location.ToVector();
            if (HasLineOfSight(tempPos, tarPos))
            {
                aStar.CalculatePath(tempPos);
                return true;
            }
        }

        aStar.CalculatePath(tarPos); //default to walking to target if los cannot be reached?
        return false;
    }
    public override void Perform()
    {
        aStar.OnStartUnitMove(() =>
        {
            _agent.CompleteAction();
        });
    }
    public override void PostPerform(ref WorldStates beliefs)
    {
        //check range
        beliefs.ModifyState(GoapStates.HasLOS.ToString(), 1);
        beliefs.RemoveState(GoapStates.NoLOS.ToString());

        CheckIfInRange(_agent, _agent.damageAbility.GetRange, ref beliefs);
    }
}
