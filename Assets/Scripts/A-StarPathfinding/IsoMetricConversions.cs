using AStarPathfinding;
using UnityEngine;

//Small static class for holding conversions to and from Isometric (local pos) to the
//location on the "Grid" used to represet the play area
public static class IsoMetricConversions
{
    // Distance (X and Y) from one tile center another in one of the cardinal directions (no diagonals)
    // in the isometric grid. (This must be updated if isometric "tilt" is adjusted)
    private static Vector2 step = new Vector2(0.5f, 0.25f);

    // z pos adjusted with y value to allow for easy layering of sprites
    // (.01f holds no significance, just used to keep value small)
    private static float _spriteAdjustmentZDir = 0.01f;

    public static Vector3 ConvertToIsometricFromGrid(Vector2Int pos)
    {
        float x = (pos.x - pos.y) * step.x;
        float y = (pos.x + pos.y) * step.y;
        return new Vector3(x, y, y * _spriteAdjustmentZDir);
    }
    public static Vector2Int ConvertToGridFromIsometric(Vector3 pos)
    {
        int x = (int)(((pos.y / step.y) + (pos.x / step.x)) * 0.5f);
        int y = (int)(((pos.y / step.y) - (pos.x / step.x)) * 0.5f);
        return new Vector2Int((int)(pos.x + 2 * pos.y), (int)(2 * pos.y - pos.x));
    }
}
