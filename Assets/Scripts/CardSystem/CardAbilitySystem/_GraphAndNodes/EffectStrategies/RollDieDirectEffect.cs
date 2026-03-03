using System;
using XNode;
using static DiceRoll;

namespace CardSystem
{
    [CreateNodeMenu("Gambling Effects/Dice Direct Effect")]
    public class RollDieDirectEffect : EffectStrategy
    {
        [Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte effects;

        public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0)
        {
            base.StartEffect(abilityData, onFinished, effectValueChange);

            //check each effect connected to node
            foreach (NodePort port in Outputs)
            {
                if (port.Connection == null || port.Connection.node == null || port.Connection.node is EffectStrategy == false)
                    continue;

                int diceResult = RollDice(abilityData.GetUnit)[0];
                abilityData.GetUnit.GetFloatingText?.SpawnFloatingText($"Roll: {diceResult}", TextPresetType.MissTextPreset);

                EffectStrategy curEffect = port.Connection.node as EffectStrategy;
                curEffect.StartEffect(abilityData, onFinished, diceResult);
            }

            _onFinished?.Invoke();
        }
    }
}