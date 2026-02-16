using System;
using CardSystem;

[CreateNodeMenu("Misc Effects/End Effect")]
public class EndEffect : EffectStrategy
{
    public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0)
    {
        base.StartEffect(abilityData, onFinished, effectValueChange);

        (graph as CardAbilityDefinition).EndEffects(abilityData.GetGUID);
    }
}