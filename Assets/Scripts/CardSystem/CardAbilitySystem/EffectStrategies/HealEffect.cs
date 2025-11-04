using System;
using UnityEngine;

namespace CardSystem
{
    // Concrete helpful effect class to heal a unit instantly or over a duration
    [CreateNodeMenu("Helpful Effects/Heal")]
    public class HealEffect : HelpfulEffect
    {
        public override void StartEffect(AbilityData abilityData, Action onFinished)
        {
            foreach (GameObject target in abilityData.Targets)
            {
                if (target != null && target.TryGetComponent<Unit>(out Unit unit))
                {
                    if (_hasDuration)
                        unit.StartCoroutine(DoEffectOverTime(unit, _duration, _effectValue));
                    else
                        unit.ChangeHealth(_effectValue, true);
                }
            }

            onFinished();
        }
    }
}