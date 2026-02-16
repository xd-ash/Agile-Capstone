using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    // Concrete targeting strategy to cast ability on the unit with an option to affectSelf or not
    [CreateNodeMenu("Targeting/Self")]
    public class SelfTarget : TargetingStrategy
    {
        public override void StartTargeting(AbilityData abilityData, Action onFinished)
        {
            base.StartTargeting(abilityData, onFinished);

            //abilityData.Targets = isAOE ? GetGameObjectsInRadius(abilityData.GetUnit) : TargetSelf(abilityData);
            abilityData.Targets = TargetSelf(abilityData);
            onFinished();
        }

        private IEnumerable<GameObject>TargetSelf(AbilityData abilityData)
        {
            yield return abilityData.GetUnit.gameObject;
        }

        protected override IEnumerable<GameObject> GetGameObjectsInRadius(Unit user)
        {
            throw new NotImplementedException();
        }
    }
}