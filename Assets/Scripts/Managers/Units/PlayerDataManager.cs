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

    private List<CardAbilityDefinition> _ownedCards = new();
    [SerializeField] private Deck _deck;
    
    public int GetSeed => _randomSeed == -1 ? GetRandomSeed() : _randomSeed;
    public CombatMapData GetCurrMapNodeData => _currMapNodeData;
    public int GetBalance => _balance;
    public bool[] GetNodeCompleted => _nodeCompleted;
    public bool[] GetNodeUnlocked => _nodeUnlocked;
    public int GetCurrentNodeIndex => _currentNodeIndex;
    public List<CardAbilityDefinition> GetOwnedCards => _ownedCards;
    public Deck GetDeck => _deck;

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

        if (SaveLoadScript.CheckForSaveGame)
            SaveLoadScript.LoadGame?.Invoke();
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
        //CurrencyManager.Instance?.OnBalanceChanged?.Invoke(currentBalance);
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
    public void UpdateCardData(List<CardAbilityDefinition> ownedCards, Deck deck)
    {
        _ownedCards = ownedCards;
        _deck = deck;
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
    public void SetCurrMapNodeData(CombatMapData currMapNodeData)
    {
        _currMapNodeData = currMapNodeData;
    }
    // On game load, update variable values using incoming data param and
    // reinitialize node data for proper node enabling on node map
    public void OnGameLoad(GameData data)
    {
        if (_cardAndDeckLibrary == null)
            _cardAndDeckLibrary = Resources.Load<CardAndDeckLibrary>("CardAndDeckLibrary");

        var currencyData = data.GetCurrencyData;
        var nodeData = data.GetMapNodeData;
        var cardData = data.GetCardData;

        Deck deck = _cardAndDeckLibrary.GetDeckFromName(cardData.GetDeckName);
        List<CardAbilityDefinition> ownedCards = new();
        foreach (var name in cardData.GetOwnedCardNames)
            ownedCards.Add(_cardAndDeckLibrary.GetCardFromName(name));

        UpdateCurrencyData(currencyData.GetBalance);
        UpdateNodeData(nodeData.GetNodesCompleted, nodeData.GetNodesUnlocked, nodeData.GetCurrentNodeIndex, nodeData.GetSeed);
        UpdateCardData(ownedCards, deck);

        SceneProgressManager.Instance?.InitNodeData();
        CurrencyManager.Instance?.OnBalanceChanged?.Invoke(_balance);
        //Debug.Log("Game Loaded");
    }
}
