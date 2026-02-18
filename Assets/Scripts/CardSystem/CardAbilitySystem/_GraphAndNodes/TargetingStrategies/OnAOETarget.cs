using System.Collections.Generic;
using UnityEngine;
using XNode;
using static IsoMetricConversions;

namespace CardSystem
{
    public class OnAOETarget : AbilityNodeBase
    {
        [Input(connectionType = ConnectionType.Override)] public bool input;

        [SerializeField] private Color _aoeHighlightColor;
        [SerializeField] private int _range;

        private Vector2Int _currTilePos;

        TargetingStrategy _targetingStrat;

        public void InitNode()
        {
            _currTilePos = new Vector2Int(-1, -1);
        }

        public void GrabTargetsInRange(ref AbilityData abilityData)
        {
            if (_targetingStrat == null)
                foreach (NodePort port in Inputs)
                {
                    if (port.Connection == null || port.Connection.node == null || port.Connection.node is not TargetingStrategy)
                        continue;
                    _targetingStrat = port.Connection.node as TargetingStrategy;
                }

            List<GameObject> tempTargets = abilityData.Targets == null ? new List<GameObject>() : new List<GameObject>(abilityData.Targets);
            Vector2Int startingCell = _targetingStrat is SelfTarget ? ConvertToGridFromIsometric(abilityData.GetUnit.transform.localPosition) :
                                                                     (Vector2Int)MouseFunctionManager.Instance?.GetCurrTilePosition;
            if (startingCell == _currTilePos) return;
            _currTilePos = startingCell;

            var cellsInRange = ComputeCellsInRange(startingCell);
            ByteMapController bmc = ByteMapController.Instance;
            byte[,] map = bmc.GetByteMap;

            foreach (var cell in cellsInRange)
            {
                if (map[cell.x, cell.y] == 1 || map[cell.x, cell.y] == 3) //player or enemy
                {
                    var unit = bmc.GetUnitAtPosition(new Vector2Int(cell.x, cell.y));
                    if (unit == null || tempTargets.Contains(unit.gameObject)) continue;
                    tempTargets.Add(unit.gameObject);
                }
            }
            abilityData.Targets = tempTargets;

            TileHighlighter.ClearHighlights();
            TileHighlighter.ApplyHighlights(cellsInRange, _aoeHighlightColor);
        }

        private HashSet<Vector2Int> ComputeCellsInRange(Vector2Int tilePos)
        {
            var result = new HashSet<Vector2Int>();

            byte[,] map = ByteMapController.Instance.GetByteMap;
            int width = map.GetLength(0);
            int height = map.GetLength(1);

            int maxSteps = Mathf.Max(0, _range);

            Queue<(Vector2Int pos, int dist)> queue = new Queue<(Vector2Int, int)>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

            queue.Enqueue((tilePos, 0));
            visited.Add(tilePos);

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

                    if (map[next.x, next.y] == 2)
                        continue;

                    visited.Add(next);
                    queue.Enqueue((next, dist + 1));
                }
            }
            return result;
        }
    }
}