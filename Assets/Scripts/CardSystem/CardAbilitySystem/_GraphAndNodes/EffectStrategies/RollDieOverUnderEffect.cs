using System;
using UnityEngine;
using XNode;
using static DiceRoll;

namespace CardSystem
{
    [CreateNodeMenu("Gambling Effects/Dice Over Under Effect")]
    public class RollDieOverUnderEffect : EffectStrategy
    {
        [Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte onOver;
        [Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte onUnder;

        [SerializeField] private int _desiredMinRoll;

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

                bool portBool = port.fieldName.Split(' ')[0] == "onOver";
                bool resultBool = diceResult > _desiredMinRoll;
                if (resultBool != portBool) continue;

                EffectStrategy curEffect = port.Connection.node as EffectStrategy;
                curEffect.StartEffect(abilityData, onFinished);
            }

            _onFinished?.Invoke();
        }
    }
}