using System;
using UnityEngine;

namespace CardSystem
{
    public abstract class EffectStrategy : AbilityNodeBase
    {
        [Input(connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte enter;

        public abstract void StartEffect(AbilityData abilityData, Action onFinished);
    }
}