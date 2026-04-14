using System;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using static IsoMetricConversions;
using static UnityEditor.PlayerSettings;

namespace CardSystem
{
    public class OnAOETarget : AbilityNodeBase
    {
        [Input(connectionType = ConnectionType.Override)] public bool input;

        [SerializeField] private Color _aoeHighlightColor = Color.darkRed;
        [SerializeField] private int _range;

        private Vector2Int _currTilePos;

        TargetingStrategy _targetingStrat;

        public void InitNode()
        {
            _currTilePos = new Vector2Int(-1, -1);
        }

        public void GrabTargetsInRange(ref AbilityData abilityData, Vector2Int targettingPos)
        {
            if (_targetingStrat == null)
                foreach (NodePort port in Inputs)
                {
                    if (port.Connection == null || port.Connection.node == null || port.Connection.node is not TargetingStrategy)
                        continue;
                    _targetingStrat = port.Connection.node as TargetingStrategy;
                }

            List<GameObject> tempTargets = new List<GameObject>();
            Vector2Int startingCell = _targetingStrat is SelfTarget ? ConvertToGridFromIsometric(abilityData.GetUnit.transform.localPosition) : targettingPos;
            if (startingCell == _currTilePos) return;
            _currTilePos = startingCell;

            ByteMapController bmc = ByteMapController.Instance;
            byte[,] map = bmc.GetByteMap;

            var cellsInRange = _targetingStrat.ComputeCellsInRange(startingCell, _range);
            if (startingCell.x >= 0 && startingCell.y >= 0 && startingCell.x < map.GetLength(0) && startingCell.y < map.GetLength(1))
                cellsInRange.Add(startingCell);

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

            TileHighlighter.ClearHighlights(abilityData.GetGUID);
            TileHighlighter.ApplyHighlights(cellsInRange, abilityData.GetGUID, _aoeHighlightColor, true);
        }
    }
}