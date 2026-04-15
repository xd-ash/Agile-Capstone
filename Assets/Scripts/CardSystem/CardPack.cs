using CardSystem;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CardPack
{
    [SerializeField] private string _packName;
    [SerializeField] private List<CardAbilityDefinition> _cardsInPack = new();

    public List<CardAbilityDefinition> GetCardsInPack => _cardsInPack;
    public string GetPackName => _packName;

    public CardPack(string name, List<CardAbilityDefinition> cardAbilities = null)
    {
        _packName = name;
        _cardsInPack = cardAbilities ?? new();
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
