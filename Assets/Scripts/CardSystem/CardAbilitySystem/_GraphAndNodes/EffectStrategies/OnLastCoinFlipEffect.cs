using System;
using UnityEngine;
using XNode;

namespace CardSystem
{
    [CreateNodeMenu("Gambling Effects/On Last Coin Flip")]
    public class OnLastCoinFlipEffect : EffectStrategy
    {
        [Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte onHeads;
        [Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte onTails;

        public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0)
        {
            base.StartEffect(abilityData, onFinished, effectValueChange);

            bool lastCoinFlip = SpecialMechanicsManager.Instance.GetLastCoinFlipOutcome(abilityData.GetUnit);

            //check each effect connected to node
            foreach (NodePort port in Outputs)
            {
                if (port.Connection == null || port.Connection.node == null || port.Connection.node is EffectStrategy == false)
                    continue;

                bool portCoinSide = port.fieldName.Split(' ')[0] == "onHeads";//grab the "onHeads" or "onTails" of the the port field name and assign coin side/bool
                //filter strats by coinflip results
                if (lastCoinFlip != portCoinSide)
                    continue;

                EffectStrategy curEffect = port.Connection.node as EffectStrategy;
                curEffect.StartEffect(abilityData, onFinished);
            }

            _onFinished?.Invoke();
        }
    }
}