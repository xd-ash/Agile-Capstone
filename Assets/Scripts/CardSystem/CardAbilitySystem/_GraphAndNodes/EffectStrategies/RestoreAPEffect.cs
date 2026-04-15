using System;
using UnityEngine;
using XNode;

namespace CardSystem
{
    [CreateNodeMenu("Misc Effects/Restore AP")]
    public class RestoreAPEffect : EffectStrategy, IUseEffectValue
    {
        public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0, bool playAnimation = true)
        {
            base.StartEffect(abilityData, onFinished, effectValueChange, playAnimation);

            var adjustedEffectVal = GetRarityAdjustedEffectValue(abilityData.GetCardRarity);

            foreach (GameObject target in abilityData.Targets)
                if (target != null && target.TryGetComponent(out Unit targetUnit))
                    targetUnit.RestoreAP(adjustedEffectVal + (graph as CardAbilityDefinition).GetApCost);

            _onFinished?.Invoke();
        }
    }
}