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
            base.StartEffect(abilityData, onFinished);

            foreach (GameObject target in abilityData.Targets)
            {
                if (target != null && target.TryGetComponent<Unit>(out Unit targetUnit))
                {
                    bool hit = CombatMath.RollHit(abilityData.GetUnit, targetUnit, out int hitChance, out float roll);

                    _visualsStrategy.CreateVisualEffect(abilityData, targetUnit); //do effect visuals

                    if (!hit)
                    {
                        // TODO: floating 'Miss' text here
                        Debug.Log($"[{abilityData.GetUnit}] Attack Missed, Targetted @ {targetUnit}");
                        continue;
                    }

                    if (_hasDuration)
                        targetUnit.StartCoroutine(DoEffectOverTime(targetUnit, _duration, _effectValue));
                    else
                        targetUnit.ChangeHealth(_effectValue, false);
                }
            }

            onFinished();
        }
    }
}