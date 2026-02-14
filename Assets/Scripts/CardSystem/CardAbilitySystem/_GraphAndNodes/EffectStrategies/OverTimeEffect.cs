using CardSystem;
using System;
using UnityEngine;
using XNode;

[CreateNodeMenu("Misc Effects/Over Time Effect")]
public class OverTimeEffect : EffectStrategy
{
    [Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte effects;

    public override void StartEffect(AbilityData abilityData, Action onFinished)
    {
        base.StartEffect(abilityData, onFinished);

        foreach (GameObject target in abilityData.Targets)
        {
            if (target != null && target.TryGetComponent(out ActiveEffectsTracker eTracker))
            {
                foreach (NodePort port in Outputs)
                {
                    if (port.Connection == null || port.Connection.node == null || port.Connection.node is not EffectStrategy)
                        continue;

                    EffectStrategy strat = port.Connection.node as EffectStrategy;

                    eTracker.AddEffect((() => strat.StartEffect(abilityData, null)), _effectValue, Guid.NewGuid(), strat.name);

                    foreach (var t in abilityData.Targets)
                        Debug.Log($"target-{target.name}");
                }
            }
            else
                Debug.LogError($"Target failure in OverTimeEffect strategy. " + target == null ? "Target is null" : $"Effect Tracker not attached to {target.name}");
        }

        onFinished();
    }
}
