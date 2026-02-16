using CardSystem;
using System;
using UnityEngine;
using XNode;

[CreateNodeMenu("Misc Effects/Over Time Effect")]
public class OverTimeEffect : EffectStrategy, IUseEffectValue
{
    [Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte effects;
    [SerializeField] private bool _doEffectAtStart = true;

    public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0)
    {
        base.StartEffect(abilityData, onFinished, effectValueChange);

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

                    if (_doEffectAtStart)
                        strat.StartEffect(abilityData, onFinished);//initial effect trigger before store
                    eTracker.AddEffect(() => { strat.StartEffect(abilityData, onFinished); }, effectValue, Guid.NewGuid(), strat.name);

                    foreach (var t in abilityData.Targets)
                        Debug.Log($"target-{target.name}");
                }
            }
            else
                Debug.LogError($"Target failure in OverTimeEffect strategy. " + target == null ? "Target is null" : $"Effect Tracker not attached to {target.name}");
        }

        onFinished?.Invoke();
    }
}
