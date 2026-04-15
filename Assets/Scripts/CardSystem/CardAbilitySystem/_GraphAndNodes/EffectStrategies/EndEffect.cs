using System;
using CardSystem;

[CreateNodeMenu("Misc Effects/End Effect")]
public class EndEffect : EffectStrategy
{
    public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0, bool playAnimation = true)
    {
        base.StartEffect(abilityData, onFinished, effectValueChange, playAnimation);

        (graph as CardAbilityDefinition).EndEffects(abilityData.GetGUID);

        _onFinished?.Invoke();
    }
}