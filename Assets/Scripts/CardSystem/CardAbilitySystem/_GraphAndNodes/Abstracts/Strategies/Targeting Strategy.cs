using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using static IsoMetricConversions;
using static TileHighlighter;

namespace CardSystem
{
    // Base abstract targeting strategy
    public abstract class TargetingStrategy : AbilityNodeBase
    {
        [Input(connectionType = ConnectionType.Override)] public short abilityRoot;
        [Output(connectionType = ConnectionType.Override)] public bool aoeStrat;

        protected OnAOETarget _aoeStrat;
        protected HashSet<Vector2Int> _tilesInRange;

        public virtual void StartTargeting(AbilityData abilityData, ref Action onFinished)
        {
            if (abilityData.GetUnit.GetTeam != Team.Enemy)
            {
                AbilityEvents.TargetingStarted();
                AudioManager.Instance?.PlayCardSelectSfx();
            }
            foreach (NodePort port in Outputs)
            {
                if (port.Connection == null || port.Connection.node == null || port.Connection.node is not OnAOETarget)
                    continue;
                _aoeStrat = port.Connection.node as OnAOETarget;
                _aoeStrat?.InitNode();
            }

            if (abilityData.GetUnit.GetTeam == Team.Enemy) return;

            int range = (graph as CardAbilityDefinition).GetRange;
            Vector2Int unitPos = ConvertToGridFromIsometric(abilityData.GetUnit.transform.localPosition);
            _tilesInRange = ComputeCellsInRange(unitPos, range);
            ApplyHighlights(_tilesInRange, abilityData.GetUnit.GetGuid, Color.softRed * new Color(1,1,1,0.65f), 1); // set up general unit ability range tiles

            AbilityEvents.OnAbilityTargetingStopped += () => ClearHighlights(abilityData.GetGUID);
            onFinished += () =>
            {
                //ClearHighlights(abilityData.GetGUID);
                AbilityEvents.OnAbilityTargetingStopped -= () => ClearHighlights(abilityData.GetGUID);
            };
        }

        public abstract IEnumerator TargetingCoro(AbilityData abilityData, Action onFinished);

        public virtual HashSet<Vector2Int> ComputeCellsInRange(Vector2Int tilePos, int range)
        {
            var result = new HashSet<Vector2Int>();

            byte[,] map = ByteMapController.Instance.GetByteMap;
            int width = map.GetLength(0);
            int height = map.GetLength(1);

            int maxSteps = Mathf.Max(0, range);

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