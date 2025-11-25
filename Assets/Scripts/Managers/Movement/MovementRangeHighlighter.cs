using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using AStarPathfinding;
using static IsoMetricConversions;

public class MovementRangeHighlighter : MonoBehaviour
{
    public static MovementRangeHighlighter instance;

    [SerializeField] private Tilemap _highlightTilemap;
    [SerializeField] private Color _reachableColor = new Color(0f, 0.3f, 1f, 0.3f);

    private readonly List<Vector3Int> _lastHighlightedCells = new List<Vector3Int>();
    private Unit _currentUnit;
    private TurnManager.Turn _lastTurn = TurnManager.Turn.Player;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        TrySetCurrentUnit(TurnManager.GetCurrentUnit);
        if (TurnManager.instance != null)
        {
            _lastTurn = TurnManager.instance.currTurn;
        }

        AbilityEvents.OnAbilityTargetingStarted += ClearHighlights;
        AbilityEvents.OnAbilityTargetingStopped += RebuildForCurrentUnit;
    }
    private void OnDestroy()
    {
        AbilityEvents.OnAbilityTargetingStarted -= ClearHighlights;
        AbilityEvents.OnAbilityTargetingStopped -= RebuildForCurrentUnit;
    }
    private void Update()
    {
        if (TurnManager.instance.currTurn != _lastTurn)
        {
            _lastTurn = TurnManager.instance.currTurn;
            OnTurnChanged(_lastTurn);
        }

        if (_lastTurn == TurnManager.Turn.Player)
        {
            var unit = TurnManager.GetCurrentUnit;
            if (unit != _currentUnit)
                TrySetCurrentUnit(unit);
        }
    }

    private void OnTurnChanged(TurnManager.Turn newTurn)
    {
        if (newTurn == TurnManager.Turn.Player)
        {
            TrySetCurrentUnit(TurnManager.GetCurrentUnit);
        }
        else
        {
            ClearHighlights();
            UnsubscribeFromUnit();
            _currentUnit = null;
        }
    }

    private void TrySetCurrentUnit(Unit unit)
    {
        if (unit == null || unit.team != Team.Friendly)
        {
            ClearHighlights();
            UnsubscribeFromUnit();
            _currentUnit = null;
            return;
        }

        if (_currentUnit == unit)
        {
            RebuildForCurrentUnit();
            return;
        }

        UnsubscribeFromUnit();
        _currentUnit = unit;
        _currentUnit.OnApChanged += HandleUnitApChanged;
        RebuildForCurrentUnit();
    }

    private void UnsubscribeFromUnit()
    {
        if (_currentUnit != null)
            _currentUnit.OnApChanged -= HandleUnitApChanged;
    }

    private void HandleUnitApChanged(Unit unit)
    {
        RebuildForCurrentUnit();
    }

    public void RebuildForCurrentUnit()
    {
        if (_currentUnit == null ||
            TurnManager.instance == null ||
            TurnManager.instance.currTurn != TurnManager.Turn.Player ||
            AbilityEvents.IsTargeting ||
            PauseMenu.isPaused)
        {
            ClearHighlights();
            return;
        }
        var reachable = ComputeReachableCells(_currentUnit);
        ApplyHighlights(reachable);
    }

    private HashSet<Vector3Int> ComputeReachableCells(Unit unit)
    {
        var result = new HashSet<Vector3Int>();
        
        byte[,] map = MapCreator.instance.GetByteMap;
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        Vector2Int start = ConvertToGridFromIsometric(unit.transform.localPosition);
        int maxSteps = Mathf.Max(0, unit.ap);

        Queue<(Vector2Int pos, int dist)> queue = new Queue<(Vector2Int, int)>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue((start, 0));
        visited.Add(start);

        Vector2Int[] dirs =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        while (queue.Count > 0)
        {
            var (pos, dist) = queue.Dequeue();

            if (dist > 0)
            {
                result.Add(new Vector3Int(pos.x, pos.y, 0));
            }

            if (dist == maxSteps)
            {
                continue;
            }

            for (int i = 0; i < dirs.Length; i++)
            {
                Vector2Int next = pos + dirs[i];

                if (next.x < 0 || next.y < 0 || next.x >= width || next.y >= height)
                {
                    continue;
                }

                if (visited.Contains(next))
                {
                    continue;
                }

                byte cell = map[next.x, next.y];
                if (cell != 0)
                {
                    continue;
                }

                visited.Add(next);
                queue.Enqueue((next, dist + 1));
            }
        }
        return result;
    }

    private void ApplyHighlights(HashSet<Vector3Int> cells)
    {
        if (_highlightTilemap == null)
        {
            Debug.LogWarning("[MovementRangeHighlighter] No highlight tilemap assigned.");
            return;
        }
        
        ClearHighlights();

        foreach (var cell in cells)
        {
            _highlightTilemap.SetTileFlags(cell, TileFlags.None);
            _highlightTilemap.SetColor(cell, _reachableColor);
            _lastHighlightedCells.Add(cell);
        }
    }

    private void ClearHighlights()
    {
        for (int i = 0; i < _lastHighlightedCells.Count; i++)
        {
            _highlightTilemap.SetColor(_lastHighlightedCells[i], Color.white);
        }
        _lastHighlightedCells.Clear();
    }
}