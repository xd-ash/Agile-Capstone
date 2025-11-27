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
            base.StartTargeting(abilityData, onFinished);

            abilityData.Targets = isAOE ? GetGameObjectsInRadius(abilityData.GetUnit) : TargetSelf(abilityData);
            onFinished();
        }
        public override IEnumerator TargetingCoro(AbilityData abilityData, Action onFinished)
        {
<<<<<<< Updated upstream:Assets/Scripts/CardSystem/CardAbilitySystem/_GraphAndNodes/TargetingStrategies/SelfTarget.cs
            throw new NotImplementedException();
=======
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

            abilityData.Targets = isAOE ? GetGameObjectsInRadius(abilityData.GetUnit) : TargetSelf(abilityData);
            onFinished();
>>>>>>> Stashed changes:Assets/Scripts/CardSystem/CardAbilitySystem/TargetingStrategies/SelfTarget.cs
        }

        private IEnumerable<GameObject>TargetSelf(AbilityData abilityData)
        {
            yield return abilityData.GetUnit.gameObject;
        }

        protected override IEnumerable<GameObject> GetGameObjectsInRadius(Unit user)
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