using System;
using UnityEngine;

namespace CardSystem
{
    [CreateNodeMenu("Harmful Effects/Damage")]
    public class DamageEffect : EffectStrategy, IUseEffectValue
    {
        public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0)
        {
            base.StartEffect(abilityData, onFinished, effectValueChange);

            foreach (GameObject target in abilityData.Targets)
            {
                if (target != null && target.TryGetComponent(out Unit targetUnit))
                {
                    var def = graph as CardAbilityDefinition;
                    bool hit = CombatMath.RollHit(abilityData.GetUnit.transform.localPosition, targetUnit, def);

                    _visualsStrategy?.CreateVisualEffect(abilityData, targetUnit); //do effect visuals

                    if (!hit) continue;

                    targetUnit.ChangeHealth(_effectValue, false);
                    targetUnit.GetFloatingText.SpawnFloatingText($"-{_effectValue}", TextPresetType.DamagePreset);
                }
            }
            
            _onFinished?.Invoke();
        }
    }
}