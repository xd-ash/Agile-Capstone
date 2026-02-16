using CardSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

// Data manager for player data that will be saved in game data
public class PlayerDataManager : MonoBehaviour
{
    private CardAndDeckLibrary _cardAndDeckLibrary;

    private int _randomSeed = -1;
    private CombatMapData _currMapNodeData;

    private int _balance = 0;

    private bool[] _nodeCompleted;
    private bool[] _nodeUnlocked;
    private int _currentNodeIndex;

    [SerializeField] private List<Deck> _createdDecks = new();
    [SerializeField] private Deck _activeDeck;
    [SerializeField] private List<CardAbilityDefinition> _ownedCards = new();

    [SerializeField] private List<bool> _coinFlipsThisRun = new();

    public int GetSeed => _randomSeed == -1 ? GetRandomSeed() : _randomSeed;
    public CombatMapData GetCurrMapNodeData => _currMapNodeData;
    public int GetBalance => _balance;
    public bool[] GetNodeCompleted => _nodeCompleted;
    public bool[] GetNodeUnlocked => _nodeUnlocked;
    public int GetCurrentNodeIndex => _currentNodeIndex;
    public List<CardAbilityDefinition> GetOwnedCards => _ownedCards;
    public Deck GetActiveDeck => _activeDeck;
    public List<Deck> GetAllPlayerDecks => _createdDecks;
    public bool[] GetAllCoinFlipsThisRun => _coinFlipsThisRun.ToArray();
    public int GetNumHeadsThisRun => _coinFlipsThisRun.FindAll(x => true).Count;
    public int GetNumTailsThisRun => _coinFlipsThisRun.FindAll(x => false).Count;

    public static PlayerDataManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        if (_cardAndDeckLibrary == null)
            _cardAndDeckLibrary = Resources.Load<CardAndDeckLibrary>("Libraries/CardAndDeckLibrary");
        
        if (SaveLoadScript.CheckForSaveGame)
            SaveLoadScript.LoadGame?.Invoke();
        else
            SaveLoadScript.CreateNewGame?.Invoke();

        if (_activeDeck == null)
           SetActiveDeck(_cardAndDeckLibrary.GetDecksInProject[0]);

        WinLossManager.GameReset += ClearRunCoinFlips;
    }
    private void OnDestroy()
    {
        WinLossManager.GameReset -= ClearRunCoinFlips;
    }

    // create random int seed for map generation & shop card pulling
    public int GetRandomSeed()
    {
        int temp = _randomSeed;
        do
        {
            _randomSeed = UnityEngine.Random.Range(0, int.MaxValue - DateTime.Now.Millisecond) + DateTime.Now.Millisecond;
        } while (temp == _randomSeed);

        //Debug.Log("random Seed Generated");
        return _randomSeed;
    }
    // Data update methods for setting values
    public void UpdateCurrencyData(int currentBalance)
    {
        _balance = currentBalance;
    }
    public void UpdateNodeData(bool[] nodesCompleted, bool[] nodesUnlocked, int currentNodeIndex, int seed)
    {
        _nodeCompleted = nodesCompleted;
        _nodeUnlocked = nodesUnlocked;
        _currentNodeIndex = currentNodeIndex;
        _randomSeed = seed;
    }
    public void UpdateNodeData(bool[] nodesCompleted, bool[] nodesUnlocked)
    {
        _nodeCompleted = nodesCompleted;
        _nodeUnlocked = nodesUnlocked;
    }
    public void UpdateNodeData(int currentNodeIndex)
    {
        _currentNodeIndex = currentNodeIndex;
    }
    public void UpdateCardData(List<CardAbilityDefinition> ownedCards, string activeDeckName, List<Deck> createdDecks)
    {
        _ownedCards = ownedCards;
        _createdDecks = createdDecks;
        _activeDeck = _cardAndDeckLibrary.GetDeckFromName(activeDeckName, false);
    }
    public void UpdateCardData(CardAbilityDefinition def, bool isAddition = true)
    {
        if (def == null) return;

        if (isAddition)
            _ownedCards.Add(def);
        else
            if (_ownedCards.Contains(def))
                _ownedCards.Remove(def);
    }
    public void CreateOrAdjustDeck(Deck deck)
    {
        if (deck == null) return;
        for (int i = _createdDecks.Count - 1; i >= 0; i--)
            if (_createdDecks[i].GetDeckName == deck.GetDeckName)
                _createdDecks.RemoveAt(i);
        _createdDecks.Add(deck);
        SaveLoadScript.SaveGame?.Invoke();
    }
    public void DeleteDeck(Deck deck)
    {
        if (deck == null) return;
        for (int i = _createdDecks.Count - 1; i >= 0; i--)
            if (_createdDecks[i].GetDeckName == deck.GetDeckName)
                _createdDecks.RemoveAt(i);
        SaveLoadScript.SaveGame?.Invoke();
    }
    public void SetActiveDeck(Deck activeDeck)
    {
        if (activeDeck == null) return;
        _activeDeck = activeDeck;
    }

    public void SetCurrMapNodeData(CombatMapData currMapNodeData)
    {
        _currMapNodeData = currMapNodeData;
    }

    public void AddCoinflip(bool result)
    {
        if (_coinFlipsThisRun == null) _coinFlipsThisRun = new();
        _coinFlipsThisRun.Add(result);
    }
    public void ClearRunCoinFlips()
    {
        _coinFlipsThisRun?.Clear();
    }

    // On game load, update variable values using incoming data param and
    // reinitialize node data for proper node enabling on node map
    public void OnGameLoad(GameData data)
    {
        var currencyData = data.GetCurrencyData;
        var nodeData = data.GetMapNodeData;
        var cardData = data.GetCardData;
        var specialMechanicData = data.GetSpecialMechanicData;

        List<Deck> createdDecks = new();
        foreach (var deck in cardData.GetPlayerDecks)
        {
            List<CardAbilityDefinition> cards = new();
            foreach (var card in deck.cardNames)
                cards.Add(_cardAndDeckLibrary.GetCardFromName(card));
            createdDecks.Add(new(deck.deckName, cards));
        }

        List<CardAbilityDefinition> ownedCards = new();
        foreach (var name in cardData.GetOwnedCardNames)
            ownedCards.Add(_cardAndDeckLibrary.GetCardFromName(name));

        UpdateCurrencyData(currencyData.GetBalance);
        UpdateNodeData(nodeData.GetNodesCompleted, nodeData.GetNodesUnlocked, nodeData.GetCurrentNodeIndex, nodeData.GetSeed);
        UpdateCardData(ownedCards, cardData.GetActiveDeckName, createdDecks);

        _coinFlipsThisRun = new();
        foreach (var b in specialMechanicData.GetCoinFlipsCurrentRun)
            AddCoinflip(b);

        SceneProgressManager.Instance?.InitNodeData();
        CurrencyManager.Instance?.OnBalanceChanged?.Invoke(_balance);
        //Debug.Log("Game Loaded");
    }
}
