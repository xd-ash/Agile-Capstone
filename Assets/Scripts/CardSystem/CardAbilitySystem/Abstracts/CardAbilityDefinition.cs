using UnityEngine;
using XNode;

namespace CardSystem
{
	// NodeGraph of new card ability
	[CreateAssetMenu(fileName = "NewCardAbility", menuName = "Deckbuilding System/New Card Ability")]
	public class CardAbilityDefinition : NodeGraph
	{
        [SerializeField] private string _cardName;
        [TextArea(1, 3)]
        [SerializeField] private string _description;
		public AudioClip abilitySFX;

        private AbilityRootNode _rootNode;

        public string GetCardName { get { return _cardName; } }
        public string GetDescription { get { return _description; } }

        public AbilityRootNode RootNode
		{
			get
			{
				if(_rootNode == null)
					foreach(AbilityNodeBase node in nodes)// nodes is built in collection of nodes
						if (node is AbilityRootNode)
							_rootNode = node as AbilityRootNode;
				return _rootNode;
			}
		}

		public void UseAility(Unit user)
		{
			RootNode?.UseAbility(user);
		}
	}
}