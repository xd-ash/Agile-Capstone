using System;
using UnityEngine;

namespace CardSystem
{
    public enum DamageTypes //not used for much currently
    {
        None,
        Slash,
        Pierce,
        Fire,
        Emotional
    }

    // Concrete harmful effect class to damage a unit instantly or over a duration
    [CreateNodeMenu("Harmful Effects/Damage")]
    public class DamageEffect : HarmfulEffect
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
                        unit.ChangeHealth(_effectValue, false);
                }
            }

            onFinished();
        }
    }
}