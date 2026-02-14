using CardSystem;
using System;
using UnityEngine;

[CreateNodeMenu("Misc Effects/Stop Movement Effect")]
public class StopMovementEffect : EffectStrategy
{
    public override void StartEffect(AbilityData abilityData, Action onFinished)
    {
        base.StartEffect(abilityData, onFinished);

        foreach (GameObject target in abilityData.Targets)
        {
            //flip some new bool?
            //add this as aduration effect and have a* check for it?

        }

        onFinished();
    }
}
