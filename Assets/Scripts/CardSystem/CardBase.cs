using UnityEngine;

namespace CardSystem
{
    [System.Serializable]
    public class Card
    {
        //initial constructor uses AbilityDefinition param to grab card data
        public Card(CardAbilityDefinition def)
        {
            GrabSOData(def);
        }

        [SerializeField] private string _cardName;
        private string _description;
        [SerializeField] private Transform _cardTransform;
        [SerializeField] private CardAbilityDefinition _cardAbility;
        [SerializeField] private int _shopCost;

        public string GetCardName { get => _cardName; }
        public string GetDescription { get => _description; }
        public Transform CardTransform { get => _cardTransform; set => _cardTransform = value; }
        public CardAbilityDefinition GetCardAbility { get => _cardAbility; }
        public int ShopCost { get => _shopCost; set => _shopCost = value; }

        public virtual void GrabSOData(CardAbilityDefinition def)
        {
            _cardName = def.GetCardName;
            _description = def.GetDescription;
            _cardAbility = def;
        }
    }
}
