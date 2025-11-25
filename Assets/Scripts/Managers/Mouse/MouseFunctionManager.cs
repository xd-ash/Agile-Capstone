using AStarPathfinding;
using CardSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static AbilityEvents;

public class MouseFunctionManager : MonoBehaviour
{
    public static MouseFunctionManager instance;

    [SerializeField] private Tilemap _tilemap;

    [SerializeField] private Color _mouseTileColor = Color.yellow;

    [SerializeField] private APHoverIndicator _apHoverIndicator;

    [Header("Path line")]
    [SerializeField] private LineRenderer _line;
    [SerializeField] private float _lineZOffset = 0.01f;

    [SerializeField] private Vector3Int _tilePos;
    [SerializeField] private TileBase _currTile;
    private Vector3Int _lastTilePos = new Vector3Int(-1, -1, -1);

    //Target Select stuff
    public Action<bool> OnClickTarget;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (_tilemap == null)
            _tilemap = FindAnyObjectByType<Tilemap>();

        _line.useWorldSpace = true;
        _line.positionCount = 0;
    }

    private void Update()
    {
        // Quick fix for right clicking to cancel activated attack card/ability
        if (Input.GetMouseButtonDown(1))
        {
            if (IsTargeting && !PauseMenu.isPaused)
            {
                if (CardSystem.CardManager.instance.selectedCard != null &&
                    CardSystem.CardManager.instance.selectedCard.CardTransform.TryGetComponent<CardSelect>(out CardSelect card))
                {
                    AbilityEvents.TargetingStopped();
                    card.ReturnCardToHand();
                    CardManager.instance.OnCardAblityCancel?.Invoke();
                }
            }
        }

        TrackMouse();
        ManageCurrTileColor();

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

    private void ManageCurrTileColor()
    {
        SetTileColor(_tilePos);

        if (_currTile == null)
        {
            ClearTileColor(_lastTilePos);
            _lastTilePos = new Vector3Int(-1, -1, -1);
            return;
        }

        if (_lastTilePos != _tilePos)
        {
            //Clear any highlighted tiles once a new tile is selected
            ClearTileColor(_lastTilePos);
            _lastTilePos = _tilePos;
        }
    }
    private void TrackMouse()
    {
        Vector3 worldMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldMouse.z = 0f;

        _tilePos = _tilemap.WorldToCell(worldMouse);
        _currTile = _tilemap.GetTile(_tilePos);
    }
    private void DoTargetingStuff()
    {
        ClearLine();
    }

    private void DrawMovementPath()
    {
        if (PauseMenu.isPaused || TurnManager.instance == null || TurnManager.instance.currTurn != TurnManager.Turn.Player)
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

        List<PathMarker> path = TurnManager.GetCurrentUnit.GetComponent<FindPathAStar>().CalculatePath((Vector2Int)_tilePos);
        if (path == null || path.Count == 0)
        {
            ClearLine();
            return;
        }

        int steps = path.Count;
        int ap = Mathf.Max(0, unit.ap);
        int keep = ap < steps ? ap : steps;

        // Update the Fallout style AP indicator
        if (_apHoverIndicator != null)
        {
            Vector3 indicatorPos = GridToWorld((Vector2Int)_tilePos);
            indicatorPos.z += _lineZOffset;

            if (steps <= ap)
            {
                // In range – show only the AP number
                _apHoverIndicator.ShowCost(indicatorPos, steps);
                _line.gameObject.SetActive(true);

            }
            else
            {
                // Out of range – show AP number plus red X
                _apHoverIndicator.ShowOutOfRange(indicatorPos, steps);
                _line.gameObject.SetActive(false);
            }
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

        if (Input.GetMouseButtonDown(0))
            TurnManager.GetCurrentUnit.GetComponent<FindPathAStar>().OnStartUnitMove();
    }

    private Vector3 GridToWorld(Vector2Int cell)
    {
        // same pipeline as the board / units
        Vector3 localIso = IsoMetricConversions.ConvertToIsometricFromGrid(cell, 0f);
        return MapCreator.instance.transform.TransformPoint(localIso);
    }

    private void ClearLine()
    {
        _line.positionCount = 0;
        _apHoverIndicator.Hide();
    }

    private void SetTileColor(Vector3Int tilePos)
    {
        _tilemap.SetColor(tilePos, _mouseTileColor);
    }
    private void ClearTileColor(Vector3Int tilePos)
    {
        _tilemap.SetColor(tilePos, Color.white);
    }
}
