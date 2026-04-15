using System;
using UnityEngine;
using XNode;

namespace CardSystem
{
    [CreateNodeMenu("Misc Effects/Multiply Effect")]
    public class MultiplyEffect : EffectStrategy, IUseEffectValue
    {
        [Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte doubledEffects;

        public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0, bool playAnimation = true)
        {
            base.StartEffect(abilityData, onFinished, effectValueChange, playAnimation);

            var adjustedEffectVal = GetRarityAdjustedEffectValue(abilityData.GetCardRarity);

            //check each effect connected to node
            foreach (NodePort port in Outputs)
            {
                if (port.Connection == null || port.Connection.node == null || port.Connection.node is EffectStrategy == false)
                    continue;
                EffectStrategy curEffect = port.Connection.node as EffectStrategy;

                for (int i = 0; i < adjustedEffectVal; i++)
                    curEffect.StartEffect(abilityData, onFinished);
            }

            _onFinished?.Invoke();
        }
    }
}