using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace CardSystem
{
	public class AbilityRootNode : AbilityNodeBase
	{
		[Output(connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public bool targeting;
		[Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public int filtering;
		[Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte effects;

		private TargetingStrategy _targetingStrategy;

		public void UseAbility(Unit user)
		{
            if (_targetingStrategy == null)
            {
				_targetingStrategy = GetPort("targeting").Connection.node as TargetingStrategy;
            }

			AbilityData abilityData = new AbilityData(user);
			_targetingStrategy?.StartTargeting(abilityData, () =>
			{
				InitAbility(abilityData);
				//Debug.Log("init ability called");
			});
        }

		private void InitAbility(AbilityData abilityData)
		{
			foreach (NodePort port in Outputs)
			{
				if (port.Connection == null || port.Connection.node == null || port.Connection.node is FilterStrategy == false)
				{
					continue;
				}

				abilityData.Targets = (port.Connection.node as FilterStrategy).Filter(abilityData.Targets); //filter is set up in filter strategy
			}
			foreach (NodePort port in Outputs)
			{
				if (port.Connection == null || port.Connection.node == null || port.Connection.node is EffectStrategy == false)
				{
					continue;
				}

				EffectStrategy curEffect = port.Connection.node as EffectStrategy;
				curEffect.StartEffect(abilityData, OnEffectFinished);
			}
		}

		private void OnEffectFinished()
		{
			//end turn? other stuff?
		}
	}
}