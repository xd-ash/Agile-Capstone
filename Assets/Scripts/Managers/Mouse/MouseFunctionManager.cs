using AStarPathfinding;
<<<<<<< Updated upstream
using System;
=======
>>>>>>> Stashed changes
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static AbilityEvents;

public class MouseFunctionManager : MonoBehaviour
{
    public static MouseFunctionManager instance;
<<<<<<< Updated upstream
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        _tilemap = FindAnyObjectByType<Tilemap>();
    }

    private Tilemap _tilemap;
    private Vector3Int _tilePos;
    private Vector3 _mousePos;
    private TileBase _currTile;
    private List<PathMarker> _currHighlightedPath;
    private Vector3Int _lastTilePos = new Vector3Int(-1, -1, -1);

    [SerializeField] private Color _mouseTileColor,
                                   _reachablePathColor,
                                   _unreachablePathColor;

    void Update()
    {
        TrackMouse();

        //if no tile is moused over, clear the current path (if any)
        //and set _lastTilePos to value outside the range of the play map
        //(_currtile will never be this value, unless the tilemap is changed).
        if (_currTile == null)
        {
            if (_currHighlightedPath != null)
            {
                //probably dont need this check as methods do null checking where needed
                if (IsTargeting)
                    ClearTileColor(_lastTilePos);
                else
                    ClearTileColor(_currHighlightedPath);
            }
            _lastTilePos = new Vector3Int(-1, -1, -1);
            return;
        }

        if (_lastTilePos != _tilePos)
        {
            //Clear any highlighted tiles once a new tile is selected
            ClearTileColor(_lastTilePos);
            if (_currHighlightedPath != null)
                ClearTileColor(_currHighlightedPath);
        }

        //Do relevant tasks for on mouseover & click
        if (IsTargeting)
            DoTargetingStuff();
        else
            FindMovementPath();
    }
    private void FindMovementPath()
    {
        if (MapCreator.instance.GetByteMap[_tilePos.x, _tilePos.y] != 0) return; // return if mouseover tile pos is not walkable

        // find path to mouseover position and set tile color
        var tempPath = FindPathAStar.instance.OnTileHover((Vector2Int)_tilePos);
        if (tempPath != null)
            SetTileColor(tempPath);

        _currHighlightedPath = tempPath;

        _lastTilePos = _tilePos;

        // Swap me to new input system at some point
        if (Input.GetMouseButtonDown(0))
            FindPathAStar.instance.OnTileClick();
    }
    private void DoTargetingStuff()
    {
        SetTileColor(_tilePos);
        _lastTilePos = _tilePos;

        Debug.LogWarning("DoTargetingStuff not implemented in mouseManager");
    }

    //Tracks mouse position and sets tile position and currtile
    private void TrackMouse()
    {
        Vector3 trueMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _mousePos = new Vector3(trueMousePos.x, trueMousePos.y, 0f);

        _tilePos = _tilemap.WorldToCell(_mousePos);
        _currTile = _tilemap.GetTile(_tilePos);
    }

    //Methods to set and clear tile colors, with overloads for affecting only
    //the mouseover tile or the entire current path
    private void SetTileColor(Vector3Int tilePos)
    {
        _tilemap.SetColor(tilePos, _mouseTileColor);
    }
    private void SetTileColor(List<PathMarker> path)
    {
        if (path == null || path.Count == 0) return;

        foreach (PathMarker pm in path)
            if (pm != null)
                _tilemap.SetColor(new Vector3Int(pm.location.x, pm.location.y), pm.isReachable ? _reachablePathColor : _unreachablePathColor);
    }
    private void ClearTileColor(Vector3Int tilePos)
    {
        _tilemap.SetColor(tilePos, Color.white);
    }
    private void ClearTileColor(List<PathMarker> path)
    {
        if (path == null || path.Count == 0) return;

        foreach (PathMarker pm in path)
            if (pm != null)
                ClearTileColor(new Vector3Int(pm.location.x, pm.location.y));
=======

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
>>>>>>> Stashed changes
    }
}
