using CardSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardAndPackLibrary", menuName = "Libraries/New Card & Pack Library")]
public class CardAndPackLibrary : ScriptableObject
{
    [SerializeField] private List<CardPack> _packsInProject = new();
    //[SerializeField] private List<CardAbilityDefinition> _shopPool;

    [SerializeField] private List<CardAbilityDefinition> _cardsInProject = new();

    public List<CardAbilityDefinition> GetCardsInProject => _cardsInProject;
    public List<CardPack> GetPacksInProject => _packsInProject;

    public static Action GrabAssets;

    public void AddCardToLibrary(CardAbilityDefinition card)
    {
        if (card == null) return;

        if (!_cardsInProject.Contains(card))
            _cardsInProject.Add(card);
    }
    public void CleanUpLists()
    {
        for (int i = _packsInProject.Count - 1; i >= 0; i--)
            if (_packsInProject[i] == null)
                _packsInProject.RemoveAt(i);
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
    public CardPack GetPackFromName(string packName, bool sendDebugOnFail = true)
    {
        //check library starter decks
        foreach (var pack in _packsInProject)
            if (pack.GetPackName == packName)
                return pack;

        //Check player decks
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.GetAllPlayerPacks != null)
            foreach (var pack in PlayerDataManager.Instance.GetAllPlayerPacks)
                if (pack.GetPackName == packName)
                    return pack;

        if (sendDebugOnFail)
            Debug.LogError($"No matching card pack found in library for \"{packName}\"");
        return null;
    }
}
