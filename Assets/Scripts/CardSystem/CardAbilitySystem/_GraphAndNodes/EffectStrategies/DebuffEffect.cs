using System;
using UnityEngine;

namespace CardSystem
{
    // Concrete harmful effect class to debuff a unit for a duration
    [CreateNodeMenu("Harmful Effects/Debuff")]
    public class DebuffEffect : EffectStrategy, IUseEffectValue
    {
        public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0)
        {
            base.StartEffect(abilityData, onFinished, effectValueChange);

            foreach (GameObject target in abilityData.Targets)
            {
                if (target != null && target.TryGetComponent<Unit>(out Unit unit))
                {
                    //do effect things
                    /*if (_hasDuration)
                        unit.StartCoroutine(DoEffectOverTime(unit, _duration));*/
                }
            }

            _onFinished?.Invoke();
        }
    }
}