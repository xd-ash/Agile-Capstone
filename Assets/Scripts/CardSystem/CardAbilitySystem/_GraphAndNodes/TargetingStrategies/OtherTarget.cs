using System;
using System.Collections;
using System.Collections.Generic;
using static IsoMetricConversions;
using UnityEngine;

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
                    GoapAgent agent = abilityData.GetUnit.GetComponent<GoapAgent>();
                    if (agent.GetCurrentTarget != null)
                        abilityData.Targets = new List<GameObject>() { agent.GetCurrentTarget.gameObject };
                    _aoeStrat?.GrabTargetsInRange(ref abilityData, ByteMapController.Instance.GetPositionOfUnit(agent.GetCurrentTarget));
                    onFinished?.Invoke();
                    break;
            }
        }
        //goap agent grabbing nearby tile for use in throwing AOE bombs & stuff (allows for "targetting" units out of LOS but within AOE radius)
        public void GetNearbyTileInLOS(ref AbilityData abilityData, Vector2Int targetUnitPos, Vector2Int agentPos)
        {
            var map = ByteMapController.Instance.GetByteMap;
            
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

            GameObject empty = new("empty");
            empty.transform.parent = FindFirstObjectByType<MapCreator>().transform;
            empty.transform.localPosition = ConvertToIsometricFromGrid(tilePos);

            abilityData.AbilityTriggerPos = tilePos;
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