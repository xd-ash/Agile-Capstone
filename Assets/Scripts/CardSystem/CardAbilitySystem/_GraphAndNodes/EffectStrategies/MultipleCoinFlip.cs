using System;
using UnityEngine;
using XNode;

namespace CardSystem
{
    [CreateNodeMenu("Gambling Effects/Multiple Coin Flips")]
    public class MultipleCoinFlip : EffectStrategy, IUseEffectValue
    {
        [Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte effects;

        [SerializeField] private bool _desiredCoinSide;

        public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0)
        {
            base.StartEffect(abilityData, onFinished, effectValueChange);

            var multiflipResults = CoinFlip.FlipCoin(abilityData.GetUnit, _effectValue); //why do I have to do this

            int count = 0;
            foreach (var flipResult in multiflipResults)
                if (flipResult == _desiredCoinSide)
                    count++;

            count = Math.Min(count, _effectValue);
            abilityData.GetUnit?.GetFloatingText?.SpawnFloatingText($"{count} {(_desiredCoinSide ? "Heads" : "Tails")}", TextPresetType.CoinFlipPreset);

            //Do each effect connected to node
            foreach (NodePort port in Outputs)
            {
                if (port.Connection == null || port.Connection.node == null || port.Connection.node is EffectStrategy == false)
                    continue;

                EffectStrategy curEffect = port.Connection.node as EffectStrategy;
                curEffect.StartEffect(abilityData, onFinished, count);
            }

            _onFinished?.Invoke();
        }
    }
}