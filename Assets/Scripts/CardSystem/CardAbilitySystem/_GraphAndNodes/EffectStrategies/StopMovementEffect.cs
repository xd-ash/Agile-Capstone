using CardSystem;
using System;
using UnityEngine;

[CreateNodeMenu("Misc Effects/Stop Movement Effect")]
public class StopMovementEffect : EffectStrategy
{
    public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0)
    {
        base.StartEffect(abilityData, onFinished, effectValueChange);

        foreach (GameObject target in abilityData.Targets)
        {
            if (target != null && target.TryGetComponent(out Unit targetUnit))
            {
                var def = graph as CardAbilityDefinition;
                var abilityPos = abilityData.AbilityTriggerPos == -Vector2Int.one ?
                    ByteMapController.Instance.GetPositionOfUnit(abilityData.GetUnit) : abilityData.AbilityTriggerPos;
                bool hit = CombatMath.RollHit(abilityPos, targetUnit, def);
                //bool hit = CombatMath.RollHit(abilityData.GetUnit.transform.localPosition, targetUnit, def);
                
                _visualsStrategy?.CreateVisualEffect(abilityData, targetUnit); //do effect visuals

                if (!hit) continue;
                
                targetUnit.ToggleCanMove(false);
            }
        }

        _onFinished?.Invoke();
    }
}
