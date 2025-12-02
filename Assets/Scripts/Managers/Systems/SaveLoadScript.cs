using CardSystem;
using System;
using System.IO;
using UnityEngine;

public static class SaveLoadScript
{
    public static Action SaveGame => () => SaveData();
    public static Action LoadGame => () => LoadData();

    private static void SaveData()
    {
        string json = JsonUtility.ToJson(new GameData());
        StreamWriter sw = new StreamWriter(Application.persistentDataPath +
            "DC_GameSave.json");
        sw.Write(json);
        sw.Close();
    }

    private static void LoadData()
    {
        string json = string.Empty;
        StreamReader sr = new StreamReader(Application.persistentDataPath +
            "DC_GameSave.json");
        json = sr.ReadToEnd();

        GameData _gameData = JsonUtility.FromJson<GameData>(json);
        _gameData.LoadGameData();
        sr.Close();
    }
}

[System.Serializable]
public class GameData
{
    public MapNodeDataToken mapNodeData;
    public CurrencyManagerDataToken currencyData;
    //public DeckDataToken deckData;

    public GameData()
    {
        mapNodeData = new();
        currencyData = new();
        //deckData = new();
    }

    public void LoadGameData()
    {
        mapNodeData.LoadData();
        currencyData.LoadData();
        //deckData.LoadData();
    }

    // node map vars
    [System.Serializable]
    public class MapNodeDataToken
    {
        private bool[] _nodeCompleted;
        private bool[] _nodeUnlocked;
        private int _currentNodeIndex;

        public MapNodeDataToken()
        {
            SceneProgressManager.Instance.GrabNodeData( ref _nodeCompleted, 
                ref _nodeUnlocked, ref _currentNodeIndex);
        }
        public void LoadData()
        {
            SceneProgressManager.Instance.LoadNodeData(_nodeCompleted, 
                _nodeUnlocked, _currentNodeIndex);
        }
    }

    // currency
    [System.Serializable]
    public class CurrencyManagerDataToken
    {
        private int _balance;

        public CurrencyManagerDataToken()
        {
            if (CurrencyManager.instance != null)
                _balance = CurrencyManager.instance.Balance;
        }
        public void LoadData()
        {
            if (CurrencyManager.instance != null)
                CurrencyManager.instance.LoadGameData(_balance);
        }
    }

    // deck and card info
    [System.Serializable]
    public class DeckDataToken
    {
        private string[] _runtimeAddedCardNames;

        public DeckDataToken()
        {
            var runtimeDefs = CardManager.instance.runtimeAddedDefinitions;
            _runtimeAddedCardNames = new string[runtimeDefs.Count];
            for (int i = 0; i < runtimeDefs.Count; i++)
                _runtimeAddedCardNames[i] = runtimeDefs[i].name;
        }
        public void LoadData()
        {

        }
    }
}
