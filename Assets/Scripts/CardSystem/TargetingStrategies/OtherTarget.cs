using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace CardSystem
{
    [CreateNodeMenu("Targeting/Other")]
    public class OtherTarget : TargetingStrategy
    {
        private bool drawLineGizmo;

        public override void StartTargeting(AbilityData abilityData, Action onFinished)
        {
            abilityData.GetUnit.StartCoroutine(TargetSelectionCoro(abilityData, onFinished));
            abilityData.GetUnit.StartCoroutine(LineDrawCoro(abilityData.GetUnit));
        }

        private IEnumerator TargetSelectionCoro(AbilityData abilityData, Action onFinished)
        {
            drawLineGizmo = true;
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
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
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity);

            if (hit.collider != null)
                yield return hit.collider.gameObject;
        }

        protected override IEnumerable<GameObject> GetGameObjectsInRadius(Unit unit)
        {
            Collider[] foundObjects = Physics.OverlapSphere(unit.transform.position, radius, layerMask);

            foreach (Collider collider in foundObjects)
            {
                yield return collider.gameObject;
            }
        }
    }
}