using CardSystem;
using UnityEngine;
using System.Collections.Generic;

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
    public Deck(List<Card> cards)
    {
        _cardsInDeck.Clear();
        foreach (var card in cards)
            AddCard(card);
    }    
    public Deck(Deck deck)
    {
        _cardsInDeck = deck._cardsInDeck;
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

    public bool Contains(Card card)
    {
        foreach (var deckCard in _cardsInDeck)
            if(deckCard == card)
                return true;
        return false;
    }
}
