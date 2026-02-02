using CardSystem;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardAndDeckLibrary", menuName = "Deckbuilding System/New Card & Deck Library")]
public class CardAndDeckLibrary : ScriptableObject
{
    [SerializeField] private List<Deck> _decksInProject = new();
    [SerializeField] private List<CardAbilityDefinition> _cardsInProject = new();
    public List<Deck> GetDecksInProject => _decksInProject;
    public List<CardAbilityDefinition> GetCardsInProject => _cardsInProject;

    public void AddDeckToLibrary(Deck deck)
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
}
