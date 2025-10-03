using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    /*
    [CreateNodeMenu("Targeting/Radius")]
    public class RadiusTargeting : TargetingStrategy
    {
        public float radius;
        public LayerMask layerMask;

        public override void StartTargeting(AbilityData abilityData, Action onFinished)
        {
            abilityData.Targets = GetGameObjectsInRadius(abilityData.GetUser);
            onFinished();
        }

        private IEnumerable<GameObject> GetGameObjectsInRadius(AbilityController user)
        {
            Collider[] foundObjects = Physics.OverlapSphere(user.transform.position, radius, layerMask);

            foreach (Collider collider in foundObjects)
            {
                yield return collider.gameObject;
            }
        }
    }*/
}