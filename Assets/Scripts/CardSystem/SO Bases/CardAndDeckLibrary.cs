using CardSystem;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "CardAndDeckLibrary", menuName = "Deckbuilding System/New Card & Deck Library")]
public class CardAndDeckLibrary : ScriptableObject
{
    //[SerializeField] private List<Deck> _decksInProject = new();
    [SerializeField] private List<DeckBase> _decksInProject = new();
    [SerializeField] private DeckBase _shopPool;

    [SerializeField] private List<CardAbilityDefinition> _cardsInProject = new();

    //public List<Deck> GetDecksInProject => _decksInProject;
    public List<DeckBase> GetDecksInProject => _decksInProject;
    public List<CardAbilityDefinition> GetCardsInProject => _cardsInProject;
    public DeckBase GetShopPool => _shopPool;

    public void AddDeckToLibrary(DeckBase deck)
    {
        if (deck == null) return;

        if (!_decksInProject.Contains(deck))
            _decksInProject.Add(deck);
    }
    public void AddCardToLibrary(CardAbilityDefinition card)
    {
        if (card == null) return;

        if (!_cardsInProject.Contains(card))
            _cardsInProject.Add(card);
    }
    public void CleanUpLists()
    {
        for (int i = _decksInProject.Count - 1; i >= 0; i--)
            if (_decksInProject[i] == null)
                _decksInProject.RemoveAt(i);
        for (int i = _cardsInProject.Count - 1; i >= 0; i--)
            if (_cardsInProject[i] == null)
                _cardsInProject.RemoveAt(i);
    }

    public CardAbilityDefinition GetCardFromName(string cardName)
    {
        foreach (var card in _cardsInProject)
            if (card.name == cardName)
                return card;

        Debug.LogError($"No matching card definition found in library for \"{cardName}\"");
        return null;
    }
    public DeckBase GetDeckFromName(string deckName)
    {
        foreach (var deck in _decksInProject)
            if (deck.GetDeckName == deckName)
                return deck;

        Debug.LogError($"No matching deck SO found in library for \"{deckName}\"");
        return null;
    }

}
