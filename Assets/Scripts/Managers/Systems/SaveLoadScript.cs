using CardSystem;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public static class SaveLoadScript
{
    public static Action SaveGame => () => SaveData();
    public static Action LoadGame => () => LoadData();

    public static Action DataSaved;
    public static Action DataLoaded;

    private static string _filePath = Application.persistentDataPath + "DC_GameSave.json";
    public static string GetFilePath => _filePath;
    public static bool CheckForSaveGame => File.Exists(_filePath);

    private static void SaveData(bool pretty = false)
    {
        string json = JsonUtility.ToJson(new GameData(), pretty);
        StreamWriter sw = new StreamWriter(_filePath);
        sw.Write(json);
        sw.Close();
        DataSaved?.Invoke();
        Debug.Log("Game Saved:"+ _filePath);
    }

    private static void LoadData()
    {
        string json = string.Empty;
        StreamReader sr = new StreamReader(_filePath);
        json = sr.ReadToEnd();

        GameData _gameData = JsonUtility.FromJson<GameData>(json);
        PlayerDataManager.Instance?.OnGameLoad(_gameData);
        sr.Close();
    }
}

[System.Serializable]
public class GameData
{
    [SerializeField] private MapNodeDataToken _mapNodeData;
    [SerializeField] private CurrencyManagerDataToken _currencyData;
    [SerializeField] private CardDataToken _cardData;

    public MapNodeDataToken GetMapNodeData => _mapNodeData;
    public CurrencyManagerDataToken GetCurrencyData => _currencyData;
    public CardDataToken GetCardData => _cardData;

    public GameData()
    {
        var pdm = PlayerDataManager.Instance;

        _mapNodeData = new(pdm.GetNodeCompleted, pdm.GetNodeUnlocked, pdm.GetCurrentNodeIndex);
        _currencyData = new(pdm.GetBalance);
        _cardData = new(pdm.GetOwnedCards, pdm.GetDeck);
    }

    // node map vars
    [System.Serializable]
    public class MapNodeDataToken
    {
        [SerializeField] private bool[] _nodesCompleted;
        [SerializeField] private bool[] _nodesUnlocked;
        [SerializeField] private int _currentNodeIndex;

        public bool[] GetNodesCompleted => _nodesCompleted;
        public bool[] GetNodesUnlocked => _nodesUnlocked;
        public int GetCurrentNodeIndex => _currentNodeIndex;

        public MapNodeDataToken(bool[] nodesCompleted, bool[] nodesUnlocked, int currentNodeIndex)
        {
            _nodesCompleted = nodesCompleted;
            _nodesUnlocked = nodesUnlocked;
            _currentNodeIndex = currentNodeIndex;
        }
    }

    // currency
    [System.Serializable]
    public class CurrencyManagerDataToken
    {
        [SerializeField] private int _balance;
        public int GetBalance => _balance;

        public CurrencyManagerDataToken(int balance)
        {
            _balance = balance;
        }
    }

    // deck and card info
    [System.Serializable]
    public class CardDataToken
    {
        [SerializeField] private string[] _ownedCardNames;
        [SerializeField] private string _deckName;

        public string[] GetOwnedCardNames => _ownedCardNames;
        public string GetDeckName => _deckName;

        public CardDataToken(List<CardAbilityDefinition> ownedCards, Deck deck)
        {
            if (ownedCards != null)
            {
                _ownedCardNames = new string[ownedCards.Count];
                for (int i = 0; i < ownedCards.Count; i++)
                    _ownedCardNames[i] = ownedCards[i].name;
            }
            else
                _ownedCardNames = new string[0];

            _deckName = deck.name;
        }
    }
}
