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
            switch (abilityData.GetUnit.GetTeam)
            {
                case Team.Friendly:
                    base.StartTargeting(abilityData, onFinished);
                    abilityData.GetUnit.StartTargetingCoroutine(TargetingCoro(abilityData, onFinished, _targetTilesNotUnits));
                    break;
                case Team.Enemy:
                    GoapAgent agent = abilityData.GetUnit.GetComponent<GoapAgent>();
                    if (agent.curtarget != null)
                        abilityData.Targets = new List<GameObject>() { agent.curtarget.gameObject };
                    onFinished();
                    break;
            }
        }
        public IEnumerator TargetingCoro(AbilityData abilityData, Action onFinished, bool isTileTargeted)
        {
            Unit caster = abilityData.GetUnit;
            Unit hoveredUnit = null;
            var def = graph as CardAbilityDefinition;

            while (true)
            {
                if (!isTileTargeted)
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
                            int hitChance = CombatMath.GetHitChance(caster, hoveredUnit, def);
                            hoveredUnit.ShowHitChance(hitChance);
                        }
                    }
                }

                if (Input.GetMouseButtonDown(0))
                {
                    abilityData.Targets = isTileTargeted ? TileOnMouse() : TargetOnMouse(caster);

                    //if (isAOE)
                        //abilityData.Targets.Concat<GameObject>(GetGameObjectsInRadius(caster));

                    if (abilityData.GetTargetCount > 0)
                        break;
                }

                yield return null;
            }

            if (hoveredUnit != null)
                hoveredUnit.HideHitChance();

            onFinished();
        }

        private IEnumerable<GameObject> TileOnMouse()
        {
            Vector2Int tilePos = (Vector2Int)MouseFunctionManager.Instance.GetCurrTilePosition;
            GameObject empty = new();
            empty.transform.parent = FindFirstObjectByType<MapCreator>().transform;
            empty.transform.localPosition = ConvertToIsometricFromGrid(tilePos);
            yield return empty;
        }

        private Unit GetUnitUnderMouse()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);

            if (hit.collider == null)
                return null;

            return hit.collider.GetComponent<Unit>();
        }

        /*public override IEnumerator TargetingCoro(AbilityData abilityData, Action onFinished)
        {
            do
            {
                yield return new WaitForEndOfFrame();
                yield return new WaitUntil(() => Input.GetMouseButtonDown(0));//find better option?
                
                abilityData.Targets = TargetOnMouse(abilityData.GetUnit);
                
                if (isAOE)
                    abilityData.Targets.Concat<GameObject>(GetGameObjectsInRadius(abilityData.GetUnit));
            }while (abilityData.GetTargetCount == 0);
            
            onFinished();
        }*/

        private IEnumerable<GameObject> TargetOnMouse(Unit unit)
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), 
                Vector2.zero, Mathf.Infinity);

            if (hit.collider != null && hit.collider.GetComponent<Unit>())
                yield return hit.collider.gameObject;
            else
                yield break;
        }

        protected override IEnumerable<GameObject> GetGameObjectsInRadius(Unit unit)
        {
            throw new NotImplementedException();

            /*Collider[] foundObjects = Physics.OverlapSphere(unit.transform.position, radius);

            foreach (Collider collider in foundObjects)
                yield return collider.gameObject;*/
        }

        /*
        protected IEnumerable<GameObject> XXX(Unit unit, IEnumerable<GameObject> x)
        {
            Collider[] foundObjects = Physics.OverlapSphere(unit.transform.position, radius);

            foreach (GameObject item in x)
            {
                yield return item;
            }

            foreach (Collider collider in foundObjects)
                yield return collider.gameObject;
        }*/
    }
}