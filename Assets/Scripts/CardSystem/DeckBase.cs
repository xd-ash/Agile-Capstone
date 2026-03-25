using CardSystem;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Deck
{
    [SerializeField] private string _deckName;
    [SerializeField] private List<CardAbilityDefinition> _cardsInDeck = new();
    public string GetDeckName => _deckName;
    public List<CardAbilityDefinition> GetCardsInDeck => _cardsInDeck;

    public Deck (string deckName)
    {
        _deckName = deckName;
    }
    public Deck (List<CardAbilityDefinition> cardsInDeck)
    {
        _cardsInDeck = cardsInDeck;
    }
    public Deck(string deckName, List<CardAbilityDefinition> cardsInDeck)
    {
        _deckName = deckName;
        _cardsInDeck = cardsInDeck;
    }

    public bool AddCardToDeck(CardAbilityDefinition card)
    {
        if (card == null) return false;
        _cardsInDeck.Add(card);
        return true;
    }
    public bool RemoveCardFromDeck(CardAbilityDefinition card)
    {
        if (card == null) return false;
        _cardsInDeck.Remove(card);
        return true;
    }
    public void ClearDeck() 
    {
        _cardsInDeck?.Clear();
    }
    public void ClearDeck(List<CardAbilityDefinition> newDeck)
    {
        _cardsInDeck = new(newDeck);
    }
}
