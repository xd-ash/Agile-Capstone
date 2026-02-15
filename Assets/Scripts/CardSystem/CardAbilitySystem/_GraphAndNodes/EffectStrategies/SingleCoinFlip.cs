using System;
using UnityEngine;
using XNode;

namespace CardSystem
{
    [CreateNodeMenu("Gambling Effects/Single Coin Flip")]
    public class SingleCoinFlip : EffectStrategy, ICoinFlip
    {
        [Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte onHeads;
        [Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte onTails;

        public override void StartEffect(AbilityData abilityData, Action onFinished)
        {
            base.StartEffect(abilityData, onFinished);

            bool coinflip = (this as ICoinFlip).FlipCoin()[0];
            
            //check each effect connected to node
            foreach (NodePort port in Outputs)
            {
                if (port.Connection == null || port.Connection.node == null || port.Connection.node is EffectStrategy == false)
                    continue;

                //filter strats by coinflip results
                if (coinflip && port.fieldName == "On Tails" || !coinflip && port.fieldName == "On Heads") 
                    continue;

                EffectStrategy curEffect = port.Connection.node as EffectStrategy;
                curEffect.StartEffect(abilityData, null);
            }
            onFinished?.Invoke();
        }
    }
}