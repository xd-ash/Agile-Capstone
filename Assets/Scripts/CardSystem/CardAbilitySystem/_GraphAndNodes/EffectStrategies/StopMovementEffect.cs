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
                bool hit = CombatMath.RollHit(targetUnit.transform.transform.localPosition, targetUnit, def);
                
                _visualsStrategy?.CreateVisualEffect(abilityData, targetUnit); //do effect visuals

                if (!hit) continue;
                
                targetUnit.ToggleCanMove(false);
            }
        }

        onFinished?.Invoke();
    }
}
