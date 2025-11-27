using UnityEngine;

namespace CardSystem
{
    [System.Serializable]
    public class Card
    {
        //initial constructor uses CardSO param to grab card data
        public Card(CardSO so)
        {
            GrabSOData(so);
        }

        [SerializeField] private string _cardName;
        private string _description;
        [SerializeField] private int _apCost;
        [SerializeField] private Transform _cardTransform;
<<<<<<< Updated upstream
        [SerializeField] private CardAbilityDefinition _cardAbility;
        [SerializeField] private int _shopCost;
=======
        [SerializeField] private AbilityDefinition _cardAbility;
        private GameObject _cardPrefab;
>>>>>>> Stashed changes

        public string GetCardName { get => _cardName; }
        public string GetDescription { get => _description; }
        public int APCost { get => _apCost; set { if (value >= 0) _apCost = value; } }
        public Transform CardTransform { get => _cardTransform; set => _cardTransform = value; }
<<<<<<< Updated upstream
        public CardAbilityDefinition GetCardAbility { get => _cardAbility; }
        public int ShopCost { get => _shopCost; set => _shopCost = value; }
=======
        public AbilityDefinition GetCardAbility { get => _cardAbility; }
        public GameObject GetCardPrefab { get => _cardPrefab; }
>>>>>>> Stashed changes

        public virtual void GrabSOData(CardSO so)
        {
            _cardName = so.GetCardName;
            _description = so.GetDescription;
            _apCost = so.GetAPCost;
            _cardAbility = so.GetCardAbility;
            _cardPrefab = so.GetCardPrefab;
        }
    }
}
