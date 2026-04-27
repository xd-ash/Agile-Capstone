using System;
using UnityEngine;

namespace CardSystem
{
    [CreateNodeMenu("Harmful Effects/Damage")]
    public class DamageEffect : EffectStrategy, IUseEffectValue
    {
        public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0, bool playAnimation = true)
        {
            base.StartEffect(abilityData, onFinished, effectValueChange, playAnimation);

            var def = graph as CardAbilityDefinition;
            var abilityPos = abilityData.AbilityTriggerPos == -Vector2Int.one ?
                ByteMapController.Instance.GetPositionOfUnit(abilityData.GetUnit) : abilityData.AbilityTriggerPos;

            foreach (GameObject target in abilityData.Targets)
            {
                if (target == null) continue;
                if (!target.TryGetComponent(out Unit targetUnit)) continue;

                if (targetUnit.IsDead) continue;

                bool hit = CombatMath.RollHit(abilityPos, targetUnit, def);

                _visualsStrategy?.CreateVisualEffect(abilityData, target);

                if (!hit) continue;

                var adjustedEffectVal = GetRarityAdjustedEffectValue(abilityData.GetCardRarity);
                
                targetUnit.ChangeHealth(adjustedEffectVal, false);
                targetUnit.GetFloatingText?.SpawnFloatingText($"-{adjustedEffectVal}", TextPresetType.DamagePreset);

                if (playAnimation && !targetUnit.IsDead)
                    targetUnit.PlayFlinchAnim(abilityPos);
            }

            _onFinished?.Invoke();
        }
    }
}