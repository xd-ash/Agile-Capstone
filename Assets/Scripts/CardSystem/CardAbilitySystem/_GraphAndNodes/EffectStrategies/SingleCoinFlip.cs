using System;
using XNode;

namespace CardSystem
{
    [CreateNodeMenu("Gambling Effects/Single Coin Flip")]
    public class SingleCoinFlip : EffectStrategy
    {
        [Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte onHeads;
        [Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte onTails;

        public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0)
        {
            base.StartEffect(abilityData, onFinished, effectValueChange);

            bool coinflip = CoinFlip.FlipCoin(abilityData.GetUnit)[0];
            abilityData.GetUnit.GetFloatingText?.SpawnFloatingText($"{(coinflip ? "Heads" : "Tails")}", TextPresetType.CoinFlipPreset);

            //check each effect connected to node
            foreach (NodePort port in Outputs)
            {
                if (port.Connection == null || port.Connection.node == null || port.Connection.node is EffectStrategy == false)
                    continue;

                bool portCoinSide = port.fieldName.Split(' ')[0] == "onHeads";//grab the "onHeads" or "onTails" of the the port field name and assign coin side/bool
                //filter strats by coinflip results
                if (coinflip != portCoinSide)
                    continue;

                EffectStrategy curEffect = port.Connection.node as EffectStrategy;
                curEffect.StartEffect(abilityData, onFinished);
            }

            _onFinished?.Invoke();
        }
    }
}