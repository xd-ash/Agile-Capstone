using CardSystem;
using System.Collections.Generic;
using UnityEngine;

// Data manager for player data that will be saved in game data
public class PlayerDataManager : MonoBehaviour
{
    private int _balance = 0;

    private bool[] _nodeCompleted;
    private bool[] _nodeUnlocked;
    [SerializeField]private int _currentNodeIndex;

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

        if (SaveLoadScript.CheckForSaveGame)
            SaveLoadScript.LoadGame?.Invoke();
    }

    // Data update methods for setting values
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

    // On game load, update variable values using incoming data param and
    // reinitialize node data for proper node enabling on node map
    public void OnGameLoad(GameData data)
    {
        var currencyData = data.GetCurrencyData;
        var nodeData = data.GetMapNodeData;
        var cardData = data.GetCardData;

        Deck deck = GetDeckFromName(cardData.GetDeckName);
        List<CardAbilityDefinition> ownedCards = new();
        foreach (var name in cardData.GetOwnedCardNames)
            ownedCards.Add(GetCardDefinitionFromName(name));

        UpdateCurrencyData(currencyData.GetBalance);
        UpdateNodeData(nodeData.GetNodesCompleted, nodeData.GetNodesUnlocked, nodeData.GetCurrentNodeIndex);
        UpdateCardData(ownedCards, deck);

        SceneProgressManager.Instance?.InitNodeData();
        //Debug.Log("Game Loaded");
    }

    // Grab card using card name
    // this may need adjusting as it only searches cards in the current deck
    // so if player owns cards that arent in the base deck (bought in shop),
    // then the card won't be found
    private CardAbilityDefinition GetCardDefinitionFromName(string cardName)
    {
        foreach (var card in _deck.GetDeck)
            if (card.name == cardName)
                return card;

        Debug.LogError($"No matching card SO found in deck for \"{cardName}\"");
        return null;
    }

    // GrabDeck from resources folder
    private Deck GetDeckFromName(string deckName)
    {
        return Resources.Load<Deck>("ScriptableObjects/DeckSOs/" + deckName);
    }
}
