using System;
using CardSystem;

[CreateNodeMenu("Misc Effects/End Effect")]
public class EndEffect : EffectStrategy
{
    public override void StartEffect(AbilityData abilityData, Action onFinished)
    {
        base.StartEffect(abilityData, onFinished);

        (graph as CardAbilityDefinition).EndEffects(abilityData.GetGUID);
    }
}