using CardSystem;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    private int _balance = 0;

    private bool[] _nodeCompleted;
    private bool[] _nodeUnlocked;
    private int _currentNodeIndex;

    private List<CardAbilityDefinition> _ownedCards = new();
    [SerializeField] private Deck _deck;

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
    }
    public void UpdateCurrencyData(int currentBalance)
    {
        _balance = currentBalance;
    }
    public void UpdateNodeData(bool[] nodesCompleted, bool[] nodesUnlocked, int currentNodeIndex)
    {
        _nodeCompleted = nodesCompleted;
        _nodeUnlocked = nodesUnlocked;
        _currentNodeIndex = currentNodeIndex;
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
    public void OnGameLoad(GameData data)
    {
        var currencyData = data.GetCurrencyData;
        var nodeData = data.GetMapNodeData;
        var cardData = data.GetCardData;

        List<CardAbilityDefinition> ownedCards = new();
        foreach (var name in cardData.GetOwnedCardNames)
            ownedCards.Add(GetCardDefinitionFromName(name));
        Deck deck = GetDeckFromName(cardData.GetDeckName);

        UpdateCurrencyData(currencyData.GetBalance);
        UpdateNodeData(nodeData.GetNodesCompleted, nodeData.GetNodesUnlocked, nodeData.GetCurrentNodeIndex);
        UpdateCardData(ownedCards, deck);

        SaveLoadScript.DataLoaded?.Invoke();
        Debug.Log("Game Loaded");
    }
    private CardAbilityDefinition GetCardDefinitionFromName(string cardName)
    {
        string[] cardGUID = AssetDatabase.FindAssets(cardName, new[] { "Assets/ScriptableObjects/CardAbilities" });
        if (cardGUID.Length != 1)
        {
            Debug.LogError($"{cardGUID.Length} Card GUID matches for cardName on data load. ({cardName})");
            return null;
        }
        string cardPath = AssetDatabase.GUIDToAssetPath(cardGUID[0]);
        return AssetDatabase.LoadAssetAtPath<CardAbilityDefinition>(cardPath);
    }
    private Deck GetDeckFromName(string deckName)
    {
        string[] deckGUID = AssetDatabase.FindAssets(deckName, new[] { "Assets/ScriptableObjects/DeckSOs" });
        if (deckGUID.Length != 1)
        {
            Debug.LogError($"{deckGUID.Length} Deck GUID matches for decckNAme on data load. ({deckName})");
            return null;
        }
        string deckPath = AssetDatabase.GUIDToAssetPath(deckGUID[0]);
        return AssetDatabase.LoadAssetAtPath<Deck>(deckPath);
    }
}
