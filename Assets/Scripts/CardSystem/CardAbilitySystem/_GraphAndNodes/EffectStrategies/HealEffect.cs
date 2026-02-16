using System;
using UnityEngine;

namespace CardSystem
{
    // Concrete helpful effect class to heal a unit instantly or over a duration
    [CreateNodeMenu("Helpful Effects/Heal")]
    public class HealEffect : EffectStrategy, IUseEffectValue
    {
        public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0)
        {
            base.StartEffect(abilityData, onFinished, effectValueChange);

            foreach (GameObject target in abilityData.Targets)
            {
                if (target != null && target.TryGetComponent<Unit>(out Unit unit))
                {
                    unit.ChangeHealth(effectValue, true);
                    unit.GetFloatingText.SpawnFloatingText($"+{effectValue}", TextPresetType.HealPreset);
                }
            }

            onFinished();
        }
    }
}