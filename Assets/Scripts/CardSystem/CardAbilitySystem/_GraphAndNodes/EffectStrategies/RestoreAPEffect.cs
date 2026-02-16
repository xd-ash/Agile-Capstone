using System;
using UnityEngine;
using XNode;

namespace CardSystem
{
    public class RestoreAPEffect : EffectStrategy, IUseEffectValue
    {
        public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0)
        {
            base.StartEffect(abilityData, onFinished, effectValueChange);

            foreach (GameObject target in abilityData.Targets)
                if (target != null && target.TryGetComponent(out Unit targetUnit))
                    targetUnit.RestoreAP(effectValue + (graph as CardAbilityDefinition).GetApCost);

            _onFinished?.Invoke();
        }
    }
}