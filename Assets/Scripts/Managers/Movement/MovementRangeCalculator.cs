using System.Collections.Generic;
using UnityEngine;
using static IsoMetricConversions;

public class MovementRangeCalculator : MonoBehaviour
{
    [SerializeField] private Color _reachableColor = new Color(0f, 0.3f, 1f, 0.3f);

    private Unit _currentUnit;
    private TurnManager.Turn _lastTurn = TurnManager.Turn.Player;

    public static MovementRangeCalculator Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        TrySetCurrentUnit(TurnManager.GetCurrentUnit);
        if (TurnManager.Instance != null)
            _lastTurn = TurnManager.Instance.CurrTurn;

        AbilityEvents.OnAbilityTargetingStarted += TileHighlighter.ClearHighlights;
        AbilityEvents.OnAbilityTargetingStopped += RebuildForCurrentUnit;
    }
    private void OnDestroy()
    {
        AbilityEvents.OnAbilityTargetingStarted -= TileHighlighter.ClearHighlights;
        AbilityEvents.OnAbilityTargetingStopped -= RebuildForCurrentUnit;
    }

    private void Update()
    {
        /* revisit this code after turn manager code is revisited */

        if (TurnManager.Instance.CurrTurn != _lastTurn)
        {
            _lastTurn = TurnManager.Instance.CurrTurn;
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
            TileHighlighter.ClearHighlights();
            UnsubscribeFromUnit();
            _currentUnit = null;
        }
    }

    private void TrySetCurrentUnit(Unit unit)
    {
        if (unit == null || unit.GetTeam != Team.Friendly)
        {
            TileHighlighter.ClearHighlights();
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
            TurnManager.Instance == null ||
            TurnManager.Instance.CurrTurn != TurnManager.Turn.Player ||
            AbilityEvents.IsTargeting ||
            PauseMenu.isPaused)
        {
            TileHighlighter.ClearHighlights();
            return;
        }
        var reachable = ComputeReachableCells(_currentUnit);
        TileHighlighter.ApplyHighlights(reachable, _reachableColor);
    }

    private HashSet<Vector2Int> ComputeReachableCells(Unit unit)
    {
        var result = new HashSet<Vector2Int>();
        
        byte[,] map = ByteMapController.Instance.GetByteMap;
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
}