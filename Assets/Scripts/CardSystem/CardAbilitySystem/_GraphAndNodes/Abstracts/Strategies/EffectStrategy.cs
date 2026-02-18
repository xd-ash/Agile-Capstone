using System;
using UnityEngine;

namespace CardSystem
{
    // Base abstract effect strategy class
    public abstract class EffectStrategy : AbilityNodeBase
    {
        [SerializeField, HideInInspector] protected int _effectValue;
        protected Action _onFinished;

        [Input(connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte input;

        protected EffectVisualsStrategy _visualsStrategy;
        [Output(connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public long effectVisuals;

        public virtual void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0)
        {
            var def = this.graph as CardAbilityDefinition;
            AudioManager.Instance?.SetPendingUseSfx(def.GetAbilitySFX);

            if (_visualsStrategy == null)
                _visualsStrategy = GetPort("effectVisuals")?.Connection?.node as EffectVisualsStrategy;

            _onFinished = onFinished;

            //set up value change and store value reset within onfinished
            int temp = _effectValue;
            _effectValue += effectValueChange;
            _onFinished += () => { ResetValue(temp); };
        }
        protected void ResetValue(int initVal)
        {
            _effectValue = initVal;
        }
    }
}