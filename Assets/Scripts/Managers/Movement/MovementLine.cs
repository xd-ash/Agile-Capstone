using AStarPathfinding;
using System.Collections.Generic;
using UnityEngine;
using static IsoMetricConversions;

public class MovementLine : MonoBehaviour
{
    [Header("Path Line")]
    private LineRenderer _line;
    [SerializeField] private float _lineZOffset = 0.01f;

    public static MovementLine instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        } 

        if (_line == null)
            _line = GetComponent<LineRenderer>();

        _line.useWorldSpace = true;
        _line.positionCount = 0;
    }
    public bool DrawMovementPath(out bool shouldMove)
    {
        shouldMove = false;

        var tilePos = MouseFunctionManager.instance.GetCurrTilePosition;

        if (PauseMenu.isPaused || TurnManager.instance == null || TurnManager.instance.CurrTurn != TurnManager.Turn.Player ||
            MapCreator.instance.GetByteMap[tilePos.x, tilePos.y] != 0 || TurnManager.GetCurrentUnit == null)
            return false;

        Unit unit = TurnManager.GetCurrentUnit;
        var unitAStar = unit.GetComponent<FindPathAStar>();
        List<PathMarker> path = unitAStar.CalculatePath((Vector2Int)tilePos);
        if (path == null || path.Count == 0)
            return false;

        int steps = path.Count;
        int ap = Mathf.Max(0, unit.GetAP);
        int keep = ap < steps ? ap : steps;

        // Update the Fallout style AP indicator
        Vector3 indicatorPos = GridToWorld((Vector2Int)tilePos);
        indicatorPos.z += _lineZOffset;

        if (steps <= ap)
        {
            // In range � show only the AP number
            APHoverIndicator.instance?.ShowCost(indicatorPos, steps);
            _line.gameObject.SetActive(true);
            shouldMove = true;
        }
        else
        {
            // Out of range � show AP number plus red X
            APHoverIndicator.instance?.ShowOutOfRange(indicatorPos, steps);
            _line.gameObject.SetActive(false);
        }

        List<Vector3> points = new List<Vector3>();

        // start at unit position
        Vector3 startPos = unit.transform.position;
        startPos.z += _lineZOffset;
        points.Add(startPos);

        int startIndex = steps - 1;
        int endIndex = steps - keep;

        for (int i = startIndex; i >= endIndex; i--)
        {
            Vector2Int grid = new Vector2Int(path[i].location.x, path[i].location.y);
            Vector3 world = GridToWorld(grid);
            world.z += _lineZOffset;
            points.Add(world);
        }

        _line.positionCount = points.Count;
        _line.SetPositions(points.ToArray());

        return true;
    }
    public void ClearLine()
    {
        _line.positionCount = 0;
        APHoverIndicator.instance?.Hide();
    }
    public static Vector3 GridToWorld(Vector2Int cell)
    {
        Vector3 localIso = ConvertToIsometricFromGrid(cell, 0f);
        return MapCreator.instance.transform.TransformPoint(localIso);
    }
}
