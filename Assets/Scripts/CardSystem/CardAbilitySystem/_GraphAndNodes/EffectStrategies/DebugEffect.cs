using System;
using UnityEngine;

namespace CardSystem
{
    // Concrete misc effect class to send a debug log message instantly or multiple time over duration
    [CreateNodeMenu("Misc Effects/Debug Message")]
    public class DebugEffect : MiscEffect
    {
        [TextArea]
        [SerializeField] private string message;

        public override void StartEffect(AbilityData abilityData, Action onFinished)
        {
            base.StartEffect(abilityData, onFinished);

            foreach (GameObject target in abilityData.Targets)
            {
                if (target != null && target.TryGetComponent<Unit>(out Unit unit))
                {
                    Debug.Log(message);
                    if (_hasDuration)
                        DoEffectOverTime(unit, _duration);
                }
            }

            onFinished();
        }
    }
}
