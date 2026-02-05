using CardSystem;
using UnityEngine;

[System.Serializable]
public class Deck
{
    [SerializeField] private string _deckName;
    [SerializeField] private CardAbilityDefinition[] _cardsInDeck;
    public string GetDeckName => _deckName;
    public CardAbilityDefinition[] GetCardsInDeck => _cardsInDeck;
}
