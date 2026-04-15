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

    public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0, bool playAnimation = true)
    {
        base.StartEffect(abilityData, onFinished, effectValueChange, playAnimation);

        var adjustedEffectVal = GetRarityAdjustedEffectValue(abilityData.GetCardRarity);

        foreach (GameObject target in abilityData.Targets)
        {
            if (target == null) return;
            
            if (target.TryGetComponent(out ActiveEffectsTracker eTracker) && target.TryGetComponent(out Unit targetUnit))
            {
                foreach (NodePort port in Outputs)
                {
                    if (port.Connection == null || port.Connection.node == null || port.Connection.node is not EffectStrategy)
                        continue;

                    var abilityPos = abilityData.AbilityTriggerPos == -Vector2Int.one ?
                        ByteMapController.Instance.GetPositionOfUnit(abilityData.GetUnit) : abilityData.AbilityTriggerPos;
                    bool hit = CombatMath.RollHit(abilityPos, targetUnit, graph as CardAbilityDefinition, false);
                    //bool hit = CombatMath.RollHit(abilityData.GetUnit.transform.localPosition, targetUnit, def);
                    _visualsStrategy?.CreateVisualEffect(abilityData, targetUnit); //do effect visuals

                    if (!hit) continue;

                    EffectStrategy strat = port.Connection.node as EffectStrategy;

                    AbilityData tmp = new(abilityData);
                    tmp.Targets = new [] { target };
                    
                    if (_doEffectOnApply)
                        strat.StartEffect(tmp, onFinished, 0, false);//initial effect trigger before store
                    eTracker.AddEffect(() => 
                    {
                        tmp.AbilityTriggerPos = ByteMapController.Instance.GetPositionOfUnit(targetUnit); //set ability trigger pos to target's pos before effect start
                        strat.StartEffect(tmp, onFinished, 0, false);
                    }, adjustedEffectVal, Guid.NewGuid(), _tickOnStart, strat.name);
                }
            }
            else
                Debug.LogError($"Target failure in OverTimeEffect strategy. " + target == null ? "Target is null" : $"Effect Tracker not attached to {target.name}");
        }

        _onFinished?.Invoke();
    }
}
