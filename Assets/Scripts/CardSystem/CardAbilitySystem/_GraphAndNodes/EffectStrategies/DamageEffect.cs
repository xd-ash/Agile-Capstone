using System;
using UnityEngine;
using AStarPathfinding;

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

            Unit attacker = abilityData.GetUnit;

            foreach (GameObject target in abilityData.Targets)
            {
                if (target == null)
                    continue;

                if (!target.TryGetComponent<Unit>(out Unit targetUnit))
                    continue;

                // If somehow we don't have an attacker, fall back to old behavior
                if (attacker == null)
                {
                    ApplyDamageOverTime(targetUnit);
                    continue;
                }

                bool hit = CombatMath.RollHit(attacker, targetUnit, out int hitChance, out float roll);

                Debug.Log($"[Combat] {attacker.name} attacks {targetUnit.name} | " + $"chance: {hitChance}% | roll: {roll:0.0} => {(hit ? "HIT" : "MISS")}");

                if (!hit)
                {
                    // TODO: floating 'Miss' text here
                    continue;
                }

                ApplyDamageOverTime(targetUnit);
            }

            onFinished();
        }

        private void ApplyDamageOverTime(Unit unit)
        {
            if (_hasDuration)
            {
                unit.StartCoroutine(DoEffectOverTime(unit, _duration, _effectValue));
            }
            else
            {
                unit.ChangeHealth(_effectValue, false);
            }
        }
    }
}