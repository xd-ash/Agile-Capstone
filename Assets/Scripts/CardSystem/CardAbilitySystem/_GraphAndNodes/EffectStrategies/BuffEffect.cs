using System;
using UnityEngine;

namespace CardSystem
{
    // Concrete helpful effect class to buff a unit for a duration
    [CreateNodeMenu("Helpful Effects/Buff")]
    public class BuffEffect : EffectStrategy, IUseEffectValue
    {
        public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0)
        {
            base.StartEffect(abilityData, onFinished, effectValueChange);

            foreach (GameObject targetObj in abilityData.Targets)
            {
                if (targetObj != null && targetObj.TryGetComponent<Unit>(out Unit unit))
                {
                    var adjustedEffectVal = GetRarityAdjustedEffectValue(abilityData.GetCardRarity);

                    unit.AddShield(adjustedEffectVal);
                    unit.GetFloatingText.SpawnFloatingText($"+{adjustedEffectVal}", TextPresetType.ShieldPreset);
                }
            }

            _onFinished?.Invoke();
        }
    }
}