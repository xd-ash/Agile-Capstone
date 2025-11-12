using AStarPathfinding;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static AbilityEvents;

public class MouseFunctionManager : MonoBehaviour
{
    public static MouseFunctionManager instance;

    [SerializeField] private Tilemap _tilemap;

    [SerializeField] private Color _mouseTileColor = Color.yellow;

    [Header("Path line")]
    [SerializeField] private LineRenderer _line;
    [SerializeField] private float _lineZOffset = 0.01f;

    private Vector3Int _tilePos;
    private TileBase _currTile;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        if (_tilemap == null)
        {
            _tilemap = FindAnyObjectByType<Tilemap>();
        }
        _line.useWorldSpace = true;
        _line.positionCount = 0;
    }

    private void Update()
    {
        TrackMouse();
        if (_currTile == null)
        {
            ClearLine();
            return;
        }
        if (IsTargeting)
        {
            DoTargetingStuff();
            return;
        }
        DrawMovementPath();
    }

    private void TrackMouse()
    {
        Vector3 worldMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldMouse.z = 0f;

        _tilePos = _tilemap.WorldToCell(worldMouse);
        _currTile = _tilemap.GetTile(_tilePos);
    }

    private void DrawMovementPath()
    {
        if (PauseMenu.isPaused ||
            TurnManager.instance == null ||
            TurnManager.instance.currTurn != TurnManager.Turn.Player)
        {
            ClearLine();
            return;
        }

        // unwalkable -> no line
        if (MapCreator.instance.GetByteMap[_tilePos.x, _tilePos.y] != 0)
        {
            ClearLine();
            return;
        }

        Unit unit = TurnManager.GetCurrentUnit;
        if (unit == null)
        {
            ClearLine();
            return;
        }

        List<PathMarker> path = FindPathAStar.instance.OnTileHover((Vector2Int)_tilePos);
        if (path == null || path.Count == 0)
        {
            ClearLine();
            return;
        }

        int steps = path.Count;
        int ap = Mathf.Max(0, unit.ap);
        int keep = ap < steps ? ap : steps;

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

        if (Input.GetMouseButtonDown(0))
        {
            FindPathAStar.instance.OnTileClick();
        }
    }

    private Vector3 GridToWorld(Vector2Int cell)
    {
        // same pipeline as the board / units
        Vector3 localIso = IsoMetricConversions.ConvertToIsometricFromGrid(cell, 0f);
        return MapCreator.instance.transform.TransformPoint(localIso);
    }

    private void ClearLine()
    {
        if (_line != null)
            _line.positionCount = 0;
    }

    private void DoTargetingStuff()
    {
        _tilemap.SetColor(_tilePos, _mouseTileColor);
        ClearLine();
    }
}
