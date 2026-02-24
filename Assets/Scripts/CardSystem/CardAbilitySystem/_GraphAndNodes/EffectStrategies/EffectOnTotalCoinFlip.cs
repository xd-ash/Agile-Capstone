using System;
using UnityEngine;
using XNode;

namespace CardSystem 
{
    [CreateNodeMenu("Gambling Effects/On Total Coin Flips")]
    public class EffectOnTotalCoinFlip : EffectStrategy
    {
        [Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte effects;

        [Tooltip("Heads - True, Tails - False")]
        [SerializeField] private bool desiredCoinOutSide = false;

        public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0)
        {
            base.StartEffect(abilityData, onFinished, effectValueChange);

            //check each effect connected to node
            foreach (NodePort port in Outputs)
            {
                if (port.Connection == null || port.Connection.node == null || port.Connection.node is EffectStrategy == false)
                    continue;

                int amountOfCoinFlips = 0;
                if (SpecialMechanicsManager.Instance != null)
                    amountOfCoinFlips = desiredCoinOutSide ? SpecialMechanicsManager.Instance.GetNumHeadsThisCombat(abilityData.GetUnit) :
                                                             SpecialMechanicsManager.Instance.GetNumTailsThisCombat(abilityData.GetUnit);

                EffectStrategy curEffect = port.Connection.node as EffectStrategy;
                curEffect.StartEffect(abilityData, onFinished, amountOfCoinFlips);
                abilityData.GetUnit.GetFloatingText.SpawnFloatingText($"{amountOfCoinFlips} {(desiredCoinOutSide ? "Heads" : "Tails")}");
            }

            _onFinished?.Invoke();
        }
    }
}