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
<<<<<<< Updated upstream:Assets/Scripts/CardSystem/CardAbilitySystem/_GraphAndNodes/EffectStrategies/DebugEffect.cs
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

=======
            Debug.Log(message);
>>>>>>> Stashed changes:Assets/Scripts/CardSystem/CardAbilitySystem/EffectStrategies/DebugEffect.cs
            onFinished();
        }
    }
}
