using System;
using UnityEngine;

namespace CardSystem
{
    // Concrete misc effect class to send a debug log message instantly or multiple time over duration
    [CreateNodeMenu("Misc Effects/Debug Message")]
    public class DebugEffect : EffectStrategy, IUseEffectValue
    {
        [TextArea]
        [SerializeField] private string message;

        public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0)
        {
            base.StartEffect(abilityData, onFinished, effectValueChange);

            Debug.Log(message);

            _onFinished?.Invoke(); 
        }
    }
}
