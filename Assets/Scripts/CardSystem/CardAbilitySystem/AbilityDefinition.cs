using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace CardSystem
{
	[CreateAssetMenu(fileName = "CardAbility", menuName = "Deckbuilding System/New Card Ability Definition")]
	public class AbilityDefinition : NodeGraph
	{
		private AbilityRootNode _rootNode;

		private AbilityRootNode RootNode
		{
			get
			{
				if(_rootNode == null)
				{
					foreach(AbilityNodeBase node in nodes)// nodes is built in collection of nodes
					{
						if (node is AbilityRootNode)
						{
							_rootNode = node as AbilityRootNode;
						}
					}
				}
				return _rootNode;
			}
		}

		public void UseAility(Unit user)
		{
			RootNode?.UseAbility(user);
		}
	}
}