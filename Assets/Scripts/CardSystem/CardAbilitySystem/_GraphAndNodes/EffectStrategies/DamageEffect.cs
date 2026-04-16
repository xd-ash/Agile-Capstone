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

            foreach (GameObject target in abilityData.Targets)
            {
                if (target != null && target.TryGetComponent(out Unit targetUnit))
                {
                    var def = graph as CardAbilityDefinition;
                    var abilityPos = abilityData.AbilityTriggerPos == -Vector2Int.one ? 
                        ByteMapController.Instance.GetPositionOfUnit(abilityData.GetUnit) : abilityData.AbilityTriggerPos;
                    bool hit = CombatMath.RollHit(abilityPos, targetUnit, def);
                    //bool hit = CombatMath.RollHit(abilityData.GetUnit.transform.localPosition, targetUnit, def);

                    _visualsStrategy?.CreateVisualEffect(abilityData, target); //do effect visuals

                    if (!hit) continue;
                    var adjustedEffectVal = GetRarityAdjustedEffectValue(abilityData.GetCardRarity);
                    targetUnit.ChangeHealth(adjustedEffectVal, false);
                    targetUnit.GetFloatingText.SpawnFloatingText($"-{adjustedEffectVal}", TextPresetType.DamagePreset);
                }
            }
            
            _onFinished?.Invoke();
        }
    }
}