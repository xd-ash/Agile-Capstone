using System;
using UnityEngine;

namespace CardSystem
{
    [CreateNodeMenu("Effect/Debug")]
    public class DebugEffect : EffectStrategy
    {
        public string message;

        public override void StartEffect(AbilityData ablilityData, Action onFinished)
        {
            Debug.Log(message);
            onFinished();
        }
    }
}
