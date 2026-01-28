using CardSystem;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class SaveLoadScript
{
    public static Action SaveGame => () => SaveData();
    public static Action LoadGame => () => LoadData();

    public static Action DataSaved;
    public static Action DataLoaded;

    private static void SaveData()
    {
        string json = JsonUtility.ToJson(new GameData());
        StreamWriter sw = new StreamWriter(Application.persistentDataPath + "DC_GameSave.json");
        sw.Write(json);
        sw.Close();
        DataSaved?.Invoke();
        Debug.Log("Game Saved");
    }

    private static void LoadData()
    {
        string json = string.Empty;
        StreamReader sr = new StreamReader(Application.persistentDataPath + "DC_GameSave.json");
        json = sr.ReadToEnd();

        GameData _gameData = JsonUtility.FromJson<GameData>(json);
        //_gameData.LoadGameData();
        PlayerDataManager.Instance?.OnGameLoad(_gameData);
        sr.Close();
        Debug.Log("Game Loaded");
    }
}

[System.Serializable]
public class GameData
{
    private MapNodeDataToken _mapNodeData;
    private CurrencyManagerDataToken _currencyData;
    private CardDataToken _cardData;

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

    /*public void LoadGameData()
    {
        _mapNodeData.LoadData();
        _currencyData.LoadData();
        _cardData.LoadData();
    }*/

    // node map vars
    [System.Serializable]
    public class MapNodeDataToken
    {
        private bool[] _nodesCompleted;
        private bool[] _nodesUnlocked;
        private int _currentNodeIndex;

        public bool[] GetNodesCompleted => _nodesCompleted;
        public bool[] GetNodesUnlocked => _nodesUnlocked;
        public int GetCurrentNodeIndex => _currentNodeIndex;

        public MapNodeDataToken(bool[] nodesCompleted, bool[] nodesUnlocked, int currentNodeIndex)
        {
            //SceneProgressManager.Instance.GrabNodeData( ref _nodeCompleted, 
            //ref _nodeUnlocked, ref _currentNodeIndex);
            _nodesCompleted = nodesCompleted;
            _nodesUnlocked = nodesUnlocked;
            _currentNodeIndex = currentNodeIndex;
        }
        /*public void LoadData()
        {
            //SceneProgressManager.Instance.LoadNodeData(_nodeCompleted, 
            //_nodeUnlocked, _currentNodeIndex);
        }*/
    }

    // currency
    [System.Serializable]
    public class CurrencyManagerDataToken
    {
        private int _balance;
        public int GetBalance => _balance;

        public CurrencyManagerDataToken(int balance)
        {
            //if (CurrencyManager.instance != null)
            //_balance = CurrencyManager.instance.Balance;
            _balance = balance;
        }
        /*public void LoadData()
        {
            //CurrencyManager.instance?.LoadGameData(_balance);
            PlayerDataManager.Instance?.UpdateCurrencyData(_balance);
        }*/
    }

    // deck and card info
    [System.Serializable]
    public class CardDataToken
    {
        private string[] _ownedCardNames;
        private string _deckName;

        public string[] GetOwnedCardNames => _ownedCardNames;
        public string GetDeckName => _deckName;

        public CardDataToken(List<CardAbilityDefinition> ownedCards, Deck deck)
        {
            _ownedCardNames = new string[ownedCards.Count];
            for (int i = 0; i < ownedCards.Count; i++)
                _ownedCardNames[i] = ownedCards[i].name;

            _deckName = deck.name;
        }
        /*public void LoadData()
        {
            List<CardAbilityDefinition> ownedCards = new();
            foreach (var name in _ownedCardNames)
                ownedCards.Add(GetCardDefinitionFromName(name));
            //PlayerCardCollection.instance.LoadGameData(ownedCards);
            //DeckAndHandManager.instance.LoadGameData(GetDeckFromName(_deckName));

            PlayerDataManager.Instance?.UpdateCardData(ownedCards, GetDeckFromName(_deckName));
        }*/

        //Make these generic at some point & combine
    }
}
