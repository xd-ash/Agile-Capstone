using TMPro;
using UnityEngine;
using System.Collections.Generic;
using CardSystem;
using System.Linq;

public class DeckInitializer : MonoBehaviour
{
    private CardAndPackLibrary _cardAndPackLibrary;
    [SerializeField] private int _numberRandomPacks = 3;
    [SerializeField] private int _numCardsInPack = 8;
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
        List<CardAbilityDefinition> tempDeckCards = new();

        for (int i = 0; i < _numberRandomPacks; i++)
        {
            var rngRareCards = GetRandomCardsOfRarity(_maxRareCardsInPack, CardRarity.Rare);
            if (rngRareCards != null)
                tempDeckCards.AddRange(rngRareCards);
            var rngEpicCards = GetRandomCardsOfRarity(_maxEpicCardsInPack, CardRarity.Epic);
            if (rngEpicCards != null)
                tempDeckCards.AddRange(rngEpicCards);

            //fill remaining "pack" cards with common fewer than max epic/rare cards
            int remaining = _numCardsInPack - (tempDeckCards.Count - i * _numCardsInPack);
            var rngCommonCards = GetRandomCardsOfRarity(remaining, CardRarity.Common);
            if (rngCommonCards != null)
                tempDeckCards.AddRange(rngCommonCards);
        }

        return new(tempDeckCards);
    }
    private CardAbilityDefinition[] GetRandomCardsOfRarity(int num, CardRarity rarity)
    {
        var cardsOfRarity = _cardAndPackLibrary?.GetCardsOfRarity(rarity);
        if (cardsOfRarity == null || cardsOfRarity.Length == 0) return null;

        List<CardAbilityDefinition> tempList = new();
        for (int i = 0; i < num; i++)
        {
            CardAbilityDefinition tempCard = null;

            //no duplicates
            do
            {
                int rng = Random.Range(0, cardsOfRarity.Length);
                tempCard = cardsOfRarity[rng];
            } while (tempList.Contains(tempCard));
            tempList.Add(tempCard);
        }
        return tempList.ToArray();
    }
}
