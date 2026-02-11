using System;
using CardSystem;
using UnityEngine;

public class OverTimeEffect : EffectStrategy
{
    [SerializeField] private int _turnDuration;
    public int GetDuration => _turnDuration;

    public override void StartEffect(AbilityData abilityData, Action onFinished)
    {
        base.StartEffect(abilityData, onFinished);

        foreach (GameObject target in abilityData.Targets)
        {
            if (target != null && target.TryGetComponent<Unit>(out Unit targetUnit))
            {

            }
        }

        onFinished();
    }
}
