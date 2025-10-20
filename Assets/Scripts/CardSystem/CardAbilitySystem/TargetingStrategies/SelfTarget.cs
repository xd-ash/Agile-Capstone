using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    // Concrete targeting strategy to cast ability on the unit with an option to affectSelf or not
    [CreateNodeMenu("Targeting/Self")]
    public class SelfTarget : TargetingStrategy
    {
        public bool affectSelf;

        public override void StartTargeting(AbilityData abilityData, Action onFinished)
        {
            abilityData.GetUnit.StartCoroutine(TargetingCoro(abilityData, onFinished));
        }
        public override IEnumerator TargetingCoro(AbilityData abilityData, Action onFinished)
        {
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

            abilityData.Targets = isAOE ? GetGameObjectsInRadius(abilityData.GetUnit) : TargetSelf(abilityData);
            onFinished();
        }

        private IEnumerable<GameObject>TargetSelf(AbilityData abilityData)
        {
            yield return abilityData.GetUnit.gameObject;
        }

        protected override IEnumerable<GameObject> GetGameObjectsInRadius(Unit user)
        {
            Collider[] foundObjects = Physics.OverlapSphere(user.transform.position, radius);

            foreach (Collider collider in foundObjects)
            {
                yield return collider.gameObject;
            }

            if (affectSelf)
                yield return user.gameObject; //also target user
        }

    }
}