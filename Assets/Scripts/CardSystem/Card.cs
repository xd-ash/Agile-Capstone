using System;
using UnityEngine;

namespace CardSystem
{
    [System.Serializable]
    public class Card
    {
        //initial constructor uses AbilityDefinition param to grab card data
        public Card(CardAbilityDefinition def, Transform cardTransform = null)
        {
            GrabSOData(def);
            _cardTransform = cardTransform;
            _guid = Guid.NewGuid();
        }

        [SerializeField] private CardAbilityDefinition _cardAbility;
        [SerializeField] private CardRarity _rarity;
        [SerializeField] private Guid _guid;

        [SerializeField, HideInInspector] private string _cardName;
        private string _description;
        private Transform _cardTransform;
        private int _shopCost;

        public CardAbilityDefinition GetCardAbility => _cardAbility;
        public CardRarity GetCardRarity => _rarity;
        public Guid GetGuid => _guid;
        public string GetCardName => _cardName;
        public string GetDescription => _description;
        public Transform GetCardTransform => _cardTransform;
        public int GetShopCost => _shopCost;

        public void GrabSOData(CardAbilityDefinition def)
        {
            _cardAbility = def;
            _rarity = def.GetBaseCardRarity;

            _cardName = def.GetCardName;
            _description = def.GetDescription;
            _shopCost = def.GetShopCost;
        }
        public void OnPrefabCreation(Transform cardTransform)
        {
            _cardTransform = cardTransform;
        }
    }
}
