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

        public override void StartTargeting(AbilityData abilityData, Action onFinished)
        {
            base.StartTargeting(abilityData, onFinished);

            switch (abilityData.GetUnit.GetTeam)
            {
                case Team.Friendly:
                    abilityData.GetUnit.StartTargetingCoroutine(TargetingCoro(abilityData, onFinished));
                    break;
                case Team.Enemy:
                    GoapAgent agent = abilityData.GetUnit.GetComponent<GoapAgent>();
                    if (agent.curtarget != null)
                        abilityData.Targets = new List<GameObject>() { agent.curtarget.gameObject };
                    _aoeStrat?.GrabTargetsInRange(ref abilityData);
                    onFinished?.Invoke();
                    break;
            }
        }
        public override IEnumerator TargetingCoro(AbilityData abilityData, Action onFinished)
        {
            Unit caster = abilityData.GetUnit;
            Unit hoveredUnit = null;
            var def = graph as CardAbilityDefinition;

            while (true)
            {
                _aoeStrat?.GrabTargetsInRange(ref abilityData);

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
                        if (hoveredUnit != null && caster != null)
                        {
                            int hitChance = CombatMath.GetHitChance(caster.transform.localPosition, hoveredUnit, def);
                            hoveredUnit.ShowHitChance(hitChance);
                        }
                    }
                }

                if (Input.GetMouseButtonDown(0))
                {
                    List<GameObject> tempTargets = abilityData.Targets == null ? new List<GameObject>() : new List<GameObject>(abilityData.Targets);
                    GameObject temp = _targetTilesNotUnits ? TileOnMouse(abilityData) : TargetOnMouse(caster);
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

        private GameObject TileOnMouse(AbilityData abilitData)
        {
            var bmc = ByteMapController.Instance;
            Vector2Int tilePos = (Vector2Int)MouseFunctionManager.Instance.GetCurrTilePosition;
            if (tilePos.x < 0 || tilePos.x >= bmc?.GetByteMap.GetLength(0) ||
                tilePos.y < 0 || tilePos.y >= bmc?.GetByteMap.GetLength(1) ||
                bmc?.GetByteAtPosition(new Vector2Int(tilePos.x, tilePos.y)) != 0)
                return null;

            GameObject empty = new("empty");
            empty.transform.parent = FindFirstObjectByType<MapCreator>().transform;
            empty.transform.localPosition = ConvertToIsometricFromGrid(tilePos);

            abilitData.AbilityTriggerPos = tilePos;

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

        private GameObject TargetOnMouse(Unit unit)
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), 
                Vector2.zero, Mathf.Infinity);
            if (hit.collider != null && hit.collider.GetComponent<Unit>())
                return hit.collider.gameObject;
            return null;
        }
    }
}