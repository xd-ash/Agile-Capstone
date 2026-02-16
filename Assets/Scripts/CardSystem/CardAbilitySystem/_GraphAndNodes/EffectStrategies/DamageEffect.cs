using System;
using UnityEngine;

namespace CardSystem
{
    // Concrete harmful effect class to damage a unit instantly or over a duration
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

                    targetUnit.ChangeHealth(effectValue, false);
                    targetUnit.GetFloatingText.SpawnFloatingText($"-{effectValue}", TextPresetType.DamagePreset);
                }
            }

            _onFinished?.Invoke();
        }
    }
}