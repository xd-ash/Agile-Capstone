using UnityEngine;

namespace CardSystem
{
    [System.Serializable]
    public class Card
    {
        //initial constructor uses AbilityDefinition param to grab card data
        public Card(CardAbilityDefinition def, Transform cardTransform)
        {
            GrabSOData(def);
            _cardTransform = cardTransform;
        }

        [SerializeField] private string _cardName;
        private string _description;
        [SerializeField] private Transform _cardTransform;
        [SerializeField] private CardAbilityDefinition _cardAbility;
        [SerializeField] private int _shopCost;

        public string GetCardName => _cardName;
        public string GetDescription => _description;
        public Transform GetCardTransform => _cardTransform;
        public CardAbilityDefinition GetCardAbility => _cardAbility;
        public int GetShopCost => _shopCost;

        public virtual void GrabSOData(CardAbilityDefinition def)
        {
            _cardName = def.GetCardName;
            _description = def.GetDescription;
            _cardAbility = def;
            _shopCost = def.GetShopCost;
        }
    }
}
