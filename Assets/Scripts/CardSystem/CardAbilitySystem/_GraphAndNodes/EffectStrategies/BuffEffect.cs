using System;
using System.Collections;
using UnityEngine;

namespace CardSystem
{
    // Concrete helpful effect class to buff a unit for a duration
    [CreateNodeMenu("Helpful Effects/Buff")]
    public class BuffEffect : EffectStrategy
    {
        // Uses _effectValue as shield amount when this effect represents a shield.
        // _hasDuration and _duration control optional timed expiry (seconds).

        public override void StartEffect(AbilityData abilityData, Action onFinished)
        {
            base.StartEffect(abilityData, onFinished);

            foreach (GameObject targetObj in abilityData.Targets)
            {
                if (targetObj != null && targetObj.TryGetComponent<Unit>(out Unit unit))
                {
                    // Add shield equal to _effectValue
                    /*if (_hasDuration)
                    {
                        // Add shield and schedule removal after _duration seconds
                        unit.AddShield(_effectValue, _duration);
                    }
                    else
                    {*/
                        // Permanent until consumed
                        unit.AddShield(_effectValue);
                    //}
                }
            }

            onFinished();
        }

        // If you prefer a per-second tick effect (legacy support), you can keep/override DoEffectOverTime.
        // For shield behavior, we use AddShield + optional duration coroutine above.
    }
}