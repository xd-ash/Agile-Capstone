using CardSystem;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

[System.Serializable]
public class Deck
{
    [SerializeField] private List<Card> _cardsInDeck = new();
    public List<Card> GetCardsInDeck => _cardsInDeck;

    public Deck(List<CardAbilityDefinition> cardAbilities)
    {
        _cardsInDeck.Clear();
        foreach (var cardAbility in cardAbilities)
            AddCard(cardAbility);
    }

    public bool AddCard(CardAbilityDefinition cardAbility, Transform cardTransform = null)
    {
        if (cardAbility == null) return false;
        _cardsInDeck.Add(new(cardAbility, cardTransform));
        return true;
    }
    public bool AddCard(Card card)
    {
        if (card == null || card.GetCardAbility == null) return false;
        _cardsInDeck.Add(card);
        return true;
    }

    public bool RemoveCard(Card card)
    {
        if (card == null) return false;
        _cardsInDeck.Remove(card);
        return true;
    }

    public bool Contains(CardAbilityDefinition cardAbility)
    {
        foreach (var card in _cardsInDeck)
            if(cardAbility == card.GetCardAbility)
                return true;
        return false;
    }
    /*
    public void ClearDeck()
    {
        _cardsInDeck.Clear();
    }
    */
}
public class CardPack
{
    [SerializeField] private List<CardAbilityDefinition> _cardsInPack = new();
    [SerializeField] private string _packName;

    public List<CardAbilityDefinition> GetCardsInDeck => _cardsInPack;
    public string GetPackName => _packName;

    public CardPack(string name, List<CardAbilityDefinition> cardAbilities)
    {
        _cardsInPack = cardAbilities;
        _packName = name;
    }

    public void ClearPack()
    {
        _cardsInPack.Clear();
    }
    public void ClearPack(List<CardAbilityDefinition> cards)
    {
        _cardsInPack = new(cards);
    }
}
