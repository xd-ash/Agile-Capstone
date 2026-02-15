using System;
using System.Collections;
using UnityEngine;
using XNode;

namespace CardSystem
{
    // Base abstract effect strategy class
    public abstract class EffectStrategy : AbilityNodeBase
    {
        public int effectValue;

        [Input(connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte input;

        protected EffectVisualsStrategy _visualsStrategy;
        [Output(connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public long effectVisuals;

        public virtual void StartEffect(AbilityData abilityData, Action onFinished)
        {
            var def = this.graph as CardAbilityDefinition;
            AudioManager.Instance?.SetPendingUseSfx(def.GetAbilitySFX);

            if (_visualsStrategy == null)
            {
                //make this better?
                try
                {
                    _visualsStrategy = GetPort("effectVisuals").Connection.node as EffectVisualsStrategy;
                }
                catch
                {
                    //Debug.Log("null effect visual");
                }
            }
        }
    }
}