using System;
using XNode;
using static DiceRoll;

namespace CardSystem
{
    [CreateNodeMenu("Gambling Effects/Dice Rolls Doubles Effect")]
    public class RollDicePairForDoubles : EffectStrategy, IUseEffectValue
    {
        [Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte onDoubles;
        [Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte onSnakeEyes;
        [Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte onNone;

        public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0)
        {
            base.StartEffect(abilityData, onFinished, effectValueChange);

            var diceResult = RollDicePairForDoubles(abilityData.GetUnit, _effectValue);
            abilityData.GetUnit.GetFloatingText?.SpawnFloatingText($"{diceResult.ToString()}", TextPresetType.CoinFlipPreset);

            //check each effect connected to node
            foreach (NodePort port in Outputs)
            {
                if (port.Connection == null || port.Connection.node == null || port.Connection.node is EffectStrategy == false)
                    continue;

                string portName = port.fieldName.Split(' ')[0];
                switch (diceResult)
                {
                    case SpecialDiceOutcome.SnakeEyes:
                        if (portName != "onSnakeEyes") continue;
                        break;
                    case SpecialDiceOutcome.Double:
                        if (portName != "onDoubles") continue;
                        break;
                    case SpecialDiceOutcome.None:
                        if (portName != "onNone") continue;
                        break;
                }

                EffectStrategy curEffect = port.Connection.node as EffectStrategy;
                curEffect.StartEffect(abilityData, onFinished);
            }

            _onFinished?.Invoke();
        }
    }
}