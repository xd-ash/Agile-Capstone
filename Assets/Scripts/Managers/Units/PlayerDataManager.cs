using CardSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

// Data manager for player data that will be saved in game data
public class PlayerDataManager : MonoBehaviour
{
    private CardAndPackLibrary _cardAndPackLibrary;

    Dictionary<RestOptions, int> _buffsThisRun = new();

    private int _balance = 0;

    private Vector2Int[] _completedNodes;
    [SerializeField] private Vector2Int _curNodeIndex = new(0,0);
    private int _generalSeed = -1;
    private int _nodeMapSeed = -1;
    private CombatMapData _currCombatNodeData;
    private Reward _currNodeReward;

    [SerializeField] private Deck _deck;

    [SerializeField] private List<bool> _coinFlipsThisRun = new();
    [SerializeField] private List<int> _dieRollsThisRun = new();

    public Dictionary<RestOptions, int> GetBuffsThisRun => _buffsThisRun;
    public int GetMaxAPBuff => _buffsThisRun.ContainsKey(RestOptions.AP) ? _buffsThisRun[RestOptions.AP] : 0;
    public int GetMaxHealthBuff => _buffsThisRun.ContainsKey(RestOptions.MaxHealth) ? _buffsThisRun[RestOptions.MaxHealth] : 0;
    public int GetStartingHandSizeBuff => _buffsThisRun.ContainsKey(RestOptions.StartingHandSize) ? _buffsThisRun[RestOptions.StartingHandSize] : 0;
    public int[] GetAllBuffs => new int[3] { GetMaxAPBuff, GetMaxHealthBuff, GetStartingHandSizeBuff };

    public int GetBalance => _balance;

    public Vector2Int[] GetCompletedNodes => _completedNodes;
    public Vector2Int GetCurrentNodeIndex => _curNodeIndex;
    public int GetGeneralSeed => _generalSeed == -1 ? GenerateRandomSeed(ref _generalSeed) : _generalSeed;
    public int GetNodeMapSeed => _nodeMapSeed == -1 ? GenerateRandomSeed(ref _nodeMapSeed) : _nodeMapSeed;
    public int GenerateGeneralSeed() => GenerateRandomSeed(ref _generalSeed);
    public int GenerateNodeMapSeed() => GenerateRandomSeed(ref _nodeMapSeed);
    public CombatMapData GetCurrCombatNodeData => _currCombatNodeData;
    public Reward GetCurrNodeReward => _currNodeReward;

    public Deck GetPlayerDeck => _deck;

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
        
        if (_cardAndPackLibrary == null)
            _cardAndPackLibrary = Resources.Load<CardAndPackLibrary>("Libraries/CardAndPackLibrary");

    #if UNITY_EDITOR
        CardAndPackLibrary.GrabAssets?.Invoke();
        BadgeLibrary.GrabAssets?.Invoke();
    #endif

        if (SaveLoadScript.CheckForSaveGame)
            SaveLoadScript.LoadGame?.Invoke();
        else
            SaveLoadScript.CreateNewGame?.Invoke();

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
    public void UpdateBuff(RestOptions option, int buffAmount)
    {
        if (_buffsThisRun.ContainsKey(option))
            _buffsThisRun[option] += buffAmount;
        else
            _buffsThisRun.Add(option, buffAmount);
    }
    public void ClearOptionBuffFromDict(RestOptions option)
    {
        if (!_buffsThisRun.ContainsKey(option)) return;
        _buffsThisRun.Remove(option);
    }
    public void ClearBuffsOnRunEnd()
    {
        _buffsThisRun.Clear();
    }

    public void UpdateCurrencyData(int currentBalance)
    {
        _balance = currentBalance;
    }
    public void AddChips(int amount)
    {
        _balance += amount;
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
    public void UpdateCardData(Deck deck)
    {
        _deck = deck;
    }
    public void UpdateCardData(Card card, bool isAddition = true)
    {
        if (card == null) return;

        if (isAddition)
            _deck.AddCard(card);
        else
            if (_deck.Contains(card))
                _deck.RemoveCard(card);
    }
    public void SetCurrMapNodeData(CombatMapData currMapNodeData)
    {
        _currCombatNodeData = currMapNodeData;
    }

    public void SetCurrNodeReward(Reward reward)
    {
        _currNodeReward = reward;
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

        List<Card> runCards = new();
        foreach (var name in cardData.GetDeck)
        {
            var cardInfo = Card.ReadNamingConventionString(name);
            Card newCard = new(_cardAndPackLibrary.GetCardFromName(cardInfo.Item2), cardInfo.Item1, null);
            runCards.Add(newCard);
        }
        var runDeck = new Deck(runCards);

        _buffsThisRun.Clear();
        for (var i = 0; i < specialMechanicData.GetBuffsCurrentRun.Length; i++)
            UpdateBuff((RestOptions)i, specialMechanicData.GetBuffsCurrentRun[i]);

        UpdateCurrencyData(currencyData.GetBalance);
        UpdateNodeData(nodeData.GetCompletedNodes, nodeData.GetCurrentNodeIndex, nodeData.GetGeneralSeed, nodeData.GetNodeMapSeed);
        UpdateCardData(runDeck);

        _coinFlipsThisRun = new();
        AddCoinFlip(specialMechanicData.GetCoinFlipsCurrentRun);
        _dieRollsThisRun = new();
        AddDiceRoll(specialMechanicData.GetDiceRollsCurrentRun);

        CurrencyManager.Instance?.OnBalanceChanged?.Invoke(_balance);
        //Debug.Log("Game Loaded");
    }
    private List<CardPack> CreatePacksFromNames(GameData.PackToken[] packTokens)
    {
        List<CardPack> packs = new();
        foreach (var pack in packTokens)
        {
            List<CardAbilityDefinition> cards = new();
            foreach (var card in pack.cardNames)
            {
                var cardDef = _cardAndPackLibrary.GetCardFromName(card);
                if (cardDef == null) continue;
                cards.Add(cardDef);
            }
            packs.Add(new(pack.packName, cards));
        }
        return packs;
    }
}
