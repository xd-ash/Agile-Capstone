using TMPro;
using UnityEngine;
using System.Collections.Generic;
using CardSystem;
using System.Linq;

public class DeckInitializer : MonoBehaviour
{
    private CardAndPackLibrary _cardAndPackLibrary;

    private Dictionary<CardRarity, List<CardAbilityDefinition>> _currentCardsByRarity = new();

    [SerializeField] private int _numberRandomPacks = 3;
    [SerializeField] private int _numCardsInPack = 5;
    [SerializeField] private int _maxRareCardsInPack = 2;
    [SerializeField] private int _maxEpicCardsInPack = 1;

    public static DeckInitializer Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void OnEnable()
    {
        _cardAndPackLibrary = Resources.Load<CardAndPackLibrary>("Libraries/CardAndPackLibrary");
    }

    public void GetNewPlayerDeck()
    {
        var newDeck = CreateNewDeck();
        PlayerDataManager.Instance.UpdateCardData(newDeck);
    }
    private Deck CreateNewDeck()
    {
        InitRarityDict();

        for (int i = 0; i < _numberRandomPacks; i++)
        {
            List<CardAbilityDefinition> currentPack = new();

            var rngRareCards = GetRandomCardsOfRarity(_maxRareCardsInPack, currentPack);
            if (rngRareCards != null)
                currentPack.AddRange(rngRareCards);
            var rngEpicCards = GetRandomCardsOfRarity(_maxEpicCardsInPack, currentPack);
            if (rngEpicCards != null)
                currentPack.AddRange(rngEpicCards);

            //fill remaining "pack" cards with common fewer than max epic/rare cards
            int remaining = _numCardsInPack - (_numCardsInPack - currentPack.Count);
            var rngCommonCards = GetRandomCardsOfRarity(remaining, currentPack);
            if (rngCommonCards != null)
                currentPack.AddRange(rngCommonCards);

            AddCardsToDict(CardRarity.Common, rngCommonCards);
            AddCardsToDict(CardRarity.Rare, rngRareCards);
            AddCardsToDict(CardRarity.Epic, rngEpicCards);
        }

        return CreateDeckFromRarityDict();
    }
    private Deck CreateDeckFromRarityDict()
    {
        List<Card> tempCards = new();
        foreach (var kvp in _currentCardsByRarity)
            foreach (var def in kvp.Value)
                tempCards.Add(new(def, kvp.Key));
        return new(tempCards);
    }
    private void InitRarityDict()
    {
        _currentCardsByRarity.Clear();
        _currentCardsByRarity = new()
        {
            [CardRarity.Common] = new(),
            [CardRarity.Rare] = new (),
            [CardRarity.Epic] = new (),
        };
    }
    private void AddCardsToDict(CardRarity rarity, CardAbilityDefinition[] cards)
    {
        if (_currentCardsByRarity.ContainsKey(rarity))
            _currentCardsByRarity[rarity].AddRange(cards);
        else
            _currentCardsByRarity.Add(rarity, new(cards));
    }
    private CardAbilityDefinition[] GetRandomCardsOfRarity(int num, List<CardAbilityDefinition> currentPack)
    {
        var cards = _cardAndPackLibrary?.GetCardsInProject.ToArray();
        if (cards == null || cards.Length == 0) return null;

        List<CardAbilityDefinition> tempList = new();
        for (int i = 0; i < num; i++)
        {
            CardAbilityDefinition tempCard = null;

            //no duplicates withtin same pack
            do
            {
                int rng = Random.Range(0, cards.Length);
                tempCard = cards[rng];
            } while (tempList.Contains(tempCard) || currentPack.Contains(tempCard));
            tempList.Add(tempCard);
        }
        return tempList.ToArray();
    }
}
