using AStarPathfinding;
using UnityEngine;
using static IsoMetricConversions;

public class HideAction : GoapAction
{
    private FindPathAStar _aStar;

    public override bool PrePerform(ref WorldStates beliefs)
    {
        var reachableTiles = MovementRangeCalculator.ComputeReachableCells(_agent.unit);
        _aStar = _agent.GetComponent<FindPathAStar>();
        var target = _agent.GetCurrentTarget;
        if (target == null) return false;
        var targetTile = ConvertToGridFromIsometric(target.transform.position);

        Vector2Int bestTileOutOfLOS = -Vector2Int.one; // maybe change this?
        foreach (var tile in reachableTiles)
        {
            //var pathToTarget = _aStar.CalculatePath()
        }
        return true;
    }
    public override void Perform()
    {
        throw new System.NotImplementedException();
    }
    public override void PostPerform(ref WorldStates beliefs)
    {
        throw new System.NotImplementedException();
    }
}
