using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNode;

namespace CardSystem
{
    [CreateNodeMenu("Targeting/Self")]
    public class SelfTarget : TargetingStrategy
    {
        public bool affectSelf;

        public override void StartTargeting(AbilityData abilityData, Action onFinished)
        {
            abilityData.Targets = isAOE ? GetGameObjectsInRadius(abilityData.GetUnit) : TargetSelf(abilityData);
            onFinished();
        }

        private IEnumerable<GameObject>TargetSelf(AbilityData abilityData)
        {
            yield return abilityData.GetUnit.gameObject;
        }

        protected override IEnumerable<GameObject> GetGameObjectsInRadius(UnitStatsScript user)
        {
            Collider[] foundObjects = Physics.OverlapSphere(user.transform.position, radius, layerMask);

            foreach (Collider collider in foundObjects)
            {
                yield return collider.gameObject;
            }

            if (affectSelf)
                yield return user.gameObject; //also target user
        }
    }
}