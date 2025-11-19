using UnityEngine;

public static class GOAPDeterminationMethods
{
    public static int FindAPAfterAction(Unit unit, int actionAPCost)
    {
        int result = unit.ap - actionAPCost;
        return result <= 0 ? 0 : result;
    }
}
