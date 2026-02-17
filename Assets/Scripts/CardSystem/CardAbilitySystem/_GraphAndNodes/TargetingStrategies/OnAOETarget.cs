using System.Collections.Generic;
using UnityEngine;
using static IsoMetricConversions;

namespace CardSystem 
{
    public class OnAOETarget : AbilityNodeBase
    {
        [Input(connectionType = ConnectionType.Override)] public float input;

        [SerializeField] private int _range;

        /*public AbilityData GrabTargetsInRange(AbilityData abilityData)
        {
            List<GameObject> tempTargets = new List<GameObject>(abilityData.Targets);
            Vector2Int startingCell = (Vector2Int)MouseFunctionManager.Instance?.GetCurrTilePosition; // make this able to swap between self targetting and on mouse?
            var cellsInRange = ComputeCellsInRange(startingCell);

            byte[,] map = MapCreator.Instance.GetByteMap;

            foreach (var cell in cellsInRange)
            {
                if (map[cell.x,cell.y] == 1 || map[cell.x, cell.y] == 3) //player or enemy
                    tempTargets.Add()
            }
        }
        private HashSet<Vector2Int> ComputeCellsInRange(Vector2Int tilePos)
        {
            var result = new HashSet<Vector2Int>();

            byte[,] map = MapCreator.Instance.GetByteMap;
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
        }*/
    }
}