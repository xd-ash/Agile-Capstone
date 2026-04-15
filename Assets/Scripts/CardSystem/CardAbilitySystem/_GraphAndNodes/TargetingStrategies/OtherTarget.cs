using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AStarPathfinding.FindPathAStar;
using static CombatMath;
using static IsoMetricConversions;
using static UnityEngine.GraphicsBuffer;

namespace CardSystem
{
    // Concrete targeting strategy for targeting the unit (if any) the player clicks with mouseover
    [CreateNodeMenu("Targeting/Other")]
    public class OtherTarget : TargetingStrategy
    {
        [SerializeField] private bool _targetTilesNotUnits = false;

        public override void StartTargeting(AbilityData abilityData, ref Action onFinished)
        {
            base.StartTargeting(abilityData, ref onFinished);

            switch (abilityData.GetUnit.GetTeam)
            {
                case Team.Friendly:
                    abilityData.GetUnit.StartTargetingCoroutine(TargetingCoro(abilityData, onFinished));
                    break;
                case Team.Enemy:
                    if (!abilityData.GetUnit.TryGetComponent(out GoapAgent agent)) 
                        return;

                    GameObject target = null;
                    Vector2Int closestValidTile = -Vector2Int.one;
                    if (agent.GetCurrentTarget != null)
                    {
                        target = agent.GetCurrentTarget.gameObject;

                        // if ability targets tiles, grab closest tile around target unit location
                        if (_targetTilesNotUnits)
                        {
                            closestValidTile = GetNearbyTileInLOS(agent.GetCurrentTarget, agent.unit);
                            target = SpawnTargettingEmpty(closestValidTile);
                            abilityData.AbilityTriggerPos = closestValidTile;
                        }
                    }

                    //band aid fix for medic heal targetting
                    if (agent.name.Contains("Medic") && agent.GetCurrentGoal.key == GoapGoals.StayAlive.ToString())
                        target = agent.gameObject;
                    //

                    abilityData.Targets = new List<GameObject>() { target };
                    if (target != null)
                        _aoeStrat?.GrabTargetsInRange(ref abilityData, closestValidTile == -Vector2Int.one ? 
                            ConvertToGridFromIsometric(target.transform.localPosition) : closestValidTile, false);
                    onFinished?.Invoke();
                    break;
            }
        }
        //goap agent grabbing nearby tile for use in throwing AOE bombs & stuff (allows for "targetting" units out of LOS but within AOE radius)
        public Vector2Int GetNearbyTileInLOS(Unit targetUnit, Unit agentUnit)
        {
            var targetUnitPos = ByteMapController.Instance.GetPositionOfUnit(targetUnit);
            var agentPos = ByteMapController.Instance.GetPositionOfUnit(agentUnit);

            var map = ByteMapController.Instance.GetByteMap;
            var tilesInRangeOfTarget = ComputeCellsInAbilityRange(targetUnitPos, _aoeStrat.GetAOERange);

            Vector2Int closestValidTile = -Vector2Int.one;
            int closestDistance = int.MaxValue;
            foreach (var tile in tilesInRangeOfTarget)
            {
                if (!HasLineOfSight(agentPos, tile) || !HasLineOfSight(targetUnitPos, tile)) continue;
                var distToTile = CalculatePath(agentPos, tile).Count;
                if (distToTile > (graph as CardAbilityDefinition).GetRange) continue;
                if (distToTile >= closestDistance) continue;
                closestValidTile = tile;
                closestDistance = distToTile;
            }

            if (closestValidTile == -Vector2Int.one)
            {
                Debug.LogWarning($"0 valid tiles for GetNearbyTileInLOS. (TargetPos:{targetUnitPos}, AgentPos:{agentPos})");
                return agentPos;
            }

            return closestValidTile;
        }

        public override IEnumerator TargetingCoro(AbilityData abilityData, Action onFinished)
        {
            Unit hoveredUnit = null;
            var def = graph as CardAbilityDefinition;

            while (true)
            {
                _aoeStrat?.GrabTargetsInRange(ref abilityData, (Vector2Int)MouseFunctionManager.Instance?.GetCurrTilePosition);

                if (!_targetTilesNotUnits)
                {
                    //Hover detection
                    Unit newHover = GetUnitUnderMouse();

                    if (newHover != hoveredUnit)
                    {
                        //Clear old hover
                        if (hoveredUnit != null)
                            hoveredUnit.HideHitChance();

                        hoveredUnit = newHover;

                        //Show new hover hit chance
                        if (hoveredUnit != null && abilityData.GetUnit != null)
                        {
                            int hitChance = CombatMath.GetHitChance(ByteMapController.Instance.GetPositionOfUnit(abilityData.GetUnit), hoveredUnit, def);
                            hoveredUnit.ShowHitChance(hitChance);
                        }
                    }
                }

                if (Input.GetMouseButtonDown(0))
                {
                    List<GameObject> tempTargets = abilityData.Targets == null ? new List<GameObject>() : new List<GameObject>(abilityData.Targets);
                    GameObject temp = _targetTilesNotUnits ? TileOnMouse(ref abilityData) : TargetOnMouse();

                    if (temp == null)
                    {
                        yield return null;
                        continue;
                    }

                    if (!tempTargets.Contains(temp))
                        tempTargets.Add(temp);
                    abilityData.Targets = tempTargets;

                    if (abilityData.GetTargetCount > 0)
                        break;
                }

                yield return null;
            }


            if (hoveredUnit != null)
                hoveredUnit.HideHitChance();

            onFinished?.Invoke();
        }

        private GameObject TileOnMouse(ref AbilityData abilityData)
        {
            var bmc = ByteMapController.Instance;
            Vector2Int tilePos = (Vector2Int)MouseFunctionManager.Instance.GetCurrTilePosition;
            if (tilePos.x < 0 || tilePos.x >= bmc?.GetByteMap.GetLength(0) ||
                tilePos.y < 0 || tilePos.y >= bmc?.GetByteMap.GetLength(1) ||
                bmc?.GetByteAtPosition(new Vector2Int(tilePos.x, tilePos.y)) == 2 ||
                bmc?.GetByteAtPosition(new Vector2Int(tilePos.x, tilePos.y)) == 5) //full or half cover
                return null;

            //check in range
            if (!_tilesInRange.Contains(tilePos))
                return null;

            var empty = SpawnTargettingEmpty(tilePos);

            abilityData.AbilityTriggerPos = tilePos;
            return empty;
        }
        private GameObject SpawnTargettingEmpty(Vector2Int tilePos)
        {
            GameObject empty = new("empty");
            empty.transform.parent = FindFirstObjectByType<MapCreator>().transform;
            empty.transform.localPosition = ConvertToIsometricFromGrid(tilePos);
            empty.AddComponent<TargetingEmptyIdentifier>();
            return empty;
        }
        private Unit GetUnitUnderMouse()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);

            if (hit.collider == null)
                return null;

            return hit.collider.GetComponent<Unit>();
        }

        private GameObject TargetOnMouse()
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition),
                Vector2.zero, Mathf.Infinity);
            if (hit.collider != null && hit.collider.GetComponent<Unit>())
                return hit.collider.gameObject;
            return null;
        }
    }
}