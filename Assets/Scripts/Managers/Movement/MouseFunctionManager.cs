using AStarPathfinding;
using CardSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static AbilityEvents;
using static IsoMetricConversions;

public class MouseFunctionManager : MonoBehaviour
{
    public static MouseFunctionManager instance;

    private Tilemap _tilemap;
    private APHoverIndicator _apHoverIndicator;

    [Header("Tile")]
    [SerializeField] private Color _mouseTileColor = Color.yellow;
    private Transform _highlightObjectParent;
    private GameObject _highlightTile;
    private Vector3Int _tilePos;
    private TileBase _currTile;
    //private Vector3Int _lastTilePos = new Vector3Int(-1, -1, -1);

    [Header("Path Line")]
    private LineRenderer _line;
    [SerializeField] private float _lineZOffset = 0.01f;

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
        if (_apHoverIndicator == null)
            _apHoverIndicator = FindAnyObjectByType<APHoverIndicator>();
        if (_line == null)
            _line = GetComponentInChildren<LineRenderer>();

        _line.useWorldSpace = true;
        _line.positionCount = 0;
    }
    private void Start()
    {
        InitializeTileHighlight();
    }
    private void InitializeTileHighlight()
    {
        _highlightObjectParent = MapCreator.instance.transform.Find("HighlightObjParent");
        if (_highlightObjectParent == null)
        {
            _highlightObjectParent = new GameObject("HighlightObjParent").transform;
            _highlightObjectParent.transform.parent = MapCreator.instance.transform;
            _highlightObjectParent.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            _highlightObjectParent.localScale = Vector3.one;
        }
        _highlightTile = Instantiate(Resources.Load<GameObject>("HighlightTile"), _highlightObjectParent);
        _highlightTile.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        _highlightTile.transform.localScale = Vector3.one;
        Color tileColor = _highlightTile.GetComponentInChildren<SpriteRenderer>().color;
        if (tileColor != _mouseTileColor)
            _highlightTile.GetComponentInChildren<SpriteRenderer>().color = _mouseTileColor;
        _highlightTile.GetComponentInChildren<SpriteRenderer>().sortingOrder = 2;
        _highlightTile.SetActive(false);
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
        
        if (PauseMenu.isPaused)
            return;

        if (!CheckMouseTileMove()) return;

        _highlightTile.SetActive(true);
        _highlightTile.transform.localPosition = ConvertToIsometricFromGrid((Vector2Int)_tilePos);

        if (IsTargeting)
        {
            DoTargetingStuff();
            return;
        }

        DrawMovementPath();
    }

    // return true if mouse crosses tile border
    private bool CheckMouseTileMove()
    {
        Vector3 worldMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldMouse.z = 0f;

        _tilePos = _tilemap.WorldToCell(worldMouse);
        var tempTile = _tilemap.GetTile(_tilePos);

        if (tempTile == null)
        {
            _highlightTile.SetActive(false);
            _currTile = null;
            ClearLine();
            return false;
        }

        _currTile = tempTile;
        return true;
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

        FindPathAStar unitAStar = unit.GetComponent<FindPathAStar>();
        List<PathMarker> path = unitAStar.CalculatePath((Vector2Int)_tilePos);
        if (path == null || path.Count == 0)
        {
            ClearLine();
            return;
        }

        int steps = path.Count;
        int ap = Mathf.Max(0, unit.ap);
        int keep = ap < steps ? ap : steps;
        bool shouldMove = true;

        // Update the Fallout style AP indicator
        if (_apHoverIndicator != null)
        {
            Vector3 indicatorPos = GridToWorld((Vector2Int)_tilePos);
            indicatorPos.z += _lineZOffset;

            if (steps <= ap)
            {
                // In range � show only the AP number
                _apHoverIndicator.ShowCost(indicatorPos, steps);
                _line.gameObject.SetActive(true);
            }
            else
            {
                // Out of range � show AP number plus red X
                _apHoverIndicator.ShowOutOfRange(indicatorPos, steps);
                shouldMove = false;
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

        if (Input.GetMouseButtonDown(0) && shouldMove)
            unitAStar.OnStartUnitMove();
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

    /*private void SetTileColor(Vector3Int tilePos)
    {
        _tilemap.SetColor(tilePos, _mouseTileColor);
    }
    private void ClearTileColor(Vector3Int tilePos)
    {
        _tilemap.SetColor(tilePos, Color.white);
    }*/
}
