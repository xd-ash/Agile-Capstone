using System;
using UnityEngine;
using XNode;

namespace CardSystem
{
    // Flip coin until the desired outcome and affect the following effects by the number of undesired outcome flipped
    [CreateNodeMenu("Gambling Effects/Coin Flip Unitl Result")]
    public class CoinFlipUntilResult : EffectStrategy
    {
        [Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte effects;

        [Tooltip("Heads - True, Tails - False")]
        [SerializeField] private bool _desiredCoinOutcome = false;

        public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0)
        {
            base.StartEffect(abilityData, onFinished, effectValueChange);

            var flipResults = CoinFlip.FlipCoin(abilityData.GetUnit, _desiredCoinOutcome, _effectValue); //why do I have to do this

            int undesiredCount = 0;
            foreach (var flipResult in flipResults)
                if (flipResult != _desiredCoinOutcome)
                    undesiredCount++;
            undesiredCount = Math.Min(undesiredCount, _effectValue);
            abilityData.GetUnit?.GetFloatingText?.SpawnFloatingText($"{undesiredCount} {(_desiredCoinOutcome == false ? "Heads" : "Tails")}", TextPresetType.CoinFlipPreset);

            //Do each effect connected to node
            foreach (NodePort port in Outputs)
            {
                if (port.Connection == null || port.Connection.node == null || port.Connection.node is EffectStrategy == false)
                    continue;

                EffectStrategy curEffect = port.Connection.node as EffectStrategy;
                curEffect.StartEffect(abilityData, onFinished, undesiredCount);
            }

            _onFinished?.Invoke();
        }
    }
}