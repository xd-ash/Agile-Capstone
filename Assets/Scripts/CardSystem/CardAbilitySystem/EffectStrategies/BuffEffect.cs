using System;
using UnityEngine;

namespace CardSystem
{
    // Concrete helpful effect class to buff a unit for a duration
    [CreateNodeMenu("Helpful Effects/Buff")]
    public class BuffEffect : HelpfulEffect
    {
        public override void StartEffect(AbilityData abilityData, Action onFinished)
        {
            foreach (GameObject target in abilityData.Targets)
            {
                if (target != null && target.TryGetComponent<Unit>(out Unit unit))
                {
                    //do effect things
                    if (_hasDuration)
                        DoEffectOverTime(unit, _duration);
                }
            }

            onFinished();
        }
    }
}