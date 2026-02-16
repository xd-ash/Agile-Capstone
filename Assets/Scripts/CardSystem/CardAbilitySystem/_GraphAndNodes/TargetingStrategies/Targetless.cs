using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    [CreateNodeMenu("Targeting/Targetless")]
    public class Targetless : TargetingStrategy
    {
        public override void StartTargeting(AbilityData abilityData, Action onFinished)
        {
            base.StartTargeting(abilityData, onFinished);
            
            onFinished();
        }

        protected override IEnumerable<GameObject> GetGameObjectsInRadius(Unit unit)
        {
            throw new NotImplementedException();
        }
    }
}