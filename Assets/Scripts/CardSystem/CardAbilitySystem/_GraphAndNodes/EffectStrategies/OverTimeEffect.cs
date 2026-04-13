using CardSystem;
using System.Collections.Generic;
using System;
using UnityEngine;
using XNode;

[CreateNodeMenu("Misc Effects/Over Time Effect")]
public class OverTimeEffect : EffectStrategy, IUseEffectValue
{
    [Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte effects;

    [SerializeField] private bool _doEffectOnApply = true;
    [SerializeField] private bool _tickOnStart = true;

    public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0)
    {
        base.StartEffect(abilityData, onFinished, effectValueChange);

        var adjustedEffectVal = GetRarityAdjustedEffectValue(abilityData.GetCardRarity);

        foreach (GameObject target in abilityData.Targets)
        {
            if (target == null) return;
            
            if (target.TryGetComponent(out ActiveEffectsTracker eTracker))
            {
                foreach (NodePort port in Outputs)
                {
                    if (port.Connection == null || port.Connection.node == null || port.Connection.node is not EffectStrategy)
                        continue;

                    EffectStrategy strat = port.Connection.node as EffectStrategy;

                    AbilityData tmp = new(abilityData);
                    tmp.Targets = new [] { target };

                    if (_doEffectOnApply)
                        strat.StartEffect(tmp, onFinished);//initial effect trigger before store
                    eTracker.AddEffect(() => 
                    {
                        strat.StartEffect(tmp, onFinished);
                    }, adjustedEffectVal, Guid.NewGuid(), _tickOnStart, strat.name);
                }
            }
            else
                Debug.LogError($"Target failure in OverTimeEffect strategy. " + target == null ? "Target is null" : $"Effect Tracker not attached to {target.name}");
        }

        _onFinished?.Invoke();
    }
}
