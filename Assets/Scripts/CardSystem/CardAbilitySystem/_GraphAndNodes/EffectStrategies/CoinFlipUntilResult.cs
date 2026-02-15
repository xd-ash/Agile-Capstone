using System;
using UnityEngine;
using XNode;

namespace CardSystem
{
    // Flip coin until the desired outcome and affect the following effects by the number of undesired outcome flipped
    [CreateNodeMenu("Gambling Effects/Coin Flip Unitl Result")]
    public class CoinFlipUntilResult : EffectStrategy, ICoinFlip
    {
        [Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte effects;

        [Tooltip("Heads - True, Tails - False")]
        public bool desiredCoinOutcome = false;

        public override void StartEffect(AbilityData abilityData, Action onFinished)
        {
            base.StartEffect(abilityData, onFinished);

            var flipResults = (this as ICoinFlip).FlipCoin(desiredCoinOutcome); //why do I have to do this
            int undesiredCount = 0;
            foreach (var flipResult in flipResults)
                if (flipResult != desiredCoinOutcome)
                    undesiredCount++;
            undesiredCount = Math.Min(undesiredCount, effectValue);
            abilityData.GetUnit?.GetFloatingText?.SpawnFloatingText($"{undesiredCount} {(desiredCoinOutcome == false ? "Heads" : "Tails")}");

            //Do each effect connected to node
            foreach (NodePort port in Outputs)
            {
                if (port.Connection == null || port.Connection.node == null || port.Connection.node is EffectStrategy == false)
                    continue;

                EffectStrategy curEffect = port.Connection.node as EffectStrategy;

                int temp = curEffect.effectValue;
                curEffect.effectValue += undesiredCount;
                curEffect.StartEffect(abilityData, null);
                curEffect.effectValue = temp; //reset effect value to normal
            }
            onFinished?.Invoke();
        }
    }
}