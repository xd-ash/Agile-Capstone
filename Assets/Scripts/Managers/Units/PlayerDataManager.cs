using CardSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

// Data manager for player data that will be saved in game data
public class PlayerDataManager : MonoBehaviour
{
    private CardAndDeckLibrary _cardAndDeckLibrary;

    private int _balance = 0;

    private Vector2Int[] _completedNodes;
    [SerializeField] private Vector2Int _curNodeIndex = new(0,0);
    private int _generalSeed = -1;
    private int _nodeMapSeed = -1;
    private CombatMapData _currMapNodeData;

    [SerializeField] private List<Deck> _createdDecks = new();
    [SerializeField] private Deck _activeDeck;
    [SerializeField] private List<CardAbilityDefinition> _ownedCards = new();

    [SerializeField] private List<bool> _coinFlipsThisRun = new();
    [SerializeField] private List<int> _dieRollsThisRun = new();

    public int GetBalance => _balance;

    public Vector2Int[] GetCompletedNodes => _completedNodes;
    public Vector2Int GetCurrentNodeIndex => _curNodeIndex;
    public int GetGeneralSeed => _generalSeed == -1 ? GenerateRandomSeed(ref _generalSeed) : _generalSeed;
    public int GetNodeMapSeed => _nodeMapSeed == -1 ? GenerateRandomSeed(ref _nodeMapSeed) : _nodeMapSeed;
    public int GenerateGeneralSeed() => GenerateRandomSeed(ref _generalSeed);
    public int GenerateNodeMapSeed() => GenerateRandomSeed(ref _nodeMapSeed);
    public CombatMapData GetCurrMapNodeData => _currMapNodeData;

    public List<CardAbilityDefinition> GetOwnedCards => _ownedCards;
    public Deck GetActiveDeck => _activeDeck;
    public List<Deck> GetAllPlayerDecks => _createdDecks;

    public bool[] GetAllCoinFlipsThisRun => _coinFlipsThisRun.ToArray();
    public int GetNumHeadsThisRun => _coinFlipsThisRun.FindAll(x => true).Count;
    public int GetNumTailsThisRun => _coinFlipsThisRun.FindAll(x => false).Count;
    public int[] GetAllDiceRollsThisRun => _dieRollsThisRun.ToArray();

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

#if UNITY_EDITOR
        CardAndDeckLibrary.GrabAssets?.Invoke();
#endif

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
    public int GenerateRandomSeed(ref int seed)
    {
        int temp = seed;
        do
        {
            seed = UnityEngine.Random.Range(0, int.MaxValue - DateTime.Now.Millisecond) + DateTime.Now.Millisecond;
        } while (temp == seed);

        //Debug.Log("random Seed Generated");
        return seed;
    }

    // Data update methods for setting values
    public void UpdateCurrencyData(int currentBalance)
    {
        _balance = currentBalance;
    }
    public void UpdateNodeData(Vector2Int[] completedNodes, Vector2Int currentNodeIndex, int generalSeed, int nodeMapSeed)
    {
        _completedNodes = completedNodes;
        _curNodeIndex = currentNodeIndex;
        _generalSeed = generalSeed;
        _nodeMapSeed = nodeMapSeed;
    }
    public void UpdateNodeData(Vector2Int[] completedNodes)
    {
        _completedNodes = completedNodes;
    }
    public void UpdateNodeData(Vector2Int currentNodeIndex)
    {
        _curNodeIndex = currentNodeIndex;
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

    public void AddCoinFlip(bool result)
    {
        if (_coinFlipsThisRun == null) _coinFlipsThisRun = new();
        _coinFlipsThisRun.Add(result);
    }
    public void AddCoinFlip(bool[] results)
    {
        if (_coinFlipsThisRun == null) _coinFlipsThisRun = new();
        _coinFlipsThisRun.AddRange(results);
    }
    public void ClearRunCoinFlips()
    {
        _coinFlipsThisRun?.Clear();
    }

    public void AddDiceRoll(int result)
    {
        if (_dieRollsThisRun == null) _dieRollsThisRun = new();
        _dieRollsThisRun.Add(result);
    }
    public void AddDiceRoll(int[] results)
    {
        if (_dieRollsThisRun == null) _dieRollsThisRun = new();
        _dieRollsThisRun.AddRange(results);
    }
    public void ClearRunDiceRolls()
    {
        _dieRollsThisRun?.Clear();
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
            {
                var cardDef = _cardAndDeckLibrary.GetCardFromName(card);
                if (cardDef == null) continue;
                cards.Add(cardDef);
            }
            createdDecks.Add(new(deck.deckName, cards));
        }

        List<CardAbilityDefinition> ownedCards = new();
        foreach (var name in cardData.GetOwnedCardNames)
            ownedCards.Add(_cardAndDeckLibrary.GetCardFromName(name));

        UpdateCurrencyData(currencyData.GetBalance);
        UpdateNodeData(nodeData.GetCompletedNodes, nodeData.GetCurrentNodeIndex, nodeData.GetGeneralSeed, nodeData.GetNodeMapSeed);
        UpdateCardData(ownedCards, cardData.GetActiveDeckName, createdDecks);

        _coinFlipsThisRun = new();
        AddCoinFlip(specialMechanicData.GetCoinFlipsCurrentRun);
        _dieRollsThisRun = new();
        AddDiceRoll(specialMechanicData.GetDiceRollsCurrentRun);

        CurrencyManager.Instance?.OnBalanceChanged?.Invoke(_balance);
        //Debug.Log("Game Loaded");
    }
}
