using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.Content;
using UnityEngine;

namespace CardSystem
{
    // Concrete targeting strategy for targeting the unit (if any) the player clicks with mouseover
    [CreateNodeMenu("Targeting/Other")]
    public class OtherTarget : TargetingStrategy
    {
        private bool drawLineGizmo;

        public override void StartTargeting(AbilityData abilityData, Action onFinished)
        {
            //Debug.Log(abilityData.GetUnit.name);

            abilityData.GetUnit.StartCoroutine(TargetingCoro(abilityData, onFinished));
            abilityData.GetUnit.StartCoroutine(LineDrawCoro(abilityData.GetUnit));
        }
        public override IEnumerator TargetingCoro(AbilityData abilityData, Action onFinished)
        {
            drawLineGizmo = true;
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));//find better option?
            drawLineGizmo = false;

            abilityData.Targets = TargetOnMouse(abilityData.GetUnit);
            if (isAOE)
                abilityData.Targets.Concat<GameObject>(GetGameObjectsInRadius(abilityData.GetUnit));

            onFinished();
        }
        private IEnumerator LineDrawCoro(Unit unit)
        {
            while (drawLineGizmo)
            {
                Debug.DrawLine(unit.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition), Color.white);
                yield return null;
            }
        }

        private IEnumerable<GameObject> TargetOnMouse(Unit unit)
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), 
                Vector2.zero, Mathf.Infinity);

            if (hit.collider != null && hit.collider.GetComponent<Unit>())
                yield return hit.collider.gameObject;
            else
            {
                Debug.Log("No target hit");
                yield return null; 
            }
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