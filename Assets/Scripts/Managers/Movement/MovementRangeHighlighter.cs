using System.Collections.Generic;
using UnityEngine;
using AStarPathfinding;
using static IsoMetricConversions;
using static GameObjectPool;

public class MovementRangeHighlighter : MonoBehaviour
{
    [SerializeField] private Color _reachableColor = new Color(0f, 0.3f, 1f, 0.3f);
    private Transform _highlightObjectParent;
    private GameObject _highlightTilePrefab;

    private readonly List<GameObject> _lastHighlightedTiles = new List<GameObject>();
    private Unit _currentUnit;
    private TurnManager.Turn _lastTurn = TurnManager.Turn.Player;

    public static MovementRangeHighlighter instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        TrySetCurrentUnit(TurnManager.GetCurrentUnit);
        if (TurnManager.instance != null)
            _lastTurn = TurnManager.instance.CurrTurn;

        _highlightObjectParent = MapCreator.instance.transform.Find("HighlightObjParent");
        _highlightTilePrefab = Resources.Load<GameObject>("HighlightTile");

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
        /* revisit this code after turn manager code is revisited */

        if (TurnManager.instance.CurrTurn != _lastTurn)
        {
            _lastTurn = TurnManager.instance.CurrTurn;
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
            TrySetCurrentUnit(TurnManager.GetCurrentUnit);
        else
        {
            ClearHighlights();
            UnsubscribeFromUnit();
            _currentUnit = null;
        }
    }

    private void TrySetCurrentUnit(Unit unit)
    {
        if (unit == null || unit.GetTeam != Team.Friendly)
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
            TurnManager.instance.CurrTurn != TurnManager.Turn.Player ||
            AbilityEvents.IsTargeting ||
            PauseMenu.isPaused)
        {
            ClearHighlights();
            return;
        }
        var reachable = ComputeReachableCells(_currentUnit);
        ApplyHighlights(reachable);
    }

    private HashSet<Vector2Int> ComputeReachableCells(Unit unit)
    {
        var result = new HashSet<Vector2Int>();
        
        byte[,] map = MapCreator.instance.GetByteMap;
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        Vector2Int start = ConvertToGridFromIsometric(unit.transform.localPosition);
        int maxSteps = Mathf.Max(0, unit.GetAP);

        Queue<(Vector2Int pos, int dist)> queue = new Queue<(Vector2Int, int)>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue((start, 0));
        visited.Add(start);

        Vector2Int[] dirs = { Vector2Int.up,
                              Vector2Int.right,
                              Vector2Int.down,
                              Vector2Int.left };

        while (queue.Count > 0)
        {
            var (pos, dist) = queue.Dequeue();

            if (dist > 0)
                result.Add(new Vector2Int(pos.x, pos.y));

            if (dist == maxSteps) continue;

            for (int i = 0; i < dirs.Length; i++)
            {
                Vector2Int next = pos + dirs[i];

                if (next.x < 0 || next.y < 0 || next.x >= width || next.y >= height || visited.Contains(next))
                    continue;

                byte cell = map[next.x, next.y];

                if (cell == 0)
                {
                    visited.Add(next);
                    queue.Enqueue((next, dist + 1));
                }
            }
        }
        return result;
    }

    private void ApplyHighlights(HashSet<Vector2Int> cells)
    {
        ClearHighlights();

        foreach (var cell in cells)
        {
            Vector3 cellLocalPos = ConvertToIsometricFromGrid(cell);
            GameObject tile = Spawn(_highlightTilePrefab, cellLocalPos, Quaternion.identity, _highlightObjectParent);
            tile.GetComponentInChildren<SpriteRenderer>().color = _reachableColor;
            _lastHighlightedTiles.Add(tile);
        }
    }

    private void ClearHighlights()
    {
        for (int i = _lastHighlightedTiles.Count - 1; i >= 0; i--)
            Remove(_lastHighlightedTiles[i]);

        _lastHighlightedTiles.Clear();
    }
}