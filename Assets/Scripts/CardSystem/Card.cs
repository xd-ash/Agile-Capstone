using System;
using UnityEngine;

namespace CardSystem
{
    [System.Serializable]
    public class Card
    {
        //initial constructor uses AbilityDefinition param to grab card data
        public Card(CardAbilityDefinition def, CardRarity rarity, Transform cardTransform = null)
        {
            GrabSOData(def);
            _rarity = rarity;
            _cardTransform = cardTransform;
            _guid = Guid.NewGuid();
        }
        public Card(Card refCard, Transform cardTransform = null)
        {
            GrabSOData(refCard.GetCardAbility);
            _rarity = refCard.GetCardRarity;
            _cardTransform = cardTransform;
            _guid = refCard.GetGuid;
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
        public CardRarity GetNextCardRarity => _rarity == CardRarity.Common ? CardRarity.Rare : CardRarity.Epic;
        public Guid GetGuid => _guid;
        public string GetCardName => _cardName;
        public string GetDescription => GetDynamicDescription();
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
        public void UpgradeCard()
        {
            if (_rarity == CardRarity.Epic)
            {
                Debug.LogWarning($"{_cardName} is max rarity (epic). Upgrade attempt failed.");
                return;
            }

            _rarity = GetNextCardRarity;
        }
        public void UseAbility(Unit user)
        {
            _cardAbility.UseAbility(user, _rarity);
        }
        private string GetDynamicDescription()
        {
            if (_cardAbility == null) return _description;

            var effects = _cardAbility.GetEffectOptions();

            var splitDescription = _description.Split('~');

            for (int i = 0; i < splitDescription.Length; i++)
            {
                if (i % 2 == 0) continue;
                if (!int.TryParse(splitDescription[i], out int index) || index > effects.Count - 1)
                {
                    Debug.LogWarning($"{_cardName} failed to parse split string at index {i}.");
                    return _description;
                }
                var effectAtIndex = effects[index];
                splitDescription[i] = Mathf.Abs(effectAtIndex.GetRarityAdjustedEffectValue(_rarity)).ToString();
            }
            return string.Join("", splitDescription);
        }
        public static string CreateNamingConventionString(Card card)
        {
            return $"{card.GetCardName}-{card.GetCardRarity.ToString()}";
        }
        public static Tuple<CardRarity, string> ReadNamingConventionString(string name)
        {
            var nameSections = name.Split('-');
            CardRarity rarity;
            switch (nameSections[1])
            {
                case "Rare":
                    rarity = CardRarity.Rare;
                    break;
                case "Epic":
                    rarity = CardRarity.Epic;
                    break;
                default:
                    rarity = CardRarity.Common;
                    break;
            }
            return new(rarity, nameSections[0]);
        }
    }
}
