using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using XNode;

namespace CardSystem
{
	// Start node for an ability
    [CreateNodeMenu("Ability Root Node")]
    public class AbilityRootNode : AbilityNodeBase
	{
        //[SerializeField] private int _apCost;
        //[SerializeField] private int _range;
        [Flags] public enum EffectTypes{ Helpful = 2, Harmful = 4, Misc = 8 }
        [SerializeField] private EffectTypes _effectTypes;

        [Output(connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public short targeting;
		[Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public double filtering;
		[Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public bool helpfulEffects;
		[Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public int harmfulEffects;
		[Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte miscEffects;

        private CardAbilityDefinition _cardDefinition => this.graph as CardAbilityDefinition;
		private TargetingStrategy _targetingStrategy;

        public EffectTypes GetEffectTypes { get { return _effectTypes; } }

		// check if user (Unit) is able to use abailty with AP, start targeting
		// based on connected targeting strategy port
        public void UseAbility(Unit user)
		{
            if (_targetingStrategy == null)
                _targetingStrategy = GetPort("targeting").Connection.node as TargetingStrategy;

			if (!user.SpendAP(_cardDefinition.GetApCost, false)) return; // simply check if ap can be spent

			AbilityData abilityData = new AbilityData(user);
			_targetingStrategy?.StartTargeting(abilityData, () =>
			{
                // Method sent through to be called after targeting strategy finishes
                InitAbility(abilityData);
			});
        }

		private void InitAbility(AbilityData abilityData)
		{
			//do each filter connected to root node
			foreach (NodePort port in Outputs)
			{
				if (port.Connection == null || port.Connection.node == null || port.Connection.node is FilterStrategy == false)
					continue;

				abilityData.Targets = (port.Connection.node as FilterStrategy).Filter(abilityData.Targets, abilityData.GetUnit);
			}

            // failed ability cast catcher
  			if (abilityData.GetUnit.team == Team.Friendly && _targetingStrategy is not Targetless && (abilityData.Targets == null || abilityData.GetTargetCount == 0))
            {
                // Return the card to hand or destroy it
                if (DeckAndHandManager.instance != null && DeckAndHandManager.instance.GetSelectedCard != null)
                {
                    var cardSelect = DeckAndHandManager.instance.GetSelectedCard.GetCardTransform.GetComponent<CardSelect>();
                    cardSelect?.ReturnCardToHand();
                }
                AbilityEvents.TargetingStopped();
                return;
            }

            //Do each effect connected to root node
            foreach (NodePort port in Outputs)
			{
				if (port.Connection == null || port.Connection.node == null || port.Connection.node is EffectStrategy == false)
					continue;

				EffectStrategy curEffect = port.Connection.node as EffectStrategy;
				curEffect.StartEffect(abilityData, OnEffectFinished);
			}

            abilityData.GetUnit.SpendAP(_cardDefinition.GetApCost);//actually use the ap
            //if (abilityData.GetUnit.team == Team.Friendly)
                //AbilityEvents.TargetingStopped();
            AbilityEvents.AbilityUsed(abilityData.GetUnit.team); //moved here to avoid early card removal/delete on multi effect cards
        }

        // Unused method for now, kept just for reminder of tutorial system setup
        private void OnEffectFinished()
		{
			//
		}

        // Not sure what this is and why it's required (or if I even set it up correctly) ¯\_(ツ)_/¯
		// I think this is just grabbing each port's data identifier type
        public override object GetValue(NodePort port)
        {
            if (port.fieldName == "targeting")
                return GetInputValue<short>("targeting");
            else if (port.IsDynamic)
            {
                if (port.GetConnections().Count == 0) return null;

				if (port.fieldName.Contains("filtering"))
					return GetInputValue<double>("filtering");
				else if (port.fieldName.Contains("helpfulEffects"))
					return GetInputValue<bool>("helpfulEffects");
                else if (port.fieldName.Contains("harmfulEffects"))
                    return GetInputValue<int>("harmfulEffects");
                else if (port.fieldName.Contains("miscEffects"))
                    return GetInputValue<byte>("miscEffects");
            }
            throw new System.Exception($"{this.GetType()}.GetValue() Override issue");
        }
    }
}