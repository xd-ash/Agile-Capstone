using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CardSystem
{
    // Concrete targeting strategy for targeting the unit (if any) the player clicks with mouseover
    [CreateNodeMenu("Targeting/Other")]
    public class OtherTarget : TargetingStrategy
    {
        public override void StartTargeting(AbilityData abilityData, Action onFinished)
        {
            base.StartTargeting(abilityData, onFinished);

            abilityData.GetUnit.StartCoroutine(TargetingCoro(abilityData, onFinished));
        }
        public override IEnumerator TargetingCoro(AbilityData abilityData, Action onFinished)
        {
            Unit caster = abilityData.GetUnit;
            Unit hoveredUnit = null;

            while (true)
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
                        int hitChance = CombatMath.GetHitChance(caster, hoveredUnit);
                        hoveredUnit.ShowHitChance(hitChance);
                    }
                }

                if (Input.GetMouseButtonDown(0))
                {
                    abilityData.Targets = TargetOnMouse(caster);

                    if (isAOE)
                        abilityData.Targets.Concat<GameObject>(GetGameObjectsInRadius(caster));

                    if (abilityData.GetTargetCount > 0)
                        break;
                }

                yield return null;
            }

            if (hoveredUnit != null)
                hoveredUnit.HideHitChance();

            onFinished();
        }

        private Unit GetUnitUnderMouse()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);

            if (hit.collider == null)
                return null;

            return hit.collider.GetComponent<Unit>();
        }


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
            Collider[] foundObjects = Physics.OverlapSphere(unit.transform.position, radius);

            foreach (Collider collider in foundObjects)
                yield return collider.gameObject;
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