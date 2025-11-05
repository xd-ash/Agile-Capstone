using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CardSystem
{
    [CreateNodeMenu("Targeting/Targetless")]
    public class Targetless : TargetingStrategy
    {
        public override void StartTargeting(AbilityData abilityData, Action onFinished)
        {
            base.StartTargeting(abilityData, onFinished);
            //abilityData.Targets = TargetSelf(abilityData);
            onFinished();
        }
        public override IEnumerator TargetingCoro(AbilityData abilityData, Action onFinished)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<GameObject> GetGameObjectsInRadius(Unit unit)
        {
            throw new NotImplementedException();
        }
    }
}