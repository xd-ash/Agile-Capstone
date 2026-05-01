using CardSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardAndPackLibrary", menuName = "Libraries/New Card & Pack Library")]
public class CardAndPackLibrary : ScriptableObject
{
    [SerializeField] private List<CardAbilityDefinition> _cardsInProject = new();

    public List<CardAbilityDefinition> GetCardsInProject => _cardsInProject;
    public CardAbilityDefinition[] GetAllCommonCards => GetCardsOfRarity(CardRarity.Common);
    public CardAbilityDefinition[] GetAllRareCards => GetCardsOfRarity(CardRarity.Rare);
    public CardAbilityDefinition[] GetAllEpicCards => GetCardsOfRarity(CardRarity.Epic);

    public static Action GrabAssets;

    public void AddCardToLibrary(CardAbilityDefinition card)
    {
        if (card == null) return;

        if (!_cardsInProject.Contains(card))
            _cardsInProject.Add(card);
    }
    public void CleanUpLists()
    {
        for (int i = _cardsInProject.Count - 1; i >= 0; i--)
            if (_cardsInProject[i] == null)
                _cardsInProject.RemoveAt(i);
    }
    public void ClearCardLibrary()
    {
        _cardsInProject.Clear();
    }
    public CardAbilityDefinition GetCardFromName(string cardName)
    {
        foreach (var card in _cardsInProject)
            if (card.name == cardName)
                return card;

        Debug.LogWarning($"No matching card definition found in library for \"{cardName}\"");
        return null;
    }
    public CardAbilityDefinition[] GetCardsOfRarity(CardRarity rarity)
    {
        List<CardAbilityDefinition> temp = new();

        foreach (var card in _cardsInProject)
            if (card != null && card.GetBaseCardRarity == rarity)
                temp.Add(card);
        return temp.ToArray();
    }
    public CardAbilityDefinition[] GetCardsOfCategory(CardCategory category)
    {
        List<CardAbilityDefinition> temp = new();

        foreach (var card in _cardsInProject)
            if (card != null && card.GetCardCategory == category)
                temp.Add(card);
        return temp.ToArray();
    }
}
