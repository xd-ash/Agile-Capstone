using UnityEngine;
using XNode;

namespace CardSystem
{
	// NodeGraph of new card ability
	[CreateAssetMenu(fileName = "NewCardAbility", menuName = "Deckbuilding System/New Card Ability")]
	public class CardAbilityDefinition : NodeGraph
	{
		[Header("Card Info")]
        [SerializeField] private string _cardName;
        [TextArea(1, 3)]
        [SerializeField] private string _description;
		[SerializeField] private AudioClip _abilitySFX;

		[Header("Card Data")]
        [SerializeField] private int _apCost;
        [SerializeField] private int _range;
        [SerializeField] private int _shopCost;
        [SerializeField] private int _shopWeight;

        private AbilityRootNode _rootNode;

        public string GetCardName => _cardName;
        public string GetDescription => _description;
        public int GetApCost => _apCost;
        public int GetRange => _range;
        public int GetShopCost => _shopCost;
        public int GetShopWeight => _shopWeight;
        public AudioClip GetAbilitySFX => _abilitySFX;

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