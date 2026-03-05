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
    private CombatMapData _currCombatNodeData;
    private Reward _currNodeReward;

    [SerializeField] private List<CardPack> _createdPacks = new();
    [SerializeField] private Deck _deck;

    [SerializeField] private List<bool> _coinFlipsThisRun = new();
    [SerializeField] private List<int> _dieRollsThisRun = new();

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
    public List<CardPack> GetAllPlayerPacks => _createdPacks;

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
    public void UpdateCardData(Deck deck, List<CardPack> createdPacks)
    {
        _deck = deck;
        _createdPacks = createdPacks;
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
    public void CreateOrAdjustPack(CardPack pack)
    {
        if (pack == null) return;
        for (int i = _createdPacks.Count - 1; i >= 0; i--)
            if (_createdPacks[i].GetPackName == pack.GetPackName)
                _createdPacks.RemoveAt(i);
        _createdPacks.Add(pack);
        SaveLoadScript.SaveGame?.Invoke();
    }
    public void DeletePack(CardPack pack)
    {
        if (pack == null) return;
        for (int i = _createdPacks.Count - 1; i >= 0; i--)
            if (_createdPacks[i].GetPackName == pack.GetPackName)
                _createdPacks.RemoveAt(i);
        SaveLoadScript.SaveGame?.Invoke();
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

        List<CardPack> createdPacks = new();
        foreach (var pack in cardData.GetPlayerPacks)
        {
            List<CardAbilityDefinition> cards = new();
            foreach (var card in pack.cardNames)
            {
                var cardDef = _cardAndDeckLibrary.GetCardFromName(card);
                if (cardDef == null) continue;
                cards.Add(cardDef);
            }
            createdPacks.Add(new(pack.packName, cards));
        }

        List<CardAbilityDefinition> runDeck = new();
        foreach (var name in cardData.GetDeck)
            runDeck.Add(_cardAndDeckLibrary.GetCardFromName(name));

        UpdateCurrencyData(currencyData.GetBalance);
        UpdateNodeData(nodeData.GetCompletedNodes, nodeData.GetCurrentNodeIndex, nodeData.GetGeneralSeed, nodeData.GetNodeMapSeed);
        UpdateCardData(new Deck(runDeck), createdPacks);

        _coinFlipsThisRun = new();
        AddCoinFlip(specialMechanicData.GetCoinFlipsCurrentRun);
        _dieRollsThisRun = new();
        AddDiceRoll(specialMechanicData.GetDiceRollsCurrentRun);

        CurrencyManager.Instance?.OnBalanceChanged?.Invoke(_balance);
        //Debug.Log("Game Loaded");
    }
}
