using CardSystem;
using UnityEngine;
using XNode;
using System;
using System.Collections.Generic;
using System.Linq;

[CreateNodeMenu("Misc Effects/Health Percent Effect")]
public class HealthPercentEffect : EffectStrategy, IUseEffectValue
{
    [Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte aboveThreshold;
    [Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte belowThreshold;

    [SerializeField] private bool _useTargetPercent = false;

    public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0, bool playAnimation = true)
    {
        base.StartEffect(abilityData, onFinished, effectValueChange, playAnimation);

        //check unit health percent
        var unitHealthPercent = abilityData.GetUnit.GetHealth * 100 / abilityData.GetUnit.GetMaxHealth;
        bool isAbove = unitHealthPercent >= _effectValue;

        //filter targets by threshold and create new abilitydata for above & below port effects
        var targetsAbove = GetTargetsByThreshold(true, abilityData.Targets.ToList());
        var aboveAbilityData = new AbilityData(abilityData);
        aboveAbilityData.Targets = targetsAbove;

        var targetsBelow = GetTargetsByThreshold(false, abilityData.Targets.ToList());
        var belowAbilityData = new AbilityData(abilityData);
        belowAbilityData.Targets = targetsBelow;

        //check each effect connected to node
        foreach (NodePort port in Outputs)
        {
            if (port.Connection == null || port.Connection.node == null || port.Connection.node is EffectStrategy == false)
                continue;

            //determine which dynamic portlist the port/effect is contained in
            bool abovePort = port.fieldName.Split(' ')[0] == "aboveThreshold";//grab the "aboveThreshold" or "belowThreshold" of the the port field name 

            EffectStrategy curEffect = port.Connection.node as EffectStrategy;

            if (_useTargetPercent)
                curEffect.StartEffect(abovePort ? aboveAbilityData : belowAbilityData, onFinished); // based on current port, send relevant abilitydata/targets (only targets above health threshold are sent to abovePort effects etc.)
            else
            {
                if (isAbove != abovePort) continue;// if threshold result doesn't match dynamic portlist container, then continue
                curEffect.StartEffect(abilityData, onFinished);
            }
        }

        _onFinished?.Invoke();
    }

    private List<GameObject> GetTargetsByThreshold(bool checkAbove, List<GameObject> targets)
    {
        List<GameObject> temp = new();

        foreach (GameObject target in targets)
        {
            if (!target.TryGetComponent(out Unit tUnit)) continue;
            var tUnitHealthPercent = tUnit.GetHealth * 100 / tUnit.GetMaxHealth;
            if (checkAbove && tUnitHealthPercent >= _effectValue || !checkAbove && tUnitHealthPercent < _effectValue)
                temp.Add(target);
        }
        return temp;
    }
}
