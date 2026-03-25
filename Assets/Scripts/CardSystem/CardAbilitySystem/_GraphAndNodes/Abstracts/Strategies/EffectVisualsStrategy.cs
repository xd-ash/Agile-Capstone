using CardSystem;
using UnityEngine;

public abstract class EffectVisualsStrategy : AbilityNodeBase
{
    [Input(connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public long input;

    public abstract void CreateVisualEffect(AbilityData abilityData, Unit target);
}